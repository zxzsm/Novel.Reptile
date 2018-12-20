using AngleSharp.Parser.Html;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Novel.Reptile.Sync
{
    /// <summary>
    /// https://www.kanshushenzhan.com/
    /// </summary>
    public class KSSZBookGrah : BaseBookGrah
    {
        public async System.Threading.Tasks.Task GrahAsync()
        {
            string html = "";
            using (HttpClient httpClient = new HttpClient())
            {
                string v = await httpClient.GetStringAsync(Url);

            }
            var parser = new HtmlParser();
            var document = parser.Parse(html);

        }
    }
}
