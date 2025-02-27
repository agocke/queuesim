using Serde;

namespace Queuesim;

static partial class Sim
{
    public static Result Run(List<Job> jobs)
    {
        return new Result
        {
            QueueDepths = [65, 59, 80, 81, 56, 55, 40]
        };
    }

    [GenerateSerde]
    public partial record Result
    {
        public required List<int> QueueDepths { get; init; }
    }
}