using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.InputHistory
{
    /// <summary>
    /// Wrapper around a StreamWriter that queues all writes/flushes and
    /// executes each asynchronously. It does this instead of synchronous
    /// operations (as we don't want to block the main thread) and
    /// asynchronous operations (as those aren't queued).
    /// </summary>
    class QueuedStreamWriter : StreamWriter
    {
        private Task currentTask;
        public QueuedStreamWriter(string path) : base(path) {}

        public void WriteLineQueued(string value)
        {
            if (currentTask == null)
            {
                currentTask = base.WriteLineAsync(value);
                return;
            }

            var task = currentTask;
            currentTask = Task.Run(async () =>
            {
                await task;
                await base.WriteLineAsync(value);
            });
        }

        public void CloseQueued()
        {
            if (currentTask == null)
            {
                base.Close();
                return;
            }

            var task = currentTask;
            currentTask = Task.Run(async () =>
            {
                await task;
                base.Close();
            });
        }

        public void FlushQueued()
        {
            if (currentTask == null)
            {
                currentTask = base.FlushAsync();
                return;
            }

            var task = currentTask;
            currentTask = Task.Run(async () =>
            {
                await task;
                await base.FlushAsync();
            });
        }
    }
}
