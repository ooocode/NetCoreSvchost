using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using Quartz;
using Quartz.Simpl;

var builder = WebApplication.CreateSlimBuilder(args);

if (Microsoft.Extensions.Hosting.WindowsServices.WindowsServiceHelpers.IsWindowsService())
{
    builder.Host.UseWindowsService();
}
else if (Microsoft.Extensions.Hosting.Systemd.SystemdHelpers.IsSystemdService())
{
    builder.Host.UseSystemd();
}
else
{
    builder.Host.UseConsoleLifetime();
}

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});


List<string> dllNames = [@"C:\Users\94386\Desktop\com\CoreProxy.dll"];

builder.Services.AddQuartz(q =>
{
    q.UsePersistentStore<RAMJobStore>(_ => { });
    q.UseDefaultThreadPool();



    foreach (var subscribeItem in dllNames)
    {
        var jobKey = new JobKey(subscribeItem);
        q.AddJob<HandlerJob>(jobKey).AddTrigger(t =>
        {
            t.StartNow().ForJob(jobKey);
            /* t.StartNow().WithSimpleSchedule(x => x
               .WithInterval(grpcListenerOptions.Timeout)
               .RepeatForever())
               .ForJob(jobKey); */
        });
    }
});

builder.Services.AddQuartzHostedService(options =>
{
    // when shutting down we want jobs to complete gracefully
    options.WaitForJobsToComplete = true;
});


var app = builder.Build();

var sampleTodos = new Todo[] {
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
};

var todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", () => sampleTodos);
todosApi.MapGet("/{id}", (int id) =>
    sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
        ? Results.Ok(todo)
        : Results.NotFound());

app.Run();

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
