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
            //New Scheduler
            IJobDetail Tranxjob = JobBuilder.Create<NewsJob>().Build();
            ITrigger Ttrigger = TriggerBuilder.Create()
                 .WithSimpleSchedule(s => s.WithIntervalInMinutes(15).RepeatForever()).Build();
            scheduler.ScheduleJob(Tranxjob, Ttrigger);


            //Football League Scheduler
            IJobDetail League = JobBuilder.Create<LeagueJob>().Build();
            ITrigger TLeague = TriggerBuilder.Create().WithDailyTimeIntervalSchedule(s => s.WithIntervalInHours(24).OnEveryDay()
                  .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(6, 00))).Build();
            scheduler.ScheduleJob(League, TLeague);
        }
        public static void Stop()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Shutdown(false);
        }
    }
}
