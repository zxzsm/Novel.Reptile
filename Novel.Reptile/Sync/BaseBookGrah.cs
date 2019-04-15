using AngleSharp;
using AngleSharp.Parser.Html;
using Novel.Reptile.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Novel.Reptile.Sync
{
    public class BaseBookGrah : IBookGrah, IDisposable
    {
        object _sync = new object();

        protected readonly string ImageDomain = "http://image.shukelai.com";
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
        protected int Type { get; set; }

        public virtual void Grah()
        {

        }
        public string GetResponse(string url, string charset = "utf-8")
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
                else
                {
                    var t = response.Content.ReadAsStringAsync();
                    SimpleScriptingSample(t.Result);
                    //这句话是关键点
                    var cookies = response.Headers.GetValues("Set-Cookie");
                    if (cookies != null && cookies.Count() > 0)
                    {
                        string jsluid = cookies.First();
                        response.Headers.Add("Cookie", jsluid);
                        response = httpClient.GetAsync(url).Result;
                        response.Content.Headers.ContentType.CharSet = charset;
                        var r = response.Content.ReadAsStringAsync();
                        result = r.Result;
                        // __jsluid = bfe8ff4240ecb6f7aa65f0958aa30579; max - age = 31536000; path =/; HttpOnly
                    }

                }
            }
            return result;
        }
        void SimpleScriptingSample(string source)
        {
            //We require a custom configuration
            var config = Configuration.Default.WithJavaScript();
            //Let's create a new parser using this configuration
            var parser = new HtmlParser(config);

            //This is our sample source, we will set the title and write on the document
            //source = source.Replace("<script>", "").Replace("</script>", "").Replace("eval","return");
            var document = parser.Parse(source);

            //Modified HTML will be output
            Console.WriteLine(document.DocumentElement.OuterHtml);
        }
        public Book GetBook(string bookName, string author, string summary, string bookImage)
        {
            lock (_sync)
            {
                Book book = null;
                book = DB.Book.FirstOrDefault(m => m.BookName == bookName);
                if (book == null)
                {
                    book = new Book
                    {
                        BookName = bookName,
                        BookAuthor = author,
                        BookReleaseTime = DateTime.Today,
                        BookState = 0,
                        BookSummary = summary,
                        BookImage = bookImage,
                        CreateTime = DateTime.Now,
                        UpdateTime = DateTime.Now,
                        ReadVolume = 0
                    };
                    DB.Book.Add(book);
                    DB.SaveChanges();
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(book.BookImage))
                    {
                        book.BookImage = bookImage;
                    }
                }
                return book;
            }
        }

        public BookReptileTask GetBookReptileTask(Book book)
        {
            lock (_sync)
            {
                BookReptileTask reptileTask = DB.BookReptileTask.FirstOrDefault(m => m.Url == Url.Trim() && m.SyncType == Type);
                if (reptileTask == null)
                {
                    reptileTask = new BookReptileTask
                    {
                        BookId = book.BookId,
                        BookName = book.BookName,
                        SyncType = Type,
                        Created = DateTime.Now,
                        CurrentRecod = "",
                        Updated = DateTime.Now,
                        Url = Url.Trim(),
                    };
                    DB.BookReptileTask.Add(reptileTask);
                }
                if (!reptileTask.BookId.HasValue)
                {
                    reptileTask.BookId = book.BookId;
                    reptileTask.BookName = book.BookName;
                }
                DB.SaveChanges();
                return reptileTask;
            }
        }

        public void Dispose()
        {
            using (DB) { }

        }
        public void SaveImageUrl(string url, string saveFilePath)
        {
            if (File.Exists(saveFilePath))
            {
                return;
            }
            using (var client = new HttpClient())
            {
                System.IO.FileStream fs;
                //文件名：序号+.jpg。可指定范围，以下是获取100.jpg~500.jpg.
                var uri = new Uri(Uri.EscapeUriString(url));
                byte[] urlContents = client.GetByteArrayAsync(uri).Result;
                fs = new System.IO.FileStream(saveFilePath, System.IO.FileMode.OpenOrCreate);
                fs.Write(urlContents, 0, urlContents.Length);
            }

        }

        public string GetDomainUriString(Uri uri)
        {
            StringBuilder parentName = new StringBuilder();

            // Append the scheme: http, ftp etc.
            parentName.Append(uri.Scheme);

            // Appned the '://' after the http, ftp etc.
            parentName.Append("://");

            // Append the host name www.foo.com
            parentName.Append(uri.Host);
            parentName.Append("/");
            // Append each segment except the last one. The last one is the
            // leaf and we will ignore it.
           
            return parentName.ToString();
        }
    }
}
