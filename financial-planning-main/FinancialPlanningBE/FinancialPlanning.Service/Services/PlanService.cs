using System.Numerics;
using System.Text.Json;
using FinancialPlanning.Common;
using FinancialPlanning.Data.Entities;
using FinancialPlanning.Data.Repositories;
using Serilog;
namespace FinancialPlanning.Service.Services
{
    public class PlanService
    {
        private readonly FileService _fileService;
        private readonly IPlanRepository _planRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly ITermRepository _termService;

        public PlanService(IPlanRepository planRepository, FileService fileService, IDepartmentRepository departmentRepository, ITermRepository termRepository)
        {
            _planRepository = planRepository ?? throw new ArgumentNullException(nameof(planRepository));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
            _termService = termRepository ?? throw new ArgumentNullException(nameof(termRepository));
        }

        public string ValidatePlanFile(byte[] file)
        {
            // Validate the file using FileService
            try
            {
                // Assuming plan documents have document type 0
                return _fileService.ValidateFile(file, documentType: 0);
            }
            catch (Exception ex)
            {
                // Log exception
                throw new InvalidOperationException("An error occurred while validating the plan file.", ex);
            }
        }

        public async Task<IEnumerable<Plan>> GetStartingPlans()
        {
            IEnumerable<Plan> plans = await _planRepository.GetAllPlans();
            List<Plan> startingPlans = [];
            foreach (var plan in plans)
            {
                {
                    startingPlans.Add(plan);
                }
            }
            return startingPlans;
        }

        public async Task<Plan?> GetPlanById(Guid id)
        {
            return await _planRepository.GetPlanById(id);
        }

        // public async Task CreatePlan(Plan plan)
        // {
        //     await _planRepository.CreatePlan(plan);
        // }

        public async Task<List<Plan>> GetFinancialPlans(string keyword = "", string department = "", string status = "")
        {
            return await _planRepository.GetFinancialPlans(keyword, department, status);
        }

        public async Task DeletePlan(Guid id)
        {
            var planToDelete = await _planRepository.GetPlanById(id);

            // Delete in S3
            foreach (var version in planToDelete.PlanVersions!)
            {
                var filename = planToDelete.Department.DepartmentName + '/' + planToDelete.Term.TermName + "/Plan/version_" + version.Version + ".xlsx";
                //delete file on cloud
                await _fileService.DeleteFileAsync(filename);
            }
            if (planToDelete != null)
            {
                await _planRepository.DeletePlan(planToDelete);
            }
            else
            {
                throw new ArgumentException("Plan not found with the specified ID");
            }
        }


        public async Task CloseDuePlans()
        {
            var plans = await _planRepository.GetAllDuePlans();
            await _planRepository.CloseAllDuePlans(plans);
            Log.Information("Closed {Count} due plans.", plans.Count());
        }

        public List<Expense> GetExpenses(byte[] file)
        {
            try
            {
                // Convert the file to a list of expenses using FileService
                var expenses = _fileService.ConvertExcelToList(file, documentType: 0);
                return expenses;
            }
            catch (Exception ex)
            {
                // Log exception
                throw new InvalidOperationException("An error occurred while importing the plan file.", ex);
            }
        }

        public async Task<IEnumerable<PlanVersion>> GetPlanVersionsAsync(Guid planId)
        {
            var planVersions = await _planRepository.GetPlanVersionsByPlanID(planId);
            return planVersions;
        }

        public async Task CreatePlan(List<Expense> expenses, Guid termId, Guid uid)

