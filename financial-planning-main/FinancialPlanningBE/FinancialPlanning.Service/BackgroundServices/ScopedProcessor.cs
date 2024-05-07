using Microsoft.Extensions.DependencyInjection;

namespace FinancialPlanning.Service.BackgroundServices
{
    public abstract class ScopedProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        protected ScopedProcessor(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task Process()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                await ProcessInScope(scope.ServiceProvider);
            }
        }
        public abstract Task ProcessInScope(IServiceProvider serviceProvider);
    }
}
