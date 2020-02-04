﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ShareInvest.Message;

namespace ShareInvest.DelayRequest
{
    public class Delay
    {
        public static Delay GetInstance(int mSecond)
        {
            delay = mSecond;

            if (request == null)
                request = new Delay();

            return request;
        }
        public void Run()
        {
            taskWorker.Start();
        }
        public void RequestTrData(Task task)
        {
            requestTaskQueue.Enqueue(task);
        }
        public int QueueCount
        {
            get
            {
                return requestTaskQueue.Count;
            }
        }
        private Delay()
        {
            taskWorker = new Thread(delegate ()
            {
                while (true)
                {
                    try
                    {
                        while (requestTaskQueue.Count > 0)
                        {
                            requestTaskQueue.Dequeue().RunSynchronously();
                            Thread.Sleep(delay);
                        }
                        Thread.Sleep(100);
                    }
                    catch (Exception ex)
                    {
                        new ExceptionMessage(ex.StackTrace);
                        Process.Start("shutdown.exe", "-r");
                        Environment.Exit(0);
                    }
                }
            });
        }
        public static int delay = 210;
        private static Delay request;
        private readonly Thread taskWorker;
        private readonly Queue<Task> requestTaskQueue = new Queue<Task>();
    }
}