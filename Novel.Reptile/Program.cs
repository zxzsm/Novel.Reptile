
using AngleSharp.Parser.Html;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Linq;
using Novel.Reptile.Sync;
using System.Text;
using Novel.Reptile.Entities;
using System.Collections.Generic;

namespace Novel.Reptile
{
    class Program
    {
        static void Main(string[] args)
        {
            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);
            string url = "http://www.biquyun.com/18_18727/";
            List<BookReptileTask> task = null;
            using (BookContext context = new BookContext())
            {
                task = context.BookReptileTask.ToList();

            }
            foreach (var item in task)
            {
                if (item.SyncType == 2)
                {
                    IBookGrah grah = new BiQuYunBookGrah();
                    grah.Url = item.Url;
                    grah.Grah();
                }

            }

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
            var itemsUrl = content.QuerySelector(".btnlinks .read").GetAttribute("href");
            //string filePath = string.Format("{0}{1}{2}", GetSaveFolder(), Pinyin.GetPinyin(title).Replace(" ", ""), Path.GetExtension(imageUrl));
            //SaveImageUrl(imageUrl, filePath);
            GetItems(itemsUrl);
            Console.WriteLine(html);
        }

        public static void GetItems(string url)
        {
            var html = GetResponse(url);
            var parser = new HtmlParser();
            var document = parser.Parse(html);

            var items = document.QuerySelectorAll(".bdsub dl table  tr td a");
            foreach (var item in items)
            {
                string href = item.GetAttribute("href");
                string itemName = item.InnerHtml;
                string content = GetContent(href);
            }

        }

        public static string GetContent(string url)
        {
            var html = GetResponse(url);
            var parser = new HtmlParser();
            var document = parser.Parse(html);
            var content = document.GetElementById("contents");
            return content.InnerHtml;
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
