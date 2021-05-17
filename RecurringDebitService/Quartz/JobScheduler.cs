using NLog;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecurringDebitService.Quartz
{
    [DisallowConcurrentExecution]
    public class JobScheduler
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        public static void Start()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();
            IJobDetail Tranxjob = JobBuilder.Create<PayStackDebitJob>().Build();
            ITrigger Ttrigger = TriggerBuilder.Create()
                  .WithDailyTimeIntervalSchedule(s => s.WithIntervalInHours(24).OnEveryDay()
                  .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(1, 30))).Build();
            scheduler.ScheduleJob(Tranxjob, Ttrigger);
        }
        public static void Stop()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Shutdown(false);
        }
    }
}
