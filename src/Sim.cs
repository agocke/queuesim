using Serde;

namespace Queuesim;

static partial class Sim
{
    public readonly partial record struct Job(
        int Duration
    ) : ISerializeProvider<Job>, IDeserializeProvider<Job>
    {
        private sealed class Proxy : ISerialize<Job>, IDeserialize<Job>
        {
            public static readonly Proxy Instance = new();
            Job IDeserialize<Job>.Deserialize(IDeserializer deserializer)
                => new Job(deserializer.ReadI32());

            void ISerialize<Job>.Serialize(Job value, ISerializer serializer)
                => serializer.SerializeI32(value.Duration);
        }

        static ISerialize<Job> ISerializeProvider<Job>.SerializeInstance => Proxy.Instance;
        static IDeserialize<Job> IDeserializeProvider<Job>.DeserializeInstance => Proxy.Instance;
        static ISerdeInfo ISerdeInfoProvider.SerdeInfo { get; } = Serde.SerdeInfo.MakePrimitive(nameof(Job));
    }

    [GenerateSerde]
    public partial record struct JobGroup(
        int StartTime,
        List<Job> Jobs
    ) : IComparable<JobGroup>
    {
        public int CompareTo(JobGroup other)
        {
            return StartTime.CompareTo(other.StartTime);
        }
    }

    [GenerateSerde]
    public partial record Config(List<JobGroup> JobGroups)
    {
        public int Workers { get; init; } = 1;
    }

    [GenerateSerde]
    public partial record Result
    {
        public required List<int> QueueDepths { get; init; }
        public required List<int> Running { get; init; }
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

        var jobGroups = new PriorityQueue<JobGroup>();
        jobGroups.EnqueueRange(config.JobGroups);
        var jobQ = new Queue<Job>();
        var workerPool = new WorkerPool(config.Workers);

        var result = new Result
        {
            QueueDepths = new List<int>(),
            Running = new List<int>(),
        };

        while (jobGroups.Count > 0 || jobQ.Count > 0 || workerPool.RunningJobs > 0)
        {
            while (jobGroups.TryPeek(out var group, out _) && group.StartTime == currentTime)
            {
                group = jobGroups.Dequeue();
                foreach (var job in group.Jobs)
                {
                    jobQ.Enqueue(job);
                }
            }

            workerPool.RemoveFinishedJobs(currentTime);

            while (workerPool.AvailableWorkers > 0 && jobQ.TryDequeue(out var job))
            {
                workerPool.Enqueue(currentTime, job);
            }

            result.QueueDepths.Add(jobQ.Count);
            result.Running.Add(workerPool.RunningJobs);
            currentTime++;
        }

        return result;
    }
}