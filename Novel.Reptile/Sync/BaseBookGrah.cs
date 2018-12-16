using Novel.Reptile.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Novel.Reptile.Sync
{
    public class BaseBookGrah : IBookGrah, IDisposable
    {
        public virtual BookContext DB
        {
            get;
            protected set;
        }
        public BaseBookGrah(string url) : this()
        {
            this.Url = url;
        }
        public BaseBookGrah()
        {
            DB = new BookContext();
        }

        public string Url { get; set; }
        protected int Type { get;  set; }

        public virtual void Grah()
        {

        }
        public string GetResponse(string url,string charset= "utf-8")
        {
            string result = string.Empty;
            using (HttpClient httpClient = new HttpClient(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip }))
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/html"));
                HttpResponseMessage response = httpClient.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    response.Content.Headers.ContentType.CharSet = charset;
                    var t = response.Content.ReadAsStringAsync();
                    result = t.Result;
                }
            }
            return result;
        }

        public void Dispose()
        {
            DB = null;
        }
    }
}
