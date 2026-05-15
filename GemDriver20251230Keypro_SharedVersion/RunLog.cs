using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GemDriver
{
    public class RunLog
    {

        private static RunLog instance;
        public static RunLog Instance
        {
            get
            {
                if (instance == null)
                    instance = new RunLog();
                return instance;
            }
        }

        private object syncObj = new object();//日志线程互锁
        private ManualResetEvent enqueueEvent;//日志线程触发
        //日志记录线程状态
        public enum EventLogState
        {
            Running,
            Stopping,
            Stopped,
        }
        private EventLogState state;//日志状态
        private Queue logQueue;//日志队列

        public RunLog()
        {
            this.logQueue = Queue.Synchronized(new Queue());
            this.enqueueEvent = new ManualResetEvent(false);
            this.state = EventLogState.Stopped;
            this.syncObj = new object();
        }

        public string logDir = Application.StartupPath + "\\Log";//日志所处路径


        /// <summary>
        /// 将日志添加到队列中
        /// </summary>
        /// <param name="sContent"></param>
        public void SaveLog(string sContent)
        {
            if (this.state != EventLogState.Running)
                Start();
            sContent = string.Format("{0} : {1}", (object)("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "]"), (object)sContent);
            this.logQueue.Enqueue((object)sContent);//写入日志队列
            this.enqueueEvent.Set();//触发日志线程写入日志
        }


        /// <summary>
        /// 停止日志记录线程
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {

            this.state = EventLogState.Stopping;
            this.enqueueEvent.Set();

            Application.DoEvents();
            Thread.Sleep(50);

            return this.state == EventLogState.Stopped;
        }

        /// <summary>
        /// 开启新线程记录日志
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            if (this.state == EventLogState.Stopped)
            {
                Thread thread = new Thread(new ThreadStart(this.LogThread));//定义线程
                thread.Priority = ThreadPriority.BelowNormal;//设置线程级别
                lock (this.syncObj)
                {
                    if (this.state != EventLogState.Stopped)
                        return this.state == EventLogState.Running;
                    this.logQueue = Queue.Synchronized(new Queue());
                    this.enqueueEvent = new ManualResetEvent(false);//线程触发
                    this.state = EventLogState.Running;//设置启动状态
                }
                thread.Start();//线程开启
            }
            return this.state == EventLogState.Running;
        }

        /// <summary>
        /// 日志线程启动方法
        /// </summary>
        private void LogThread()
        {
            TimeSpan timeout = new TimeSpan(0, 0, 1);//1秒钟
            do
            {
                if (this.enqueueEvent.WaitOne(timeout, false))//等待线程被触发
                {
                    try
                    {
                        this.WriteToFile();//写日志
                    }
                    catch
                    {
                        lock (this.syncObj)
                        {
                            this.state = EventLogState.Stopped;
                            return;
                        }
                    }
                    finally
                    {
                        this.enqueueEvent.Reset();//复位线程等待
                    }
                }
            }
            while (this.state == EventLogState.Running);

        }

        //写队列中的日志
        public void WriteToFile()
        {
            try
            {
                string path = Global.Log_Path;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                using (StreamWriter streamWriter = new StreamWriter(path + "\\" + DateTime.Now.ToString("yyyy-MM-dd-HH") + ".log", true))
                {
                    while (this.logQueue.Count > 0)
                        streamWriter.WriteLine(this.logQueue.Dequeue());
                }
            }
            catch { }
        }



    }
}
