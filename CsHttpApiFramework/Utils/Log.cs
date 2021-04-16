using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CsHttpApi
{
    public enum LogType
    {
        Info,
        Warning,
        Error
    }

    public class Log
    {
        private static object locker = new object();

        public static void Write(string msg, LogType type = LogType.Info)
        {
            if (type == LogType.Info)
            {
                Info(msg);
            }
            else if (type == LogType.Warning)
            {
                Warning(msg);
            }
            else if (type == LogType.Error)
            {
                Error(msg);
            }
        }

        private static void Save2File(string info, string flag)
        {
            lock (locker)
            {
                try
                {
                    List<string> contents = new List<string>();
                    contents.Add("-------------------------------------------");
                    contents.Add("[" + System.DateTime.Now.ToLongDateString() + " " + System.DateTime.Now.ToLongTimeString() + "]");
                    contents.Add("["+ flag + "] " + info);
                    string fileName = Environment.CurrentDirectory + "/Log/" + System.DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                    if (!Directory.Exists(Environment.CurrentDirectory + "/Log/"))
                    {
                        Directory.CreateDirectory(Environment.CurrentDirectory + "/Log/");
                    }
                    File.AppendAllLines(fileName, contents);
                }
                catch
                {

                }
            }
        }

        public static void Info(string info)
        {
            Save2File(info, "info");
        }

        public static void Info(Exception e)
        {
            Info(e.Message + "\n" + e.StackTrace);
        }

        public static void Info(object o)
        {
            Info(o.ToString());
        }

        public static void Warning(string info)
        {
            Save2File(info, "warning");
        }

        public static void Warning(Exception e)
        {
            Warning(e.Message + "\n" + e.StackTrace);
        }

        public static void Warning(object o)
        {
            Warning(o.ToString());
        }

        public static void Error(string info)
        {
            Save2File(info, "error");
        }

        public static void Error(Exception e)
        {
            Error(e.Message + "\n" + e.StackTrace);
        }

        public static void Error(object o)
        {
            Error(o.ToString());
        }

        public static void Stack()
        {
            StackFrame[] stacks = new System.Diagnostics.StackTrace(true).GetFrames();
            string result = "Stack Trace:\n";
            for (int i = 1; i < stacks.Length; i++)
            {
                StackFrame stack = stacks[i];
                result += string.Format("File:{0} Line:{1} Column:{2} Method:{3}\r\n", stack.GetFileName(),
                    stack.GetFileLineNumber(),
                    stack.GetFileColumnNumber(),
                    stack.GetMethod().ToString());
            }
            Info(result);
        }
    }
}