        {
            var department = _departmentRepository.GetDepartmentByUserId(uid);
            var term = _termService.GetTermById(termId);
            var isPlanExist = await _planRepository.IsPlanExist(term.Id, department.Id);
            if (isPlanExist)
            {
                throw new ArgumentException("Plan already exists with the specified term, department");
            }
            else
            {
                var planDueDate = term.PlanDueDate;
                if (DateTime.Now > planDueDate)
                {
                    throw new ArgumentException("Plan due date has passed");
                }
                var plan = new Plan
                {
                    DepartmentId = department.Id,
                    TermId = term.Id,
                    PlanName = department.DepartmentName + "_" + term.TermName+"_Plan",
                    Status = PlanStatus.New
                };
                plan = await _planRepository.ImportPlan(plan, uid);

                var filename = Path.Combine(department.DepartmentName, plan.Term.TermName, "Plan", "version_1" + ".xlsx");
                // Convert list of expenses to Excel file                        
                var excelFileStream = await _fileService.ConvertListToExcelAsync(expenses, 0);
                // Upload the file to AWS S3
                await _fileService.UploadFileAsync(filename.Replace('\\', '/'), new MemoryStream(excelFileStream));
            }
        }

        public async Task ReupPlan(List<Expense> expenses, List<int> approvedNos, Guid planId, Guid uid)
        {
            var plan = await _planRepository.GetPlanById(planId);
            
            if (plan == null)
            {
                throw new ArgumentException("Plan not found with the specified ID");
            }

            var department = plan!.Department;

            plan.ApprovedExpenses = JsonSerializer.Serialize(approvedNos);

            plan = await _planRepository.ReupPlan(plan, uid);

            var filename = Path.Combine(department.DepartmentName, plan.Term.TermName, "Plan", "version_" + plan.PlanVersions.Count + ".xlsx");
            // Convert list of expenses to Excel file                        
            var excelFileStream = await _fileService.ConvertListToExcelAsync(expenses, 0);
            // Upload the file to AWS S3
            await _fileService.UploadFileAsync(filename.Replace('\\', '/'), new MemoryStream(excelFileStream));
        }

        public List<int> CheckExpenses(List<Expense> expenses, Guid planId)
        {
            try {
                var plan = _planRepository.GetPlanById(planId).Result ?? throw new ArgumentException("Plan not found with the specified ID");

                string planExpenses = plan.ApprovedExpenses;

                var approvedExpenses = JsonSerializer.Deserialize<List<int>>(planExpenses);

                if (approvedExpenses!.Count == 0)
                {
                    return [];
                }

                List<int> checkedExpenses = [];

                string filename = Path.Combine(plan.Department.DepartmentName, plan.Term.TermName, "Plan", "version_" + plan.PlanVersions.Count + ".xlsx");

                var filebytes = _fileService.GetFileAsync(filename.Replace('\\', '/')).Result;
                var currentExpenses = _fileService.ConvertExcelToList(filebytes, 0);

                foreach (var expense in approvedExpenses)
                {
                    if (currentExpenses.Any(e => e.No == expense))
                    {
                        var checker = currentExpenses.FirstOrDefault(e => e.No == expense);
                        if (checker!.Equals(expenses.FirstOrDefault(e => e.No == expense)))
                        {
                            checkedExpenses.Add(expense);
                        }
                    }
                }

                return checkedExpenses;

            } catch (Exception ex)
            {
                throw new Exception("An error occurred while checking the expenses", ex);
            }
        }

        public async Task SubmitPlan(Guid planId)
        {
            var plan = await _planRepository.GetPlanById(planId);
            if (plan == null)
            {
                throw new ArgumentNullException("Plan not found with the specified ID");
            }
            if (plan.Status == PlanStatus.New)
            {
                plan.Status = PlanStatus.WaitingForApproval;
                await _planRepository.UpdatePlan(plan);
            }
            else
            {
                throw new ArgumentException("Plan is not in the correct status to be submitted");
            }
        }

        public async Task ApprovePlan(Guid planId)
        {
            var plan = await _planRepository.GetPlanById(planId);
            if (plan == null)
            {
                throw new ArgumentNullException("Plan not found with the specified ID");
            }
            if (plan.Status == PlanStatus.WaitingForApproval)
            {
                plan.Status = PlanStatus.Approved;
                await _planRepository.UpdatePlan(plan);
            }
            else
            {
                throw new ArgumentException("Plan is not in the correct status to be approved");
            }
        }

        public async Task UpdatePlanApprovedExpenses(Guid id, string planApprovedExpenses)
        {
            await _planRepository.UpdatePlanApprovedExpenses(id, planApprovedExpenses);
        }
    }

}
