using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PersonalShoppingAPI.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PersonalShoppingAPI.Services
{
    public class DeleteAccountService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<DeleteAccountService> _logger;
        private readonly IConfiguration _configuration;

        public DeleteAccountService(IServiceScopeFactory serviceScopeFactory, ILogger<DeleteAccountService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<SHOPPINGLISTContext>();
                    var items = await dbContext.Users.ToListAsync();
                    foreach (var item in items)
                    {
                        Console.WriteLine($"{item.FullName} - {item.PhoneNumber}");
                    }

                    await Task.Delay(new TimeSpan(0, 0, 10));
                }
                    
            }
        }
    }
}
