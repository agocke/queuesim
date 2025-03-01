namespace Queuesim;

interface IWorkerPool
{
    int RunningJobs { get; }
    int AvailableWorkers { get; }
    int MinWorkers { get; }
    int CurrentWorkers { get; }
    void Enqueue(int queuedJobs, int currentTime, Sim.Job job);
    int RemoveFinishedJobs(int queuedJobs, int currentTime);
}