
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
using Novel.Reptile.BookLink;

namespace Novel.Reptile
{
    class Program
    {
        static MyTaskList myTaskList = new MyTaskList();
        static MyTaskList linksTaskList = new MyTaskList();
        static void Main(string[] args)
        {
            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");
            var configuration = builder.Build();
            ConstCommon.SAVEFOLDER = configuration["ImageSavePath"];
            ConstCommon.ConnectionString = configuration.GetConnectionString("BookDatabase");

            var bookItemSync = configuration.GetValue<bool>("BookItemSync");

            myTaskList.Completed += MyTaskList_Completed;
            //StartTask();

            SKLBaiduSitePost.PCSubmit();

            Console.ReadLine();
        }

  

        private static void Test()
        {
            IBookGrah bookGrah = new BiQuYunBookGrah();
            bookGrah.Url = "https://www.biquyun.com/16_16092/";
            bookGrah.Grah();
        }


        private static void StartTask()
        {
            Console.WriteLine("一个任务开始！！！！！！！！！！！！！！！");
            if (myTaskList == null)
            {
                myTaskList = new MyTaskList();
            }
            myTaskList.Tasks.Clear();
            List<BookReptileTask> task = null;
            List<BookLinks> links = null;
            using (BookContext context = new BookContext())
            {
                var book = context.Book.Where(m => m.BookState.HasValue && m.BookState.Value == 1);
                task = context.BookReptileTask.Where(m => !book.Any(p => p.BookId == m.BookId)).OrderBy(m => m.Id).ToList();
                links = context.BookLinks.ToList();
            }
            foreach (var item in links)
            {
                LinkBase linkBase = null;
                switch (item.SyncType)
                {
                    case 2:
                        linkBase = new BQYLink(item);
                        break;
                    case 3:
                        linkBase = new MXZWLink(item);
                        break;
                }
                try
                {
                    linkBase.ReadLink();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
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
            StartTask();
        }



    }
}
