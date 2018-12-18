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
        object _sync = new object();
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
            }
            return result;
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
    }
}
