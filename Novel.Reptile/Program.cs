
using AngleSharp.Parser.Html;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Novel.Reptile
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = "https://www.23us.so/xiaoshuo/43.html";
            DingDian(url);
        }

        //顶点抓取
        private static void DingDian(string url)
        {
            var html = GetResponse(url);
            var parser = new HtmlParser();
            var document = parser.Parse(html);
            var content = document.GetElementById("content");
            var title = content.QuerySelector("dd h1").InnerHtml.Replace("全文阅读", "").Trim();
            string imageUrl = content.QuerySelector("dd .fl .hst img").GetAttribute("src");
            string author = content.QuerySelectorAll("dd .fl table tbody tr td")[1].InnerHtml;
            var summary = content.QuerySelectorAll("dd")[3].QuerySelectorAll("p")[1].InnerHtml;

            string filePath = string.Format("{0}{1}{2}", GetSaveFolder(), Pinyin.GetPinyin(title).Replace(" ", ""), Path.GetExtension(imageUrl));
            SaveImageUrl(imageUrl, filePath);

            Console.WriteLine(html);
        }
        public static string GetResponse(string url)
        {
            string result = string.Empty;
            using (HttpClient httpClient = new HttpClient(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip }))
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/html"));
                HttpResponseMessage response = httpClient.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    response.Content.Headers.ContentType.CharSet = "utf-8";
                    var t = response.Content.ReadAsStringAsync();
                    result = t.Result;
                }
            }
            return result;
        }

        public static string GetSaveFolder()
        {
            string folder = Directory.GetCurrentDirectory();
            string saveFolder = string.Format("{0}{1}", folder, "/files/bookimages/");
            if (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
            }
            return saveFolder;
        }
        public static void SaveImageUrl(string url, string saveFilePath)
        {
            var client = new HttpClient();
            System.IO.FileStream fs;
            //文件名：序号+.jpg。可指定范围，以下是获取100.jpg~500.jpg.
            var uri = new Uri(Uri.EscapeUriString(url));
            byte[] urlContents = client.GetByteArrayAsync(uri).Result;
            fs = new System.IO.FileStream(saveFilePath, System.IO.FileMode.OpenOrCreate);
            fs.Write(urlContents, 0, urlContents.Length);

        }
    }
}
