using FinancialPlanning.Data.Data;
using FinancialPlanning.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinancialPlanning.Data.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly DataContext _context;

        public ReportRepository(DataContext context)
        {
            this._context = context;
        }

        //Get all reports
        public async Task<List<Report>> GetAllReports()
        {
            var reports = new List<Report>();
            if (_context.Reports != null)
            {
                reports = await _context.Reports
                    .OrderBy(r => r.Status)
                    .ThenByDescending(r => r.UpdateDate)
                    .Include(t => t.ReportVersions)
                    .Include(t => t.Term)
                    .Include(t => t.Department).ToListAsync();
            }

            return reports;
        }

        //get report by department ID
        public async Task<List<Report>> GetReportsByDepartId(Guid departId)
        {
            var reports = await _context.Reports!
                .Where(r => r.DepartmentId == departId)
                .OrderBy(r => r.Status) // order by status
                .ThenByDescending(r => r.UpdateDate)
                .Include(t => t.ReportVersions)
                .Include(t => t.Term)
                .Include(t => t.Department)
                .ToListAsync();

            return reports;
        }

        //Delete report
        public async Task DeleteReport(Report report)
        {
            _context.Reports!.Remove(report);
            await _context.SaveChangesAsync();
        }

        //Delete report Version
        public async Task DeleteReportVersions(IEnumerable<ReportVersion> reportVersions)
        {
            foreach (var reportVersion in reportVersions)
            {
                _context.ReportVersions!.Remove(reportVersion);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Report?> GetReportById(Guid id)
        {
            var report = await _context.Reports!
                .Include(t => t.ReportVersions)
                .Include(t => t.Term)
                .Include(t => t.Department)
                .FirstOrDefaultAsync(t => t.Id == id);

            return report;
        }

        //Get list reportVersions 
        public async Task<List<ReportVersion>> GetReportVersionsByReportID(Guid reportId)
        {
            var reportVersions = await _context.ReportVersions!
                .Where(r => r.ReportId == reportId)
                .OrderByDescending(r => r.Version)
                .Include(r => r.User).ToListAsync();

            return reportVersions;
        }

        //Check report exist
        public async Task<bool> IsReportExist(Guid termId, Guid departmentId, string month)
        {
            var report = await _context.Reports!
                .Where(r => r.TermId == termId && r.DepartmentId == departmentId && r.Month == month).FirstOrDefaultAsync();

            return report != null;
        }

        //Create report
        public async Task<Report> CreateReport(Report report, Guid userId)
        {
            _context.Reports!.Add(report);
            ReportVersion reportVersion = new()
            {
                Id = Guid.NewGuid(),
                ReportId = report.Id,
                Version = 1,
                ImportDate = report.UpdateDate,
                CreatorId = userId,
            };
            _context.ReportVersions!.Add(reportVersion);
            await _context.SaveChangesAsync();

            var reportCreated = await GetReportById(report.Id);

            return reportCreated!;
        }

        //Update report
        public async Task ReupReport(Guid reportId, Guid userId)
        {
            var report = await _context.Reports!
                .Include(r => r.ReportVersions)
                .FirstOrDefaultAsync(r => r.Id == reportId);

            var nextversion = report!.GetMaxVersion() + 1;
            var reportVersion = new ReportVersion
            {
                Id = Guid.NewGuid(),
                ReportId = report.Id,
                Version = nextversion ?? 1,
                ImportDate = DateTime.Now,
                CreatorId = userId,
            };
            _context.ReportVersions!.Add(reportVersion);

            await _context.SaveChangesAsync();
        }

        public async Task<List<Report>> GetAllDueReports()
        {
            return await _context.Reports!
                .Include(r => r.Term)
                .Where(r => r.Status == (int)Common.ReportStatus.New && r.Term.ReportDueDate < DateTime.Now)
                .ToListAsync();
        }

        public async Task CloseAllDueReports(List<Report> reports)
        {
            foreach (var report in reports)
            {
                report.Status = Common.ReportStatus.Closed;
            }

            await _context.SaveChangesAsync();
        }

        //Total department that has submitted the report of the year.

        public async Task<int> GetTotalDepartByYear(int year)
        {
            var departmentCount = await _context.Reports!
              .Where(r => r.Month.EndsWith(year.ToString()))
                .Select(r => r.DepartmentId)
                .Distinct()
                .CountAsync();

            return departmentCount;
        }

        //List reports by yearg
        public async Task<List<Report>> GetAllReportsByYear(int year)
        {
            var reports = await _context.Reports!
                .Include(r => r.Department)
                .Include(r => r.Term)
                .Include(r => r.ReportVersions)
                .Where(r => r.Month.EndsWith(year.ToString()))
                .ToListAsync();

            return reports;
        }

      
    }
}