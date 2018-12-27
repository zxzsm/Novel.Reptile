using AngleSharp.Dom;
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
            Console.WriteLine("{0}:读取中........................................", bookName);
            //图片
            string imageUrl = document.QuerySelector("#fmimg img").GetAttribute("src");
            //作者
            string author = document.QuerySelectorAll("#info p")[0].InnerHtml;
            author = author.Split(new char[] { ':', '：' }, StringSplitOptions.RemoveEmptyEntries)[1];
            //简介
            var summary = document.QuerySelector("#intro").InnerHtml;
            string spell = Pinyin.GetPinyin(bookName).Replace(" ", "");
            string filePath = string.Format("{0}{1}{2}", ConstCommon.SAVEFOLDER, spell, Path.GetExtension(imageUrl));
            //保存图片
            SaveImageUrl(imageUrl, filePath);
            Book book = GetBook(bookName, author, summary, "/files/bookimages/" + spell + Path.GetExtension(imageUrl));
            BookReptileTask reptileTask = GetBookReptileTask(book);
            try
            {
                //目录
                var items = document.QuerySelectorAll("#list dd a");
                var last = items.LastOrDefault();
                if (!string.IsNullOrWhiteSpace(reptileTask.CurrentRecod) && last != null && last.InnerHtml.Trim() == reptileTask.CurrentRecod.Trim())
                {
                    Console.WriteLine("{0}:已同步到最后结束........................................", bookName);
                    return;
                }
                bool isSaveChange = string.IsNullOrWhiteSpace(reptileTask.CurrentRecod);
                var bookLasItem = DB.BookItem.Where(m => m.BookId == book.BookId).OrderByDescending(m => m.Pri).FirstOrDefault();
                int pri = bookLasItem == null ? 0 : bookLasItem.Pri;
                foreach (var item in items)
                {
                    if (isSaveChange)
                    {
                        string itemName = item.InnerHtml.Trim();
                        string href = item.GetAttribute("href");
                        href = "http://www.biquyun.com" + href;
                        string content = GetContent(href);
                        var bookItem = DB.BookItem.FirstOrDefault(m => m.BookId == book.BookId && m.ItemName == itemName);
                        if (bookItem == null)
                        {
                            pri++;
                            bookItem = new BookItem()
                            {
                                BookId = book.BookId,
                                ItemName = itemName,
                                CreateTime = DateTime.Now,
                                Content = content,
                                UpdateTime = DateTime.Now,
                                Pri = pri
                            };
                            DB.BookItem.Add(bookItem);
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(bookItem.Content))
                            {
                                bookItem.Content = content;
                            }
                        }
                        Console.WriteLine("{0}:{1}:{2}", book.BookName, bookItem.ItemName, href);
                        reptileTask.CurrentRecod = itemName;
                        reptileTask.Updated = DateTime.Now;
                        book.UpdateTime = DateTime.Today;
                        DB.SaveChanges();
                        continue;
                    }
                    if (!isSaveChange && item.InnerHtml.Trim() == reptileTask.CurrentRecod.Trim())
                    {

                        string itemName = item.InnerHtml.Trim();
                        var bookItem = DB.BookItem.FirstOrDefault(m => m.BookId == book.BookId && m.ItemName == itemName);
                        if (bookItem != null && string.IsNullOrWhiteSpace(bookItem.Content))
                        {
                            string href = item.GetAttribute("href");
                            href = "http://www.biquyun.com" + href;
                            string content = GetContent(href);
                            bookItem.Content = content;
                            bookItem.UpdateTime = DateTime.Now;
                            DB.SaveChanges();
                        }
                        isSaveChange = true;
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                reptileTask.Remark = DateTime.Now + ex.Message;
                reptileTask.Updated = DateTime.Now;
                DB.SaveChanges();
                throw ex;
            }
            Console.WriteLine("{0}:结束........................................", bookName);
        }
        private string GetContent(string url)
        {
            var html = GetResponse(url, "GBK");
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
