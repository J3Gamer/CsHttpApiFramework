using System;
using System.Net;

namespace CsHttpApi.Controller
{
    public class Index : Base
    {
        public Index(HttpListenerContext context) : base(context)
        {

        }

        [Initialize]
        public static void Init()
        {
            Console.WriteLine("Initialize Index!");
            Log.Info("Initialize Index!");
        }

        public void Hello()
        {
            Response(Success("Hello!"));
        }

        public void RedirectTest()
        {
            Redirect("https://www.google.com");
        }
    }

}
