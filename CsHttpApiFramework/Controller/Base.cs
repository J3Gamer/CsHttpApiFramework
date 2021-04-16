using System;
using System.Net;
using System.Text;

namespace CsHttpApi.Controller
{
    public abstract class Base
    {
        public HttpListenerContext Context;

        public Base(HttpListenerContext context)
        {
            Context = context;
        }

        protected string Success(string msg, string url = "")
        {
            string json = "{\"status\":200,\"msg\":" + msg + ",\"url\":\"" + url  + "\"}";
            return json;
        }

        protected string Error(string msg, string url = "")
        {
            string json = "{\"status\":202,\"msg\":" + msg + ",\"url\":\"" + url + "\"}";
            return json;
        }

        protected void Response(string response, bool json = true)
        {
            var buffer = Encoding.UTF8.GetBytes(response);
            try
            {
                Context.Response.ContentLength64 = buffer.Length;
                if(json)
                {
                    Context.Response.ContentType = "application/json";
                }
                Context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                Context.Response.OutputStream.Close();
                Context.Response.Close();
            }
            catch(Exception ex)
            {
                Log.Error(ex);
                Context.Response.OutputStream.Close();
                Context.Response.Close();
            }
        }

        protected void Redirect(string url)
        {
            try
            {
                Context.Response.StatusCode = 301;
                Context.Response.AddHeader("Location", url);
                Context.Response.OutputStream.Close();
                Context.Response.Close();
            }
            catch
            {
                Context.Response.OutputStream.Close();
                Context.Response.Close();
            }
        }
    }
}
