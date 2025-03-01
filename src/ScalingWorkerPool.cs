
using System;
using System.Collections.Generic;

namespace Queuesim;

/// <summary>
/// A worker pool that scales up and down based on the number of running jobs.
/// </summary>
/// <param name="minWorkers">
/// The minimum number of workers in the pool.
/// </param>
/// <param name="maxWorkers">
/// The maximum number of workers in the pool.
/// </param>
/// <param name="cooldownTime">
/// The time to wait before scaling down the number of workers.
/// </param>
class ScalingWorkerPool(
    int minWorkers,
    int maxWorkers,
    int scaleUpTime,
    int cooldownTime) : IWorkerPool
{
    private record struct RunningJob(int EndTime);
    private readonly List<RunningJob> _runningJobs = new();

    private enum ScaleState {
        Stable,
        ScalingUp,
        ScalingDown
    }
    private ScaleState _scaleState = ScaleState.Stable;
    private int _scaleEndTime = -1;

    // Prevent warning about multiple usage of primary constructor parameters
    private readonly int _minWorkers = minWorkers;
    private int _currentWorkers = minWorkers;

    public int RunningJobs => _runningJobs.Count;

    public int AvailableWorkers => _currentWorkers - RunningJobs;

    public void Enqueue(int queuedJobs, int currentTime, Sim.Job job)
    {
        if (AvailableWorkers == 0)
        {
            throw new InvalidOperationException("No available workers");
        }
        _runningJobs.Add(new RunningJob(currentTime + job.Duration));
        ScaleWorkers(queuedJobs, currentTime);
    }

    public int RemoveFinishedJobs(int queuedJobs, int currentTime)
    {
        var finished = _runningJobs.RemoveAll(job => job.EndTime == currentTime);
        ScaleWorkers(queuedJobs, currentTime);
        return finished;
    }

    private void ScaleWorkers(int queuedJobs, int currentTime)
    {
        if (_scaleState != ScaleState.Stable)
        {
            if (currentTime >= _scaleEndTime)
            {
                if (_scaleState == ScaleState.ScalingUp)
                {
                    _currentWorkers++;
                }
                else if (AvailableWorkers > 0)
                {
                    _currentWorkers--;
                }
                _scaleState = ScaleState.Stable;
                _scaleEndTime = -1;
            }
            return;
        }

        // Scale Up
        if (AvailableWorkers == 0 &&
            queuedJobs > 0 && _currentWorkers < maxWorkers)
        {
            _scaleState = ScaleState.ScalingUp;
            _scaleEndTime = currentTime + scaleUpTime;
        }
        // Scale Down (Cooldown-based)
        else if (AvailableWorkers > 0 &&
            queuedJobs == 0 && _currentWorkers > _minWorkers)
        {
            _scaleState = ScaleState.ScalingDown;
            _scaleEndTime = currentTime + cooldownTime;
        }
    }
}