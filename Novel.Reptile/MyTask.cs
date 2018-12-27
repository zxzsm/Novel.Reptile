using Novel.Reptile.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Novel.Reptile
{
    public class MyTaskList
    {
        public List<Action> Tasks = new List<Action>();
        private int taskCount = 0;

        public void Start()
        {
            taskCount = Tasks.Count;
            for (var i = 0; i < 5; i++)
                StartAsync();
        }

        private object _sync = new object();

        public event Action Completed;

        public void StartAsync()
        {
            lock (Tasks)
            {
                if (Tasks.Count > 0)
                {
                    var t = Tasks[Tasks.Count - 1];
                    Tasks.Remove(t);


                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            t();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                        lock (_sync)
                        {
                            taskCount = taskCount - 1;
                            if (taskCount == 0)
                            {
                                Completed?.Invoke();
                            }
                        }
                        StartAsync();
                    });
                }
            }
        }
    }
}
