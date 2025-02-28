using Microsoft.AspNetCore.Html;
using Serde.Json;
using System.Text;

namespace Queuesim;

class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        var app = builder.Build();

        app.MapGet("/", () =>
        {
            return Results.Extensions.RazorSlice<Queuesim.Slices.RenderChart, Microsoft.AspNetCore.Html.HtmlString?>(null);
        });

        app.MapGet("/run", () => {
            var config = new Sim.Config([
                new(StartTime: 1, [new(5), new(5), new(5)]),
                new(StartTime: 5, [new(1), new(2), new(3)]),
            ]);
            var results = JsonSerializer.Serialize(Sim.Run(config));
            return Results.Extensions.RazorSlice<Queuesim.Slices.RenderChart, HtmlString?>(new HtmlString(results));
        });

        app.MapGet("/sim", () =>
        {
            var config = new Sim.Config([
                new(StartTime: 1, [new(5), new(5), new(5)]),
                    new(StartTime: 5, [new(1), new(2), new(3)]),
            ]);
            var results = Sim.Run(config);
            return Results.Content(
                JsonSerializer.Serialize(results),
                "application/json",
                Encoding.UTF8);
        });

        app.Run();

    }
}
