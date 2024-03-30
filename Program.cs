using NetCoreSvchost.InternalImpl;
using NetCoreSvchost.Options;
using Quartz;
using Quartz.AspNetCore;
using Quartz.Simpl;
using System.Text.Json.Serialization;

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


List<Dll>? dllNames = builder.Configuration.GetSection("Dlls").Get<List<Dll>>();
ArgumentNullException.ThrowIfNull(dllNames);

dllNames = dllNames.Where(e => !string.IsNullOrWhiteSpace(e.FileName)).DistinctBy(e => e.FileName).ToList();

builder.Services.AddQuartz(q =>
{
    q.UsePersistentStore<RAMJobStore>(_ => { });
    q.UseDefaultThreadPool();

    foreach (var subscribeItem in dllNames)
    {
        nint address = DllList.Add(subscribeItem);

        var jobKey = new JobKey(address.ToString());
        q.AddJob<HandlerJob>(jobKey).AddTrigger(t =>
        {
            t.StartNow().ForJob(jobKey);
            //t.StartNow().WithSimpleSchedule(x => x
            //   .WithInterval(TimeSpan.FromSeconds(10))
            //   .RepeatForever())
            //   .ForJob(jobKey);
        });
    }
});

builder.Services.AddQuartzServer(options =>
{
    // when shutting down we want jobs to complete gracefully
    options.WaitForJobsToComplete = false;
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