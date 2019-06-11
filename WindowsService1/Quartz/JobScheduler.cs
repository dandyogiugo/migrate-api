using NLog;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsService1.NewsAPIJob;

namespace WindowsService1.Quartz
{
    [DisallowConcurrentExecution]
    public class JobScheduler
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        public static void Start()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();
            IJobDetail Tranxjob = JobBuilder.Create<NewsJob>().Build();
            ITrigger Ttrigger = TriggerBuilder.Create()
                 .WithSimpleSchedule(s => s.WithIntervalInMinutes(15).RepeatForever()).Build();
            scheduler.ScheduleJob(Tranxjob, Ttrigger);
        }
        public static void Stop()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Shutdown(false);
        }
    }
}
