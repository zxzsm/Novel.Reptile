using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Novel.Reptile
{
    public class MyTaskList
    {
        public List<Action> Tasks = new List<Action>();
        private int taskCount = 0;

        public void Start()
        {
            for (var i = 0; i < 15; i++)
                StartAsync();
            taskCount = Tasks.Count;
        }

        private object _sync = new object();

        public event Action Completed;
        public event Action AllTaskCompleted;

        public void StartAsync()
        {
            lock (Tasks)
            {
                if (Tasks.Count > 0)
                {
                    var t = Tasks[Tasks.Count - 1];
                    Tasks.Remove(t);
                    ThreadPool.QueueUserWorkItem(h =>
                    {

                        try
                        {
                            t();
                            
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                        StartAsync();
                        
                    });
                }
                else Completed?.Invoke();
            }
        }
    }
}
