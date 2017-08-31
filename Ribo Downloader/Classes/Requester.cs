using System;
using System.IO;
using System.Net;

namespace Ribo_Downloader.Classes
{
    public class Requester
    {
        private WebRequest request;
        private Stream dataStream;

        private string status;
        private WebResponse response = null;

        public String Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
            }
        }
        public Requester(string url)
        {
            request = WebRequest.Create(url);
        }
        public WebResponse GetResponse()
        {
            return response = request.GetResponse();
        }

        public string GetContent()
        {
            if(response == null)
            {
                GetResponse();
            }

            Status = ((HttpWebResponse)response).StatusDescription;

            dataStream = response.GetResponseStream();

            StreamReader reader = new StreamReader(dataStream);

            string content = reader.ReadToEnd();

            reader.Close();
            dataStream.Close();
            response.Close();

            return content;
        }
    }
}
