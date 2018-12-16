using AngleSharp.Parser.Html;
using Novel.Reptile.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace Novel.Reptile.Sync
{
    /// <summary>
    /// http://www.biquyun.com
    /// </summary>
    public class BiQuYunBookGrah : BaseBookGrah
    {
        public BiQuYunBookGrah()
        {
            Type = 2;
        }
        public override void Grah()
        {
            var html = GetResponse(Url, "GBK");
            var parser = new HtmlParser();
            var document = parser.Parse(html);
            //book name
            var bookName = document.QuerySelector("#info h1").InnerHtml;
            //图片
            string imageUrl = document.QuerySelector("#fmimg img").GetAttribute("src");
            //作者
            string author = document.QuerySelectorAll("#info p")[0].InnerHtml;
            author = author.Split(new char[] { ':', '：' }, StringSplitOptions.RemoveEmptyEntries)[1];
            //简介
            var summary = document.QuerySelector("#intro").InnerHtml;
            string spell = Pinyin.GetPinyin(bookName).Replace(" ", "");
            string filePath = string.Format("{0}{1}{2}", ConstCommon.SAVEFOLDER, spell, Path.GetExtension(imageUrl));
            SaveImageUrl(imageUrl, filePath);
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
                    BookImage = "/files/bookimages/" + spell + Path.GetExtension(imageUrl),
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                    ReadVolume = 0
                };
                DB.Book.Add(book);
            }
            DB.SaveChanges();

            BookReptileTask reptileTask = DB.BookReptileTask.FirstOrDefault(m => m.BookId == book.BookId && m.SyncType == Type);
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
                    Url = Url,
                };
                DB.BookReptileTask.Add(reptileTask);
                DB.SaveChanges();
            }


            //目录
            var items = document.QuerySelectorAll("#list dd a");

            bool isSaveChange = string.IsNullOrWhiteSpace(reptileTask.CurrentRecod);
            foreach (var item in items)
            {
                if (isSaveChange)
                {
                    string href = item.GetAttribute("href");
                    href = "http://www.biquyun.com" + href;
                    string itemName = item.InnerHtml;
                    string content = GetContent(href);
                    BookItem bookItem = new BookItem()
                    {
                        BookId = book.BookId,
                        ItemName = itemName,
                        CreateTime = DateTime.Now,
                        Content = content,
                        UpdateTime = DateTime.Now
                    };
                    Console.WriteLine("{0}:{1}:{2}", book.BookName, bookItem.ItemName, href);
                    DB.BookItem.Add(bookItem);
                    reptileTask.CurrentRecod = item.InnerHtml.Trim();
                    reptileTask.Updated = DateTime.Now;
                    DB.SaveChanges();
                    Thread.Sleep(1000);
                    continue;
                }
                if (!isSaveChange && item.InnerHtml.Trim() == reptileTask.CurrentRecod.Trim())
                {
                    isSaveChange = true;
                    continue;
                }
            }

        }
        private string GetContent(string url)
        {
            var html = GetResponse(url,"GBK");
            var parser = new HtmlParser();
            var document = parser.Parse(html);
            var content = document.GetElementById("content");
            return content.InnerHtml;
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


    }
}
