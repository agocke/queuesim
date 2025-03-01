using System;
using System.Collections.Generic;
using Serde;

namespace Queuesim;

static partial class Sim
{
    public readonly partial record struct Job(int Duration);

    public partial record Config(List<Config.JobGroup> JobGroups)
    {
        public int Workers { get; init; } = 1;

        [GenerateSerde]
        public partial record JobGroup(int StartTime) : IComparable<JobGroup>
        {
            public List<int>? ManualJobs { get; init; }

            [GenerateSerde]
            public partial record struct AutoJob(int Duration, int Count);
            public List<AutoJob>? AutoJobs { get; init; }

            public int CompareTo(JobGroup? other)
            {
                if (other is null)
                {
                    throw new ArgumentNullException(nameof(other));
                }
                return StartTime.CompareTo(other.StartTime);
            }
        }
    }

    [GenerateSerde]
    public partial record Result
    {
        public required List<int> QueueDepths { get; init; }
        public required List<int> Running { get; init; }
        public required List<int> Finished { get; init; }
    }

    internal class PriorityQueue<TElement> : PriorityQueue<TElement, TElement>
        where TElement : IComparable<TElement>
    {
        public void EnqueueRange(IEnumerable<TElement> elements)
        {
            foreach (var element in elements)
            {
                Enqueue(element, element);
            }
        }
    }

    public static Result Run(Config config)
    {
        int currentTime = 0;

        var jobGroups = new PriorityQueue<Config.JobGroup>();
        jobGroups.EnqueueRange(config.JobGroups);
        var jobQ = new Queue<Job>();
        var workerPool = new WorkerPool(config.Workers);

        var result = new Result
        {
            QueueDepths = new List<int>(),
            Running = new List<int>(),
            Finished = new List<int>(),
        };

        while (jobGroups.Count > 0 || jobQ.Count > 0 || workerPool.RunningJobs > 0)
        {
            while (jobGroups.TryPeek(out var group, out _) && group.StartTime == currentTime)
            {
                group = jobGroups.Dequeue();
                foreach (var duration in group.ManualJobs ?? [])
                {
                    jobQ.Enqueue(new Job(duration));
                }
                foreach (var autoJob in group.AutoJobs ?? [])
                {
                    for (int i = 0; i < autoJob.Count; i++)
                    {
                        jobQ.Enqueue(new Job(autoJob.Duration));
                    }
                }
            }

            var finished = workerPool.RemoveFinishedJobs(currentTime);

            while (workerPool.AvailableWorkers > 0 && jobQ.TryDequeue(out var job))
            {
                workerPool.Enqueue(currentTime, job);
            }

            result.QueueDepths.Add(jobQ.Count);
            result.Running.Add(workerPool.RunningJobs);
            result.Finished.Add(finished);
            currentTime++;
        }

        return result;
    }
}