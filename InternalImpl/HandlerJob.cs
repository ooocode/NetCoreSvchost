using NetCoreSvchost.Options;
using Quartz;

namespace NetCoreSvchost.InternalImpl
{
    public delegate void ServiceMain();

    [DisallowConcurrentExecution]
    public class HandlerJob : IJob
    {
        private readonly ILogger<HandlerJob> logger;

        public HandlerJob(ILogger<HandlerJob> logger)
        {
            this.logger = logger;
        }


        public unsafe Task Execute(IJobExecutionContext context)
        {
            nint address = nint.Parse(context.JobDetail.Key.Name);
            var dll = DllList.List[address];

            var serviceMain = (delegate* unmanaged[Cdecl]<int, nint[], void>)address;
            ArgumentNullException.ThrowIfNull(serviceMain);

            logger.LogInformation($"开始运行【{context.JobDetail.Key.Name}】");

            serviceMain(dll.Count, dll.ToArray());
            return Task.CompletedTask;
        }
    }
}