﻿using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Novel.Reptile.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Novel.Reptile.Sync
{
    /// <summary>
    /// http://www.shuge.net
    /// </summary>
    public class ShuGeBookGrah : BaseBookGrah
    {
        public ShuGeBookGrah()
        {
            Type = 3;
        }
        public override void Grah()
        {
            string domainUrl = GetDomainUriString(new Uri(Url));
            string html = GetResponse(Url, "utf-8");
            var parser = new HtmlParser();
            var document = parser.Parse(html);

            var metas = document.QuerySelectorAll("meta");
            var category = metas.FirstOrDefault(m => m.OuterHtml.Contains("og:novel:category")) as IHtmlMetaElement;
            //book name
            var bookName = document.QuerySelector("#info h1").InnerHtml;
            Console.WriteLine("{0}:读取中........................................", bookName);
            //图片
            string imageUrl = domainUrl + document.QuerySelector("#fmimg img").GetAttribute("src");
            //作者
            string author = document.QuerySelectorAll("#info p")[0].InnerHtml;
            author = author.Split(new char[] { ':', '：' }, StringSplitOptions.RemoveEmptyEntries)[1];
            //简介
            var summary = document.QuerySelector("#intro").InnerHtml;



            string spell = Pinyin.GetPinyin(bookName).Replace(" ", "");
            string filePath = string.Format("{0}{1}{2}", ConstCommon.SAVEFOLDER, spell, Path.GetExtension(imageUrl));
            //保存图片
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
            Book book = GetBook(bookName, author, summary, ImageDomain+"/files/bookimages/" + spell + Path.GetExtension(imageUrl));
            if (category != null)
            {
                UpdateCatetory(book, category.Content);
            }
            BookReptileTask reptileTask = GetBookReptileTask(book);

            try
            {
                var childNodes = document.QuerySelector("#list dl").Children;

                //目录
                var lastItem = childNodes.LastOrDefault();
                var last = lastItem.QuerySelector("a");
                if (!string.IsNullOrWhiteSpace(reptileTask.CurrentRecod) && last != null && last.InnerHtml.Trim() == reptileTask.CurrentRecod.Trim())
                {
                    Console.WriteLine("{0}:已同步到最后结束........................................", bookName);
                    return;
                }
                bool isSaveChange = string.IsNullOrWhiteSpace(reptileTask.CurrentRecod);
                var bookLasItem = DB.BookItem.Where(m => m.BookId == book.BookId).OrderByDescending(m => m.Pri).FirstOrDefault();
                int pri = bookLasItem == null ? 0 : bookLasItem.Pri;
                int dt = 0;
                foreach (var node in childNodes)
                {
                    if (node.NodeName.ToLower() == "dt")
                    {
                        dt++;
                        continue;
                    }
                    if (dt >= 2)
                    {
                        var item = node.QuerySelector("a");
                        if (!isSaveChange && item.InnerHtml.Trim() == reptileTask.CurrentRecod.Trim())
                        {
                            string itemName = item.InnerHtml.Trim();
                            var bookItem = DB.BookItem.FirstOrDefault(m => m.BookId == book.BookId && m.ItemName == itemName);
                            if (bookItem != null && string.IsNullOrWhiteSpace(bookItem.Content))
                            {
                                string href = item.GetAttribute("href");
                                href = "https://www.mxzw.com" + href;
                                string content = GetContent(href);
                                bookItem.Content = content;
                                bookItem.UpdateTime = DateTime.Now;
                                DB.SaveChanges();
                            }
                            isSaveChange = true;
                            continue;
                        }
                        if (isSaveChange)
                        {
                            string itemName = item.InnerHtml.Trim();
                            string href = item.GetAttribute("href");
                            href = "https://www.mxzw.com" + href;
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
                    }
                }
            }
            catch (Exception ex)
            {
                reptileTask.Remark = DateTime.Now + ":" + ex.Message;
                reptileTask.Updated = DateTime.Now;
                DB.SaveChanges();
                throw;
            }
            Console.WriteLine("{0}:结束........................................", bookName);
        }

        private string GetContent(string url)
        {
            var html = GetResponse(url, "utf-8");
            var parser = new HtmlParser();
            var document = parser.Parse(html);
            var content = document.GetElementById("content").InnerHtml;
            if (!string.IsNullOrWhiteSpace(content))
            {
                content = content.Remove(content.IndexOf("<script>"));
            }
            return content;
        }


        private void UpdateCatetory(Book book, string catetory)
        {
            if (string.IsNullOrEmpty(catetory) || book == null)
            {
                return;
            }
            int catetoryId = 0;
            catetory = catetory.Trim();
            switch (catetory)
            {
                case "玄幻奇幻":
                    catetoryId = 1;
                    break;
                case "都市言情":
                    catetoryId = 2;
                    break;
                case "武侠仙侠":
                    catetoryId = 3;
                    break;
                case "历史军事":
                    catetoryId = 4;
                    break;
                case "网游竞技":
                    catetoryId = 5;
                    break;
                case "科幻灵异":
                    catetoryId = 6;
                    break;
                default:
                    catetoryId = 0;
                    break;
            }
            UpdateCatetory(book, catetoryId);
        }
    }
}
