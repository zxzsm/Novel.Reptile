
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
            myTaskList.AllTaskCompleted += MyTaskList_AllTaskCompleted;

            KSSZBookGrah bookGrah = new KSSZBookGrah();
            bookGrah.Url = "https://www.kanshushenzhan.com/4334/";
            bookGrah.GrahAsync();
            //StartTask(myTaskList);
            //while (DateTime.Now.Hour >= 0)
            //{
            //    NewStart();
            //}
            Console.ReadLine();
        }

        private static void MyTaskList_AllTaskCompleted()
        {
            Console.WriteLine("新任务开始，重新启动");
            StartTask(myTaskList);
        }

        private static void StartTask(MyTaskList myTaskList)
        {
            myTaskList.Tasks.Clear();
            List<BookReptileTask> task = null;
            using (BookContext context = new BookContext())
            {
                task = context.BookReptileTask.ToList();
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

        private static void NewStart()
        {
            List<BookReptileTask> task = null;
            using (BookContext context = new BookContext())
            {
                var book = context.Book.Where(m => m.BookState.HasValue && m.BookState.Value == 1);
                task = context.BookReptileTask.Where(m => !book.Any(p => p.BookId == m.BookId)).OrderByDescending(m => m.Id).ToList();
            }
            foreach (var item in task)
            {
                if (item.SyncType == 2)
                {
                    IBookGrah grah = new BiQuYunBookGrah();
                    grah.Url = item.Url;
                    try
                    {
                        grah.Grah();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
        }

        private static void MyTaskList_Completed()
        {
            Console.WriteLine("一个任务结束！！！！！！！！！！！！！！！");
        }
    }
}
