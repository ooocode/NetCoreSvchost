using NetCoreSvchost.InternalImpl;
using NetCoreSvchost.Options;
using System.Text.Json.Serialization;

var builder = Host.CreateApplicationBuilder(args);

if (Microsoft.Extensions.Hosting.WindowsServices.WindowsServiceHelpers.IsWindowsService())
{
    builder.Services.AddWindowsService();
}
else if (Microsoft.Extensions.Hosting.Systemd.SystemdHelpers.IsSystemdService())
{
    builder.Services.AddSystemd();
}

builder.Services.AddHostedService<DllHandlerService>();

var app = builder.Build();
app.Run();


[JsonSerializable(typeof(DllList))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}