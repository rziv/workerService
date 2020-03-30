using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BiztalkProxyService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MW.Retry.Bl;
using MW.Retry.Dal;
using MW.Retry.Dal.Mappers;
using MW.Retry.Dal.Repositories;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace SubmitCompletionService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true).Build();

                    services.AddHostedService<Worker>();
                    // Add Quartz services
                    services.AddSingleton<IJobFactory, SingletonJobFactory>();
                    services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

                                    
                    services.AddSingleton<SubmitCompletionAsyncJob>();
                    //Cron Expression - https://en.wikipedia.org/wiki/Cron
                    var cronExpression = config["cronExpression"];
                    services.AddSingleton(new JobSchedule(
                            jobType: typeof(SubmitCompletionAsyncJob),                          
                            cronExpression: cronExpression)); 
               
                    services.AddSingleton<IRetryHandler, RetryHandler>();
                    services.AddSingleton<IMiddlewareServices>(provider => new MiddlewareServicesClient());                  
                    
                    var optionsBuilder = new DbContextOptionsBuilder<RetryContext>()
                    .UseSqlServer(config.GetSection("ConnectionStrings").GetSection("GovForms.Horizon").Value);
                    services.AddSingleton(provider => new RetryContext(optionsBuilder.Options));
                    services.AddSingleton<IRetryDataRepository, RetryDataRepository>();

                    services.AddSingleton<IUow, Uow>();

                    services.AddSingleton<IMapper>(provider => new Mapper(new MapperConfig().Create()));
                });
    }
}
