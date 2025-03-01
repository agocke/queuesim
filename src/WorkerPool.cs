
using System.Collections.Generic;

namespace Queuesim;

class FixedWorkerPool(int currentWorkers) : IWorkerPool
{
    private record struct RunningJob(int EndTime);

    private readonly List<RunningJob> _runningJobs = new();

    public int RunningJobs => _runningJobs.Count;

    public int AvailableWorkers => currentWorkers - RunningJobs;

    public void Enqueue(int _, int currentTime, Sim.Job job)
    {
        _runningJobs.Add(new RunningJob(currentTime + job.Duration));
    }

    public int RemoveFinishedJobs(int _, int currentTime)
    {
        var finished = _runningJobs.RemoveAll(job => job.EndTime == currentTime);
        return finished;
    }
}