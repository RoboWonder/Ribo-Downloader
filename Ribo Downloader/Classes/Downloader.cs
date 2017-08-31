using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Net;
using CoreState = Ribo_Downloader.Core.State;

namespace Ribo_Downloader.Core
{
    public class Downloader
    {
        private const int KB = 1024;
        private const long MB = 1024 * KB;
        private const long GB = 1024 * MB;
        DateTime FirstTime;
        public int State;
        public bool IsStateCompleted { private set; get; }
        public Exception Error { private set; get; }

        public double Completed;
        public double Speed;
        public double Progress;
        private long windowStart;

        public string FileName;
        public long FileSize;
        public string Url;
        public string FileTarget;
        public Scheduler scheduler;
        public Chunks chunks;

        private Thread workerThread;

        private System.Windows.Forms.Timer timerTracker;

        public Downloader(string Url, string FileTarget, System.Windows.Forms.Timer timer = null)
        {
            try
            {
                if(Url.IndexOf("://www.youtube.com") > 0)
                {
                    Classes.YoutubeLink yt = new Classes.YoutubeLink(Url);
                    Url = yt.getDownloadLink();
                }
                this.Url = Url;
                FileInfo();
                this.FileTarget = FileTarget + FileName;


                chunks = new Chunks(Url, FileSize);
                scheduler = new Scheduler(chunks);

                IsStateCompleted = false; State = CoreState.Create;

                if (timer != null)
                {
                    timerTracker = timer;
                    timerTracker.Tick += DownloadTracker_Tick;
                    FirstTime = DateTime.Now;
                }

                IsStateCompleted = false; State = CoreState.Idle;
            }
            catch (Exception e)
            {
                IsStateCompleted = false; State = CoreState.Error;

                Error = e;

                IsStateCompleted = true;
            }
        }

        public string FormatBytes(double bytes)
        {
            if (bytes > GB) return String.Format("{0:f2} GB", bytes / GB);
            if (bytes > MB) return String.Format("{0:f2} MB", bytes / MB);
            return String.Format("{0:f2} KB", bytes / KB);
        }

        public void FileInfo()
        {
            try
            {
                HttpWebRequest request = WebRequest.CreateHttp(Url);
                request.AllowAutoRedirect = true;
                using (HttpWebResponse res = (HttpWebResponse)request.GetResponse())
                {
                    FileSize = res.ContentLength;
                    string contentType = res.ContentType;
                    string physicalPath = res.ResponseUri.AbsolutePath.Split('/').Last();

                    if (physicalPath.Contains('.'))
                    {
                        FileName = physicalPath;
                    }
                    else
                    {
                        FileName = physicalPath + "." + contentType.Split('/').Last();
                    }
                }
            }
            catch
            {
                FileSize = 0;
                HttpWebRequest fileNameReq = WebRequest.CreateHttp(Url);
                fileNameReq.AllowAutoRedirect = true;
                fileNameReq.AddRange(0, 1024);

                using (HttpWebResponse fileNameRes = (HttpWebResponse)fileNameReq.GetResponse())
                {
                    string contentType = fileNameRes.ContentType;
                    string physicalPath = fileNameRes.ResponseUri.AbsolutePath.Split('/').Last();

                    if (physicalPath.Contains('.'))
                    {
                        FileName = physicalPath;
                    }
                    else
                    {
                        FileName = physicalPath + "." + contentType.Split('/').Last();
                    }
                }
            }
        }

        private void DownloadTracker_Tick(object sender, EventArgs e)
        {
            switch (State)
            {
                case CoreState.Complete:
                case CoreState.Download:
                    if (this.Progress < 100)
                    {
                        DateTime now = DateTime.Now;
                        TimeSpan timeSpan = now - FirstTime;
                        double t = timeSpan.Seconds + (double)timeSpan.Milliseconds / 1000;
                        OnDownload(t);
                        FirstTime = now;
                    }
                    break;
            }
        }

        public void Start()
        {
            if (State == CoreState.Idle)
            {
                IsStateCompleted = false; State = CoreState.Start;

                Error = null;

                workerThread = new Thread(() =>
                {
                    try
                    {
                        IsStateCompleted = false; State = CoreState.Download;

                        scheduler.Start();

                        IsStateCompleted = false; State = CoreState.Append;

                        Join();

                        IsStateCompleted = false; State = CoreState.Complete;

                        Complete();

                        IsStateCompleted = true;
                    }
                    catch (ThreadAbortException) {  }
                    catch (Exception e)
                    {
                        IsStateCompleted = false; State = CoreState.Abort;

                        Abort().Join();

                        IsStateCompleted = false; State = CoreState.Error;

                        Error = e;

                        IsStateCompleted = true;
                    }
                });

                workerThread.Start();
            }
        }

        public Thread Abort()
        {
            IsStateCompleted = false; State = CoreState.Abort;

            Thread abortThread = new Thread(() =>
            {
                if (workerThread.IsAlive)
                {
                    scheduler.Abort();
                }

                IsStateCompleted = true; State = CoreState.Idle;
            });

            abortThread.Start();
            return abortThread;
        }

        private void Join()
        {
            using (BufferedStream TargetFile = new BufferedStream(new FileStream(FileTarget, FileMode.Create, FileAccess.Write)))
            {
                for (int i = 0; i < chunks.ChunkCount; i++)
                {
                    using (BufferedStream SourceChunks = new BufferedStream(File.OpenRead(chunks.ChunkTarget(i))))
                    {
                        SourceChunks.CopyTo(TargetFile);
                    }
                }
            }
        }

        private void Complete()
        {
            for (int i = 0; i < chunks.ChunkCount; i++)
            {
                File.Delete(chunks.ChunkTarget(i));
            }

            Directory.Delete(Path.GetDirectoryName(chunks.ChunkTarget(0)));
        }
        public void OnDownload(double timeSpan)
        {
            double bufferedSize;
            double downloadedSize = chunks.ChunkSize * windowStart;
            //Chunks chunks = DwnlChunks;

            while (windowStart < chunks.ChunkCount)
            {
                bufferedSize = Interlocked.Read(ref chunks.ChunkProgress[windowStart]);
                if (bufferedSize == chunks.ChunkSize)
                {
                    windowStart++;
                    downloadedSize += chunks.ChunkSize;
                }
                else
                {
                    break;
                }
            }

            for (long i = windowStart; i < chunks.ChunkCount; i++)
            {
                bufferedSize = Interlocked.Read(ref chunks.ChunkProgress[i]);
                if (bufferedSize != 0)
                {
                    downloadedSize += bufferedSize;
                }
                else
                {
                    break;
                }
            }

            Speed = Math.Max(0, downloadedSize - Completed) / timeSpan;
            Completed = downloadedSize;
            Progress = Completed / FileSize * 100;
        }
    }
}
