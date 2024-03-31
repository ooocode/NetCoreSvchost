using NetCoreSvchost.Options;

namespace NetCoreSvchost.InternalImpl
{
    public class DllHandlerService : BackgroundService
    {
        private readonly ILogger<DllHandlerService> logger;

        public DllHandlerService(ILogger<DllHandlerService> logger)
        {
            this.logger = logger;
        }

        protected override unsafe Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var fileName = Path.Combine(AppContext.BaseDirectory, "dlls.json");
            var text = File.ReadAllText(fileName);
            var dllList = System.Text.Json.JsonSerializer.Deserialize(text, AppJsonSerializerContext.Default.DllList);
            ArgumentNullException.ThrowIfNull(dllList);

            List<DllDetail> dllDetails = new List<DllDetail>();
            foreach (var dll in dllList.Dlls)
            {
                var dllDetail = Dll.GetDllDetail(dll);
                dllDetails.Add(dllDetail);

                HandlerDll(dllDetail);
            }

            stoppingToken.Register(() =>
            {
                foreach (var item in dllDetails)
                {
                    item.ServiceStop();
                }
            });

            return Task.CompletedTask;
        }

        private unsafe void HandlerDll(DllDetail dllDetail)
        {
            ThreadPool.QueueUserWorkItem((s) =>
            {
                DllDetail detail = (s as DllDetail)!;
                logger.LogInformation($"开始运行【{detail.FileName}】");
                detail.ServiceMain(detail.Args.Length, detail.Args);
            }, dllDetail);
        }
    }
}