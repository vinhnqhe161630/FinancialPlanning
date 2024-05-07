using FinancialPlanning.Common;
using FinancialPlanning.Data.Data;
using FinancialPlanning.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FinancialPlanning.Data.Repositories
{
    public class PlanRepository : IPlanRepository
    {
        private readonly DataContext _context;

        public PlanRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreatePlan(Plan plan)
        {
            plan.Id = Guid.NewGuid();
            _context.Plans!.Add(plan);
            await _context.SaveChangesAsync();
            return plan.Id;
        }

        public async Task DeletePlan(Plan plan)
        {
            // Load các PlanVersions của Plan cần xóa để đảm bảo rằng các đối tượng này được theo dõi bởi DbContext
            await _context.Entry(plan)
                .Collection(p => p.PlanVersions)
                .LoadAsync();

            // Xóa tất cả các PlanVersions của Plan
            _context.PlanVersions!.RemoveRange(plan.PlanVersions);

            // Xóa Plan chính thức
            _context.Plans!.Remove(plan);

            // Ghi lại thay đổi
            await _context.SaveChangesAsync();
        }

        public async Task<List<Plan>> GetAllPlans()
        {
            return await _context.Plans!.ToListAsync();
        }

        public async Task<List<Plan>> GetPlanByDepartId(Guid departmentId)
        {
            return await _context.Plans!.Where(p => p.DepartmentId == departmentId).ToListAsync();
        }

        public async Task<Plan?> GetPlanById(Guid id)
        {
            var plan = await _context.Plans!
                .Include(p => p.Term)
                .Include(p => p.Department)
                .Include(p => p.PlanVersions)
                .FirstOrDefaultAsync(p => p.Id == id);
            return plan;
        }

        public async Task<List<Plan>> GetPlans(Guid? termId, Guid? departmentId)
        {
            // Kiểm tra xem termId hoặc departmentId có null không
            if (termId == null && departmentId == null)
            {
                // Trong trường hợp cả hai là null, không thực hiện tìm kiếm, trả về danh sách rỗng hoặc xử lý một cách phù hợp
                return await _context.Plans!.ToListAsync();

            }

            // Sử dụng termId hoặc departmentId (nếu có) để truy vấn danh sách các kế hoạch từ cơ sở dữ liệu
            var query = _context.Plans!.AsQueryable();

            if (termId != null)
            {
                query = query.Where(p => p.TermId == termId);
            }

            if (departmentId != null)
            {
                query = query.Where(p => p.DepartmentId == departmentId);
            }

            // Thực hiện truy vấn và trả về danh sách kế hoạch
            return await query.ToListAsync();

        }

        public async Task<Plan> SavePlan(Plan plan, Guid creatorId)
        {
            var existingPlan = await _context.Plans!
                .Include(p => p.Term)
                .Include(p => p.Department)
                .Include(p => p.PlanVersions)
                .FirstOrDefaultAsync(p => p.TermId == plan.TermId && p.DepartmentId == plan.DepartmentId);
            if (existingPlan != null)
            {
                var newVersion = _context.PlanVersions!.Count(pv => pv.PlanId == existingPlan.Id) + 1;
                var planVersion = new PlanVersion
                {
                    Id = Guid.NewGuid(),
                    PlanId = existingPlan.Id,
                    Version = newVersion,
                    CreatorId = creatorId,
                    ImportDate = DateTime.UtcNow
                };

                existingPlan.ApprovedExpenses = plan.ApprovedExpenses;
                _context.PlanVersions!.Add(planVersion);
                await _context.SaveChangesAsync();

                return existingPlan;
            }
            else
            {
                plan.Status = (int)PlanStatus.New;
                plan.Id = Guid.NewGuid();
                plan.ApprovedExpenses = "[]";

                var planVersion = new PlanVersion
                {
                    Id = Guid.NewGuid(),
                    PlanId = plan.Id,
                    Version = 1,
                    CreatorId = creatorId,
                    ImportDate = DateTime.UtcNow
                };

                plan.PlanVersions = new List<PlanVersion> { planVersion };

                _context.Plans!.Add(plan);
                _context.PlanVersions!.Add(planVersion);

                await _context.SaveChangesAsync();

                return _context.Plans!
                    .Include(p => p.Term)
                    .Include(p => p.Department).FirstOrDefault(p => p.Id == plan.Id)!;
            }
        }

        public async Task<Plan> ImportPlan(Plan plan, Guid userId)
        {
            plan.Id = new Guid();
            plan.ApprovedExpenses = "[]";
            _context.Plans!.Add(plan);
            PlanVersion planVersion = new()
            {
                Id = Guid.NewGuid(),
                PlanId = plan.Id,
                Version = 1,
                ImportDate = DateTime.Now,
                CreatorId = userId,
            };
            _context.PlanVersions!.Add(planVersion);
            await _context.SaveChangesAsync();
            return GetPlanById(plan.Id).Result!;
        }

        public async Task<Plan> ReupPlan(Plan plan, Guid userId)
        {
            var nextversion = plan!.PlanVersions.Count + 1;
            var planVersion = new PlanVersion
            {
                Id = Guid.NewGuid(),
                PlanId = plan.Id,
                Version = nextversion,
                ImportDate = DateTime.Now,
                CreatorId = userId,
            };
            _context.Plans!.Update(plan);
            _context.PlanVersions!.Add(planVersion);

            await _context.SaveChangesAsync();

            return GetPlanById(plan.Id).Result!;
        }

        public async Task UpdatePlan(Plan plan)
        {
            _context.Plans!.Update(plan);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Plan>> GetFinancialPlans(string keyword = "", string department = "", string status = "")
        {

            IQueryable<Plan> plans = _context.Plans!
                .Include(p => p.Term)
                .Include(p => p.Department)
                .Include(p => p.PlanVersions);

            // Lọc kế hoạch tài chính dựa trên từ khóa, phòng ban và trạng thái
            if (!string.IsNullOrEmpty(keyword))
                plans = plans.Where(p => p.PlanName.Contains(keyword, StringComparison.CurrentCultureIgnoreCase));

            if (!string.IsNullOrEmpty(department))
                plans = plans.Where(p => string.Equals(p.Department.DepartmentName.ToLower(), department.ToLower(),
                    StringComparison.Ordinal));

            if (!string.IsNullOrEmpty(status))
            {
                plans = status switch
                {
                    "New" => plans.Where(p => p.Status == (int)PlanStatus.New),
                    "Waiting for Approval" => plans.Where(p => p.Status == PlanStatus.WaitingForApproval),
                    "Approved" => plans.Where(p => p.Status == PlanStatus.Approved),
                    _ => plans,
                };
            }

            // Sắp xếp theo trạng thái và sau đó theo StartDate trong mỗi trạng thái
            plans = plans.OrderByDescending(p => p.Status)
                .ThenBy(p => p.Term.StartDate);

            return await plans.ToListAsync();
        }

        public async Task<List<Plan>> GetAllDuePlans()
        {
            return await _context.Plans!
                .Include(p => p.Term)
                .Where(p => p.Term.PlanDueDate < DateTime.UtcNow && p.Status == (int)PlanStatus.New)
                .ToListAsync();
        }

        public async Task CloseAllDuePlans(List<Plan> plans)
        {
            foreach (var plan in plans)
            {
                if (plan.Status == PlanStatus.New)
                {
                    plan.Status = PlanStatus.Closed;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<PlanVersion>> GetPlanVersionsByPlanID(Guid planId)
        {
            var planVersions = await _context.PlanVersions!
                .Where(r => r.PlanId == planId)
                .OrderByDescending(r => r.Version)
                .Include(r => r.User).ToListAsync();
            return planVersions;
        }

        public async Task<bool> IsPlanExist(Guid termId, Guid departmentId)
        {
            var plan = await _context.Plans!
                .FirstOrDefaultAsync(p => p.TermId == termId && p.DepartmentId == departmentId);

            return plan != null;
        }

        public async Task UpdatePlanStatus(Guid id, PlanStatus status)
        {
            var plan = await _context.Plans!.FindAsync(id);

            if (plan == null)
            {
                throw new Exception("Plan not found.");
            }

            plan.Status = status;
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePlanApprovedExpenses(Guid id, string planApprovedExpenses)
        {
            var plan = await _context.Plans!.FindAsync(id);

            if (plan == null)
            {
                throw new Exception("Plan not found.");
            }

            plan.ApprovedExpenses = planApprovedExpenses;
            await _context.SaveChangesAsync();
        }
    }
}