using System;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace Ribo_Downloader.Core
{
    public class Chunks
    {
        public const int CHUNK_BUFFER_SIZE = 8 * 1024;
        public const long CHUNK_SIZE_LIMIT = 10485760;
        public long ChunkSize { private set; get; }
        public long ChunkCount { private set; get; }
        public string ChunkSource { private set; get; }
        public string ChunkTargetTemplate { private set; get; }
        public long[] ChunkProgress { private set; get; }
        public long TotalSize { private set; get; }
        public Chunks(string chunkSource, long totalSize)
        {
            TotalSize = totalSize;
            ChunkSource = chunkSource;
            ChunkCount = FindChunkCount();
            ChunkSize = ChunkCount != 1 ? CHUNK_SIZE_LIMIT : totalSize;

            ChunkProgress = new long[ChunkCount];

            ChunkTargetTemplate = VAR.LocalData
                + (uint)(ChunkSource + ChunkSize + TotalSize).GetHashCode()
                + "/file {0}.tmp";

            if (!Directory.Exists(Path.GetDirectoryName(ChunkTarget(0))))
                Directory.CreateDirectory(Path.GetDirectoryName(ChunkTarget(0)));
        }

        private long FindChunkCount()
        {
            HttpWebRequest rangeReq = WebRequest.CreateHttp(ChunkSource);
            rangeReq.AddRange(0, CHUNK_SIZE_LIMIT);
            rangeReq.AllowAutoRedirect = true;

            using (HttpWebResponse rangeRes = (HttpWebResponse)rangeReq.GetResponse())
            {
                if (rangeRes.StatusCode < HttpStatusCode.Redirect && rangeRes.Headers[HttpResponseHeader.AcceptRanges] == "bytes")
                {
                    return (TotalSize / CHUNK_SIZE_LIMIT + (TotalSize % CHUNK_SIZE_LIMIT > 0 ? 1 : 0));
                }
                else
                {
                    return 1;
                }
            }
        }

        public string ChunkTarget(long chunkId)
        {
            return string.Format(ChunkTargetTemplate, chunkId);
        }

        public void DownloadChunk(long chunkId)
        {
            long chunkStart = ChunkSize * chunkId;
            long chunkEnd = Math.Min(chunkStart + ChunkSize - 1, TotalSize);
            long chunkDownloaded = File.Exists(ChunkTarget(chunkId)) ? new FileInfo(ChunkTarget(chunkId)).Length : 0;
            chunkStart += chunkDownloaded;
            ChunkProgress[chunkId] = chunkDownloaded;

            if (chunkStart < chunkEnd)
            {
                HttpWebRequest dwnlReq = WebRequest.CreateHttp(ChunkSource);
                dwnlReq.AllowAutoRedirect = true;
                dwnlReq.AddRange(chunkStart, chunkEnd);
                dwnlReq.ServicePoint.ConnectionLimit = 100;
                dwnlReq.ServicePoint.Expect100Continue = false;

                try
                {
                    using (HttpWebResponse dwnlRes = (HttpWebResponse)dwnlReq.GetResponse())
                    using (Stream dwnlSource = dwnlRes.GetResponseStream())
                    using (FileStream dwnlTarget = new FileStream(ChunkTarget(chunkId), FileMode.Append, FileAccess.Write))
                    {
                        int bufferedSize;
                        byte[] buffer = new byte[CHUNK_BUFFER_SIZE];

                        do
                        {
                            Task<int> bufferReader = dwnlSource.ReadAsync(buffer, 0, CHUNK_BUFFER_SIZE);
                            bufferReader.Wait();

                            bufferedSize = bufferReader.Result;
                            Interlocked.Add(ref ChunkProgress[chunkId], bufferedSize);

                            dwnlTarget.Write(buffer, 0, bufferedSize);

                        } while (bufferedSize > 0);
                    }
                }
                finally
                {
                    dwnlReq.Abort();
                }
            }
        }
    }
}
