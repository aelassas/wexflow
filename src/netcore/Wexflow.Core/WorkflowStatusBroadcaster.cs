using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Wexflow.Core
{
    /// <summary>
    /// Manages subscriptions and broadcasts workflow job status updates to subscribers.
    /// Subscribers register callbacks that will be invoked when a job status changes.
    /// </summary>
    public class WorkflowStatusBroadcaster
    {
        private readonly ConcurrentDictionary<string, List<Action<string>>> _subscribers = new();

        /// <summary>
        /// Generates a unique key for each workflow-job pair.
        /// </summary>
        /// <param name="workflowId">The workflow identifier.</param>
        /// <param name="jobId">The job identifier.</param>
        /// <returns>A combined string key.</returns>
        private string GetKey(int workflowId, string jobId) => $"{workflowId}:{jobId}";

        /// <summary>
        /// Subscribes a handler to status updates for a specific workflow and job.
        /// </summary>
        /// <param name="workflowId">The workflow identifier.</param>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="handler">The callback to invoke when status updates occur.</param>
        public void Subscribe(int workflowId, string jobId, Action<string> handler)
        {
            var key = GetKey(workflowId, jobId);
            var list = _subscribers.GetOrAdd(key, _ => new List<Action<string>>());
            lock (list)
            {
                list.Add(handler);
            }
        }

        /// <summary>
        /// Unsubscribes a handler from status updates for a specific workflow and job.
        /// </summary>
        /// <param name="workflowId">The workflow identifier.</param>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="handler">The callback to remove.</param>
        public void Unsubscribe(int workflowId, string jobId, Action<string> handler)
        {
            var key = GetKey(workflowId, jobId);
            if (_subscribers.TryGetValue(key, out var list))
            {
                lock (list)
                {
                    list.Remove(handler);
                    if (list.Count == 0)
                    {
                        _subscribers.TryRemove(key, out _);
                    }
                }
            }
        }

        /// <summary>
        /// Broadcasts a status update to all subscribers of a specific workflow and job.
        /// </summary>
        /// <param name="workflowId">The workflow identifier.</param>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="status">The new status to broadcast.</param>
        public void Broadcast(int workflowId, string jobId, string status)
        {
            var key = GetKey(workflowId, jobId);
            if (_subscribers.TryGetValue(key, out var list))
            {
                List<Action<string>> subscribersCopy;
                lock (list)
                {
                    subscribersCopy = list.ToList();
                }
                foreach (var subscriber in subscribersCopy)
                {
                    subscriber(status);
                }
            }
        }
    }
}
