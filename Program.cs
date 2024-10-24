using NetCoreScvhost.Contract;
using NetCoreSvchost.InternalImpl;
using NetCoreSvchost.Options;
using System.Text.Json.Serialization;

var classId = Guid.Parse(ClassIds.TestV1);
var interfaceId = Guid.Parse(ITest.IID);

ClientImport.DllGetClassObject(ref classId, ref interfaceId, out var ppv);
ITest test = ppv as ITest;
var XS = test.Method(1,"ÕÅÈý");

List<string> expected = ["1", "2±³¾°ÏÂ", "88888", "4", "5"];
test.Show("152", expected.ToArray(), expected.Count);

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