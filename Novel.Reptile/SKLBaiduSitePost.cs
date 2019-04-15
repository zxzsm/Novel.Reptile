using Novel.Reptile.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Novel.Reptile
{
    public class SKLBaiduSitePost
    {
        static SKLBaiduSitePost()
        {
            Init();
        }
        static string pcurl = "http://data.zz.baidu.com/urls?site=www.shukelai.com&token=tQJUH7DgE2boNBcM";
        static string mobileurl = "http://data.zz.baidu.com/urls?site=m.shukelai.com&token=tQJUH7DgE2boNBcM";
        static void Init()
        {
            using (BookContext bookContext = new BookContext())
            {
                AddLinksubmit(bookContext, pcurl, LinkSubmitType.Book, 1);
                AddLinksubmit(bookContext, pcurl, LinkSubmitType.Item, 1);
                AddLinksubmit(bookContext, mobileurl, LinkSubmitType.Book, 1);
                AddLinksubmit(bookContext, mobileurl, LinkSubmitType.Item, 1);
                bookContext.SaveChanges();
            }
        }

        protected static void AddLinksubmit(BookContext bookContext, string url, LinkSubmitType type, int currentId)
        {
            var lmt = bookContext.Linksubmit.FirstOrDefault(m => m.Type == (int)type && m.WebUrl == url);
            if (lmt == null)
            {
                lmt = new Linksubmit
                {
                    Type = (int)type,
                    WebUrl = url,
                    CurrentId = currentId,
                };
                bookContext.Linksubmit.Add(lmt);
            }
        }


        public static void PCSubmit()
        {
            using (BookContext bookContext = new BookContext())
            {
                Console.WriteLine("百度链接提交数据中.......");
                var lmt = bookContext.Linksubmit.FirstOrDefault(m => m.WebUrl == pcurl && m.Type == (int)LinkSubmitType.Item);
                if (lmt == null)
                {
                    return;
                }
                int itemId = lmt.CurrentId + 1999;
                var bookItemIds = bookContext.BookItem.Where(m => m.ItemId >= lmt.CurrentId && m.ItemId <= itemId).Select(m => m.ItemId).ToList();
                if (lmt.CurrentId == bookItemIds.Max())
                {
                    Console.WriteLine("已提交至最新数据");
                    return;
                }
                bool isWhile = bookItemIds.Any();
                while (isWhile)
                {

                    itemId = bookItemIds.Max();
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var item in bookItemIds)
                    {
                        Console.WriteLine("提交中:" + item);
                        stringBuilder.AppendFormat("www.shukelai.com/content/{0}.html", item).AppendLine();
                    }
                    string msg = GetPage(pcurl, stringBuilder.ToString());
                    if (msg.Contains("error"))
                    {
                        lmt.Result = msg;
                        isWhile = false;
                        lmt.UpdatedTime = DateTime.Now;
                        bookContext.SaveChanges();
                        return;
                    }
                    else
                    {
                        lmt.CurrentId = itemId;
                        lmt.Result = msg;
                        lmt.UpdatedTime = DateTime.Now;
                    }
                    bookContext.SaveChanges();
                    Console.WriteLine("百度提交已处理:" + itemId + ".....");
                    itemId = lmt.CurrentId + 1999;
                    bookItemIds = bookContext.BookItem.Where(m => m.ItemId >= lmt.CurrentId && m.ItemId <= itemId).Select(m => m.ItemId).ToList();
                    isWhile = bookItemIds.Any();

                }
                Console.WriteLine("百度链接提交结束.......");

            }
        }
        public static string GetPage(string posturl, string postData)
        {
            Stream outstream = null;
            Stream instream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            Encoding encoding = System.Text.Encoding.GetEncoding("gb2312");
            byte[] data = encoding.GetBytes(postData);
            // 准备请求...
            try
            {
                // 设置参数
                request = WebRequest.Create(posturl) as HttpWebRequest;
                CookieContainer cookieContainer = new CookieContainer();
                request.UserAgent = "curl/7.12.1";

                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "POST";

                request.ContentType = "text/plain";
                request.ContentLength = data.Length;
                outstream = request.GetRequestStream();
                outstream.Write(data, 0, data.Length);
                outstream.Close();
                //发送请求并获取相应回应数据
                response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                instream = response.GetResponseStream();
                sr = new StreamReader(instream, encoding);
                //返回结果网页（html）代码
                string content = sr.ReadToEnd();
                string err = string.Empty;
                return content;
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                return err;
            }
        }
    }
    public enum LinkSubmitType
    {
        Book = 1,
        Item = 2
    }
}
