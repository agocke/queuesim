using Microsoft.AspNetCore.Html;
using Serde.Json;
using System.Text;
using Queuesim.Slices;

namespace Queuesim;

internal record SimResults(
    HtmlString ChartData,
    Sim.Result Result,
    HtmlString JobData,
    int Workers
);

class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        var app = builder.Build();

        app.MapGet("/", () =>
        {
            return Results.Extensions.RazorSlice<RenderChart, SimResults?>(null);
        });

        app.MapGet("/run", (HttpContext ctx) => {
            var rawJobData = ctx.Request.Query["jobData"].Single()!;
            var jobData = JsonSerializer.DeserializeList<Sim.Config.JobGroup>(rawJobData);
            var workers = int.Parse(ctx.Request.Query["workers"]!);
            var results = Sim.Run(new Sim.Config(jobData) { Workers = workers });
            var chartData = JsonSerializer.Serialize(results);
            return Results.Extensions.RazorSlice<RenderChart, SimResults?>(new(
                new HtmlString(chartData),
                results,
                new HtmlString(rawJobData),
                workers
            ));
        });

        app.Run();
    }
}
