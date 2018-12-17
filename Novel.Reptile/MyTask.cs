using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Novel.Reptile
{
    public class MyTaskList
    {
        public List<Action> Tasks = new List<Action>();

        public void Start()
        {
            for (var i = 0; i < 20; i++)
                StartAsync();
        }

        public event Action Completed;

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
