using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ribo_Downloader.Core
{
    public class Scheduler
    {
        private const long SCHEDULER_LIMIT = 16;

        private long nextChunk;
        private Thread[] schedulerThreads;

        public Exception Error { private set; get; }
        public Chunks Chunks { private set; get; }
        public int ThreadLength { private set; get; }

        public Scheduler(Chunks chunks) { Chunks = chunks; }

        public void Start()
        {
            Error = null;
            nextChunk = 0;

            schedulerThreads = new Thread[Math.Min(SCHEDULER_LIMIT, Chunks.ChunkCount)];
            for (int i = 0; i < schedulerThreads.Length; i++)
                schedulerThreads[i] = new Thread(() => Schedule());

            for (int i = 0; i < schedulerThreads.Length; i++) schedulerThreads[i].Start();

            for (int i = 0; i < schedulerThreads.Length; i++)
            {
                if (schedulerThreads[i].IsAlive)
                {
                    schedulerThreads[i].Join();
                }

                if (Error != null)
                {
                    throw Error;
                }
            }
        }

        public void Abort()
        {
            for (int i = 0; i < schedulerThreads.Length; i++)
            {
                if (schedulerThreads[i].IsAlive)
                {
                    schedulerThreads[i].Abort();
                }
            }

            for (int i = 0; i < schedulerThreads.Length; i++)
            {
                if (schedulerThreads[i].IsAlive)
                {
                    schedulerThreads[i].Join();
                }
            }
        }

        private void Schedule()
        {
            try
            {
                while (true)
                {
                    ThreadLength = schedulerThreads.Length;
                    long currentChunk = -1;
                    lock (Chunks)
                    {
                        if (nextChunk < Chunks.ChunkCount)
                        {
                            currentChunk = nextChunk++;
                        }
                    }

                    if (currentChunk != -1 && Error == null)
                    {
                        Chunks.DownloadChunk(currentChunk);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Error = e;
            }
        }
    }
}
