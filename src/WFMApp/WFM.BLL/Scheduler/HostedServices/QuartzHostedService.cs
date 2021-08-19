using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WFM.BLL.Scheduler.HostedServices
{
    public class QuartzHostedService : IHostedService
    {
        private readonly IScheduler _scheduler;
        private readonly IJobFactory _jobFactory;


        public QuartzHostedService(
            IJobFactory jobFactory, IScheduler scheduler)
        {

            _jobFactory = jobFactory;
            _scheduler = scheduler;
        }

        public IScheduler Scheduler { get; set; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _scheduler.JobFactory = _jobFactory;
            await _scheduler.Start(cancellationToken);
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _scheduler.Shutdown(cancellationToken);
        }
    }
}
