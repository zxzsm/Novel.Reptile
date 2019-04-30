using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using Novel.Reptile.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Novel.Reptile.BookLink
{
    public abstract class LinkBase
    {
        protected BookLinks Links { get; set; }
        public LinkBase(BookLinks bookLinks)
        {
            this.Links = bookLinks;
        }
        protected static readonly string ImageDomain = "http://image.shukelai.com";

        public Book AddBook(BookContext bookContext, string bookName, string imageUrl, string summary, string author)
        {
            string spell = Pinyin.GetPinyin(bookName).Replace(" ", "");
            string filePath = string.Format("{0}{1}{2}", ConstCommon.SAVEFOLDER, spell, Path.GetExtension(imageUrl));
            //书籍图书url
            string bookImageUrl = "";
            if (CheckImageUrl(imageUrl))
            {
                //保存图片
                SaveImageUrl(imageUrl, filePath);
                bookImageUrl = ImageDomain + "/files/bookimages/" + spell + Path.GetExtension(imageUrl);
            }
            else
            {
                bookImageUrl = ImageDomain + "/files/bookimages/nocover.png";
            }
            var book = new Book
            {
                BookName = bookName,
                BookAuthor = author,
                BookReleaseTime = DateTime.Today,
                BookState = 0,
                BookSummary = summary,
                BookImage = bookImageUrl,
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                ReadVolume = 0
            };
            bookContext.Book.Add(book);
            return bookContext.SaveChanges() > 0 ? book : null;
        }

        public bool CheckImageUrl(string url)
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync(url).Result;
                return response.IsSuccessStatusCode;
            }
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

        public abstract void ReadLink();

        public void Task(BookContext bookContext, IHtmlCollection<IElement> metas, int syncType, bool saveChanges = false)
        {
            var url = metas.FirstOrDefault(m => m.OuterHtml.Contains("og:novel:read_url")) as IHtmlMetaElement;
            if (url == null)
            {
                return;
            }
            //小说书名
            var title = metas.FirstOrDefault(m => m.OuterHtml.Contains("og:novel:book_name")) as IHtmlMetaElement;
            if (title == null)
            {
                return;
            }
            if (bookContext.BookReptileTask.Any(m => m.Url == url.Content))
            {
                Console.WriteLine("任务已存在:" + title.Content);
                return;
            }
            //image
            var image = metas.FirstOrDefault(m => m.OuterHtml.Contains("og:image")) as IHtmlMetaElement;
            //简介
            var summary = metas.FirstOrDefault(m => m.OuterHtml.Contains("og:description")) as IHtmlMetaElement;
            //作者
            var author = metas.FirstOrDefault(m => m.OuterHtml.Contains("og:novel:author")) as IHtmlMetaElement;

            var bookName = title.Content;
            var book = bookContext.Book.FirstOrDefault(m => m.BookName == bookName);
            if (book == null)
            {
                book = AddBook(bookContext, bookName, image.ContentToString(), summary.ContentToString(), author.ContentToString());
                if (book == null)
                {
                    Console.WriteLine("添加书籍失败:" + bookName);
                    return;
                }

                Console.WriteLine("添加书籍任务:" + title.Content);
                bookContext.BookReptileTask.Add(new BookReptileTask
                {
                    BookId = book.BookId,
                    BookName = book.BookName,
                    SyncType = syncType,
                    Url = url.Content,
                    CurrentRecod = "",
                    Created = DateTime.Now,
                    Updated = DateTime.Now
                });
                if (saveChanges)
                {
                    bookContext.SaveChanges();
                }

            }
            else
            {
                Console.WriteLine("书籍已存在:" + title.Content);
            }
        }
    }
}
