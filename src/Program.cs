using Queuesim;
using Serde.Json;
using System.Text;

var builder = WebApplication.CreateSlimBuilder(args);

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/sim", () => {
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
