using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ribo_Downloader.Classes
{
    public class YoutubeLink
    {
        string ID, Link;

        public YoutubeLink(string link)
        {
            this.Link = link;
        }
        public string getDownloadLink()
        {
            ID = getId(Link);
            if(ID == "")
            {
                return "";
            }
            return getRequest();
        }
        private string getRequest()
        {
            string url = "http://www.seesub.com/api/yt" + ID;
            Requester req = new Requester(url);
            var data = JsonConvert.DeserializeObject<QualityList>(req.GetContent());
            return data.data[0].url;
        }
        private string getId(string link)
        {
            string[] arrStr = link.Split(new char[] { '?' }, StringSplitOptions.None);

            if (arrStr.Length < 2 || arrStr[1].Trim() == "" || arrStr[1].IndexOf("v=") < 0)
            {
                return "";
            }

            link = arrStr[1];

            arrStr = link.Split(new char[] { '&' }, StringSplitOptions.None);

            foreach(string str in arrStr)
            {
                if(str.IndexOf("v=") > -1)
                {
                    return str.Replace("v=", "");
                }
            }
            return "";
        }
    }
    public class QualityModel
    {
        public string quality { get; set; }
        public string url { get; set; }
    }
    public class QualityList
    {
        public List<QualityModel> data { get; set; }
    }
}
