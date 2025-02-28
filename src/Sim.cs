using Serde;

namespace Queuesim;

static partial class Sim
{
    [GenerateSerde]
    public partial record Job(int Duration);

    [GenerateSerde]
    public partial record JobGroup(
        int StartTime,
        List<Job> Jobs
    );

    [GenerateSerde]
    public partial record Config(List<JobGroup> JobGroups)
    {
        public int MaxWorkers { get; init; } = 1;
    }

    [GenerateSerde]
    public partial record Result
    {
        public required List<int> QueueDepths { get; init; }
        public required List<int> Running { get; init; }
    }

    private record RunningJob(int EndTime);

    public static Result Run(Config config)
    {
        int currentTime = 0;

        List<JobGroup> sorted;
        {
            var unsorted = config.JobGroups.ToList();
            unsorted.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
            sorted = unsorted;
        }

        var jobGroups = new Queue<JobGroup>(sorted);
        var jobQ = new Queue<Job>();
        var running = new List<RunningJob>();
        int occupiedWorkers = 0;

        var qDepths = new List<int>();
        var runningByTime = new List<int>();

        while (jobGroups.Count > 0 || jobQ.Count > 0)
        {
            while (jobGroups.TryPeek(out var group) && group.StartTime == currentTime)
            {
                group = jobGroups.Dequeue();
                foreach (var job in group.Jobs)
                {
                    jobQ.Enqueue(job);
                }
            }

            while (running.Count < config.MaxWorkers && jobQ.TryDequeue(out var job))
            {
                running.Add(new RunningJob(currentTime + job.Duration));
            }

            while (running.Count > 0 && running[0].EndTime == currentTime)
            {
                running.RemoveAt(0);
                occupiedWorkers--;
            }

            qDepths.Add(jobQ.Count);
            runningByTime.Add(running.Count);
            currentTime++;
        }

        return new Result
        {
            QueueDepths = qDepths,
            Running = runningByTime,
        };
    }
}