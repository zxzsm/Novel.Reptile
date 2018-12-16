using AngleSharp.Parser.Html;
using Novel.Reptile.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Linq;
using System.Threading;

namespace Novel.Reptile.Sync
{
    public class DingDianBookGrah : BaseBookGrah
    {

        public override void Grah()
        {
            var html = GetResponse(Url);
            var parser = new HtmlParser();
            var document = parser.Parse(html);
            var content = document.GetElementById("content");
            //book name
            var bookName = content.QuerySelector("dd h1").InnerHtml.Replace("全文阅读", "").Trim();
            //图片
            string imageUrl = content.QuerySelector("dd .fl .hst img").GetAttribute("src");
            //作者
            string author = content.QuerySelectorAll("dd .fl table tbody tr td")[1].InnerHtml;
            //简介
            var summary = content.QuerySelectorAll("dd")[3].QuerySelectorAll("p")[1].InnerHtml;
            Book book = null;
            BookReptileTask reptileTask;
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
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                    ReadVolume = 0
                };
                DB.Book.Add(book);
            }
            DB.SaveChanges();

            reptileTask = DB.BookReptileTask.FirstOrDefault(m => m.BookId == book.BookId && m.SyncType == 1);
            if (reptileTask == null)
            {
                reptileTask = new BookReptileTask
                {
                    BookId = book.BookId,
                    BookName = book.BookName,
                    SyncType = 1,
                    Created = DateTime.Now,
                    CurrentRecod = "",
                    Updated = DateTime.Now,
                    Url = Url,
                };
                DB.BookReptileTask.Add(reptileTask);
                DB.SaveChanges();
            }
            //目录
            var itemsUrl = content.QuerySelector(".btnlinks .read").GetAttribute("href");
            SaveItems(book, reptileTask, itemsUrl);
        }

        private void SaveItems(Book book, BookReptileTask reptileTask, string url)
        {
            var html = GetResponse(url);
            var parser = new HtmlParser();
            var document = parser.Parse(html);
            var items = document.QuerySelectorAll(".bdsub dl table  tr td a");

            bool isSaveChange = string.IsNullOrWhiteSpace(reptileTask.CurrentRecod);
            foreach (var item in items)
            {
                if (isSaveChange)
                {
                    string href = item.GetAttribute("href");
                    string itemName = item.InnerHtml;
                    string content = GetContent(href);
                    BookItem bookItem = new BookItem()
                    {
                        BookId = book.BookId,
                        ItemName=itemName,
                        CreateTime = DateTime.Now,
                        Content = content,
                        UpdateTime = DateTime.Now
                    };
                    Console.WriteLine("{0}:{1}:{2}",book.BookName, bookItem.ItemName,href);
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
            var html = GetResponse(url);
            var parser = new HtmlParser();
            var document = parser.Parse(html);
            var content = document.GetElementById("contents");
            return content.InnerHtml;
        }

      
    }
}
