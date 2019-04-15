using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Novel.Reptile.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Novel.Reptile
{
    public class LinkSplit
    {

        public static void ReadLinkTxt(string path)
        {
            string[] links = null;
            using (StreamReader sr = System.IO.File.OpenText(path))
            {
                links = sr.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            }
            if (links == null)
            {
                return;
            }
            using (BookContext bookContext = new BookContext())
            {
                foreach (var lk in links)
                {
                    var html = HttpHelper.HttpGet(lk, "text/html");
                    var parser = new HtmlParser();
                    var document = parser.Parse(html);
                    var metas = document.QuerySelectorAll("meta");
                    var url = metas.FirstOrDefault(m => m.OuterHtml.Contains("og:novel:read_url")) as IHtmlMetaElement;
                    var title = metas.FirstOrDefault(m => m.OuterHtml.Contains("og:novel:book_name")) as IHtmlMetaElement;
                    if (title == null)
                    {
                        continue;
                    }
                    var book = bookContext.Book.FirstOrDefault(m => m.BookName == title.Content);
                    if (book == null)
                    {
                        if (bookContext.BookReptileTask.Any(m => m.Url == url.Content))
                        {
                            Console.WriteLine("任务已存在:" + title.Content);
                            continue;
                        }
                        Console.WriteLine("添加书籍任务:" + title.Content);
                        bookContext.BookReptileTask.Add(new BookReptileTask
                        {
                            SyncType = 3,
                            Url = url.Content,
                            CurrentRecod = "",
                            Created = DateTime.Now,
                            Updated = DateTime.Now
                        });
                        bookContext.SaveChanges();
                    }
                    else
                    {
                        Console.WriteLine("书籍已存在:" + title.Content);
                    }
                    //var bookName = document.QuerySelector("#info h1").InnerHtml;
                }

            }
            //string[] xx = sr 

        }
        public static void ReadLinkTxtForBiQuYun(string path)
        {
            string[] links = null;
            using (StreamReader sr = System.IO.File.OpenText(path))
            {
                links = sr.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            }
            if (links == null)
            {
                return;
            }
            using (BookContext bookContext = new BookContext())
            {
                foreach (var lk in links)
                {
                    var html = HttpHelper.HttpGet(lk);
                    var parser = new HtmlParser();
                    var document = parser.Parse(html);
                    var metas = document.QuerySelectorAll("meta");
                    var url = metas.FirstOrDefault(m => m.OuterHtml.Contains("og:novel:read_url")) as IHtmlMetaElement;
                    var title = metas.FirstOrDefault(m => m.OuterHtml.Contains("og:novel:book_name")) as IHtmlMetaElement;
                    if (title == null)
                    {
                        continue;
                    }
                    var book = bookContext.Book.FirstOrDefault(m => m.BookName == title.Content);
                    if (book == null)
                    {
                        if (bookContext.BookReptileTask.Any(m => m.Url == url.Content))
                        {
                            Console.WriteLine("任务已存在:" + title.Content);
                            continue;
                        }
                        Console.WriteLine("添加书籍任务:" + title.Content);
                        bookContext.BookReptileTask.Add(new BookReptileTask
                        {
                            SyncType = 2,
                            Url = url.Content,
                            CurrentRecod = "",
                            Created = DateTime.Now,
                            Updated = DateTime.Now
                        });
                        bookContext.SaveChanges();
                    }
                    else
                    {
                        Console.WriteLine("书籍已存在:" + title.Content);
                    }
                    //var bookName = document.QuerySelector("#info h1").InnerHtml;
                }

            }
            //string[] xx = sr 

        }

    }
}
