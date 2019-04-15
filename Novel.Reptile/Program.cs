
using AngleSharp.Parser.Html;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Linq;
using Novel.Reptile.Sync;
using System.Text;
using Novel.Reptile.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Configuration;
using System.Web;

namespace Novel.Reptile
{
    class Program
    {
        static MyTaskList myTaskList = new MyTaskList();
        static void Main(string[] args)
        {
            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");
            var configuration = builder.Build();
            ConstCommon.SAVEFOLDER = configuration["ImageSavePath"];
            ConstCommon.ConnectionString = configuration.GetConnectionString("BookDatabase");

            myTaskList.Completed += MyTaskList_Completed;
            StartTask(myTaskList);

            //string url = "https://www.baidu.com/link?url=6DkLbC9K2EZnxGC64UD-SiEa1zU5XElCmp3YAoPhEFW&amp;wd=&amp;eqid=d465c32a0001869d000000055c739001";
            //var s = HttpHelper.HttpGet(url, "text/html");
            //LinkSplit.ReadLinkTxtForBiQuYun(@"C:\Users\Xiang\Desktop\js页面抓取\笔趣云抓取.txt");
            Console.ReadLine();
        }

        private static void MyTaskList_AllTaskCompleted()
        {
            Console.WriteLine("新任务开始，重新启动");
            //StartTask(myTaskList);
        }

        private static void StartTask(MyTaskList myTaskList)
        {
            Console.WriteLine("一个任务开始！！！！！！！！！！！！！！！");
            if (myTaskList == null)
            {
                myTaskList = new MyTaskList();
            }
            myTaskList.Tasks.Clear();
            List<BookReptileTask> task = null;
            using (BookContext context = new BookContext())
            {
                var book = context.Book.Where(m => m.BookState.HasValue && m.BookState.Value == 1);
                task = context.BookReptileTask.Where(m => !book.Any(p => p.BookId == m.BookId)).OrderBy(m => m.Id).ToList();
            }
            foreach (var item in task)
            {
                IBookGrah grah = null;
                if (item.SyncType == 2)
                {
                    grah = new BiQuYunBookGrah();
                }
                else if (item.SyncType == 3)
                {
                    grah = new ShuGeBookGrah();
                }
                if (grah != null)
                {
                    grah.Url = item.Url;
                    myTaskList.Tasks.Add(grah.Grah);
                }
            }
            myTaskList.Start();
        }

        private static void MyTaskList_Completed()
        {
            using (BookContext db = new BookContext())
            {
                db.TaskToDo.Add(new TaskToDo { EndTime = DateTime.Now, Remark = "任务结束" });
                db.SaveChanges();
            }
            Console.WriteLine("一个任务结束！！！！！！！！！！！！！！！");
            //休息30分钟
            Task task = Task.Delay(1000 * 60 * 30);
            task.Wait();
            StartTask(myTaskList);
        }


        static string GetHtml(string keyword)
        {
            string url = @"http://www.baidu.com/";
            string encodedKeyword = HttpUtility.UrlEncode(keyword, Encoding.GetEncoding(936));
            //百度使用codepage 936字符编码来作为查询串，果然专注于中文搜索……
            //更不用说，还很喜欢微软
            //谷歌能正确识别UTF-8编码和codepage这两种情况，不过本身网页在HTTP头里标明是UTF-8的
            //估计谷歌也不讨厌微软（以及微软的专有规范）
            string query = "s?wd=" + encodedKeyword;

            HttpWebRequest req;
            HttpWebResponse response;
            Stream stream;
            req = (HttpWebRequest)WebRequest.Create(url + query);
            response = (HttpWebResponse)req.GetResponse();
            stream = response.GetResponseStream();
            int count = 0;
            byte[] buf = new byte[8192];
            string decodedString = null;
            StringBuilder sb = new StringBuilder();
            try
            {
                Console.WriteLine("正在读取网页{0}的内容……", url + query);
                do
                {
                    count = stream.Read(buf, 0, buf.Length);
                    if (count > 0)
                    {
                        decodedString = Encoding.GetEncoding(936).GetString(buf, 0, count);
                        sb.Append(decodedString);
                    }
                } while (count > 0);
            }
            catch
            {
                Console.WriteLine("网络连接失败，请检查网络设置。");
            }
            return sb.ToString();
        }


    }
}
