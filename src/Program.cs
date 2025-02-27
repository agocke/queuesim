using Queuesim;
using Serde.Json;
using System.Text;

var builder = WebApplication.CreateSlimBuilder(args);

var app = builder.Build();

app.MapGet("/", () => {
    return Results.Extensions.RazorSlice<Queuesim.Slices.Hello, string>("");
});
app.MapGet("/sim", () => {
    var results = Sim.Run([
        new(Id: 1, Duration: 5),
        new(1, 5)
    ]);
    return Results.Content(
        JsonSerializer.Serialize(results),
        "application/json",
        Encoding.UTF8);
});

app.Run();
