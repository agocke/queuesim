using Microsoft.AspNetCore.Html;
using Serde.Json;
using System.Text;
using Queuesim.Slices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;

namespace Queuesim;

internal record SimResults(
    HtmlString ChartData,
    Sim.Result Result,
    HtmlString JobData,
    Sim.Config OriginalConfig
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

        app.MapPost("/run", async (HttpContext ctx) => {
            var form = await ctx.Request.ReadFormAsync();
            var rawJobData = form["jobData"].Single()!;
            var jobData = JsonSerializer.DeserializeList<Sim.JobGroup>(rawJobData);
            var config = new Sim.Config(
                (Sim.WorkerPoolScaling)int.Parse(form["workerPoolScaling"]!),
                int.Parse(form["minWorkers"]!),
                int.Parse(form["maxWorkers"]!),
                int.Parse(form["scaleUpTime"]!),
                int.Parse(form["scaleDownDelay"]!)
            );
            var results = Sim.Run(jobData, config);
            var chartData = JsonSerializer.Serialize(results);
            return Results.Extensions.RazorSlice<RenderChart, SimResults?>(new(
                new HtmlString(chartData),
                results,
                new HtmlString(rawJobData),
                config
            ));
        });

        app.Run();
    }
}
