using FinancialPlanning.Service.BackgroundServices;
using FinancialPlanning.Service.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FinancialPlanning.Service.ScheduleTasks
{
    public class DayEndTasks : ScheduledProcessor
    {
        private readonly ILogger<StartTerm> _logger;

        public DayEndTasks(IServiceScopeFactory serviceScopeFactory, ILogger<StartTerm> logger) : base(serviceScopeFactory)
        {
            _logger = logger;
        }

        // protected override string Schedule => "*/3 * * * *"; // every 3 minute for testing
        protected override string Schedule => "59 23 * * *"; // every day at 23:59 

        public override async Task ProcessInScope(IServiceProvider serviceProvider)
        {
            var termService = serviceProvider.GetRequiredService<TermService>();
            var reportService = serviceProvider.GetRequiredService<ReportService>();
            var planService = serviceProvider.GetRequiredService<PlanService>();
            
            await termService.CloseDueTerms();
            await reportService.CloseDueReports();
            await planService.CloseDuePlans();
        }
    }
}