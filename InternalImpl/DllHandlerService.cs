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

        private readonly List<DllDetail> dllDetails = [];

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var fileName = Path.Combine(AppContext.BaseDirectory, "dlls.json");
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("file not found", fileName);
            }

            var text = await File.ReadAllTextAsync(fileName, stoppingToken);
            var dllList = System.Text.Json.JsonSerializer.Deserialize(text, AppJsonSerializerContext.Default.DllList);
            ArgumentNullException.ThrowIfNull(dllList);

            foreach (var dll in dllList.Dlls)
            {
                var dllDetail = Dll.GetDllDetail(dll);
                dllDetails.Add(dllDetail);

                HandlerDll(dllDetail);
            }
        }

        private unsafe void HandlerDll(DllDetail dllDetail)
        {
            ThreadPool.QueueUserWorkItem((s) =>
            {
                DllDetail detail = (s as DllDetail)!;
                logger.LogInformation($"start execute {detail.FileName}");
                detail.ServiceMain(detail.Args.Length, detail.Args);
            }, dllDetail);
        }

        public override unsafe Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var item in dllDetails)
            {
                item.ServiceStop();
            }

            return base.StopAsync(cancellationToken);
        }
    }
}