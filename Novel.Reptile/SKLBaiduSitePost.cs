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
                AddLinksubmit(bookContext, pcurl, LinkSubmitType.PCBook, 0);
                AddLinksubmit(bookContext, mobileurl, LinkSubmitType.MobileBook, 0);
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

        private static void PCSubmit(LinkSubmitType linkSubmitType)
        {
            using (BookContext bookContext = new BookContext())
            {
                Console.WriteLine("百度链接提交数据中.......");
                string domain = "";
                if (linkSubmitType == LinkSubmitType.PCBook)
                {
                    domain = "http://www.shukelai.com";
                }
                else
                {
                    domain = "http://m.shukelai.com";
                }
                var lmt = bookContext.Linksubmit.FirstOrDefault(m => m.Type == (int)linkSubmitType);
                if (lmt == null)
                {
                    return;
                }
                var books = bookContext.Book.Where(m => m.BookId > lmt.CurrentId);
                if (lmt.CurrentId == bookContext.Book.Max(m => m.BookId))
                {
                    Console.WriteLine("已提交至最新数据");
                    return;
                }
                var bks = books.Select(m => new { m.BookName, m.BookId }).OrderByDescending(m => m.BookId).ToList();
                int count = 1;
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var item in bks)
                {
                    Console.WriteLine("组装中:" + item.BookName);
                    stringBuilder.AppendFormat("{0}/book/{1}.html", domain, item.BookId).AppendLine();
                    if (count >= 2000)
                    {
                        Console.WriteLine("提交中..............");
                        string msg = GetPage(lmt.WebUrl, stringBuilder.ToString());
                        lmt.Result = msg;
                        lmt.UpdatedTime = DateTime.Now;
                        Console.WriteLine("提交返回信息:" + msg);
                        if (msg.Contains("error"))
                        {
                            bookContext.SaveChanges();
                            Console.WriteLine("报错结束");
                            return;
                        }
                        else
                        {
                            Console.WriteLine("提交成功..............");
                            lmt.CurrentId = item.BookId;
                            bookContext.SaveChanges();
                        }
                        count = 1;
                        stringBuilder.Clear();
                    }
                    count++;
                }
                if (!string.IsNullOrWhiteSpace(stringBuilder.ToString()))
                {
                    string msg = GetPage(lmt.WebUrl, stringBuilder.ToString());
                    lmt.Result = msg;
                    lmt.UpdatedTime = DateTime.Now;
                    lmt.CurrentId = bks.Max(m => m.BookId);
                    bookContext.SaveChanges();
                    Console.WriteLine("提交返回信息:" + msg);

                }

                Console.WriteLine("百度链接提交结束.......");

            }
        }
        public static void PCSubmit()
        {
            PCSubmit(LinkSubmitType.PCBook);
            PCSubmit(LinkSubmitType.MobileBook);
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
        PCBook = 1,
        MobileBook = 2
    }
}
