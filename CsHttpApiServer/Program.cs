using System;
using System.Threading;

namespace CsHttpApi
{
    class Program
    {
        private const int port = 8888;
        private const string defaultController = "index";
        private const string defaultAction = "hello";

        static void Main(string[] args)
        {

            Framework.Start(port, defaultController, defaultAction);

            while (true)
            {
                Thread.Sleep(100);
            }
        }
    }
}
