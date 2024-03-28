using System.Runtime.InteropServices;
using Quartz;

[DisallowConcurrentExecution]
public class HandlerJob : IJob
{
     public  unsafe delegate* unmanaged<string, string, string, void> OnLogAction;

    public delegate void ServiceMain(int argc, nint argv);
    public Task Execute(IJobExecutionContext context)
    {
        NativeLibrary.TryLoad(@"C:\Users\94386\Desktop\com\CoreProxy.dll", out var ptr);
        NativeLibrary.TryGetExport(ptr, "ServiceMain", out var address);
        ServiceMain serviceMain = Marshal.GetDelegateForFunctionPointer<ServiceMain>(ptr);
        serviceMain(0, nint.Zero);
        throw new NotImplementedException();
    }
}