
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

namespace Novel.Reptile
{
    class Program
    {
        static MyTaskList myTaskList = new MyTaskList();
        static void Main(string[] args)
        {
            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);


            myTaskList.Completed += MyTaskList_Completed;
            //IBookGrah book = new DingDianBookGrah();
            //book.Url = "https://xiaoshuo.sogou.com/chapter/5254132907_172165911980583/";
            //book.Grah();
            //bookGrah.GrahAsync();
            StartTask(myTaskList);
            //while (DateTime.Now.Hour >= 0)
            //{
            //    NewStart();
            //}
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
                if (item.SyncType == 2)
                {
                    IBookGrah grah = new BiQuYunBookGrah();
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
    }
}
