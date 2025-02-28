
namespace Queuesim;

class WorkerPool(int currentWorkers)
{
    private record struct RunningJob(int EndTime);

    private readonly List<RunningJob> _runningJobs = new();

    public int RunningJobs { get; private set; }

    public int AvailableWorkers => currentWorkers - RunningJobs;

    public void Enqueue(int currentTime, Sim.Job job)
    {
        _runningJobs.Add(new RunningJob(currentTime + job.Duration));
        RunningJobs++;
    }

    public void RemoveFinishedJobs(int currentTime)
    {
        var finished = _runningJobs.RemoveAll(job => job.EndTime == currentTime);
        RunningJobs -= finished;
    }
}