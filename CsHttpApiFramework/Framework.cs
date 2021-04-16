using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CsHttpApi
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class InitializeAttribute : Attribute{}

    internal class ControllerInfo
    {
        public string Controller;
        public Dictionary<string, string> Actions;
    }

    public class Framework
    {
        private static readonly HttpListener Listener = new HttpListener();

        private static Dictionary<string, ControllerInfo> typeMapper = new Dictionary<string, ControllerInfo>();

        public static string DefaultController;
        public static string DefaultAction;
        public static int Port;

        private static Assembly appAssembly;

        public static void Start(int port, string defaultController = "Index", string defaultActoin = "Index")
        {
            Port = port;
            DefaultController = defaultController;
            DefaultAction = defaultActoin;

            Listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            Listener.Prefixes.Add("http://+:" + Port + "/");
            Listener.Start();
            Listener.BeginGetContext(new AsyncCallback(GetContextCallBack), Listener);

            appAssembly = Assembly.GetCallingAssembly();

            Type[] types = appAssembly.GetTypes();

            foreach (Type t in types)
            {
                if (t.BaseType != null && t.BaseType.FullName == "CsHttpApi.Controller.Base")
                {
                    ControllerInfo info = new ControllerInfo();
                    info.Controller = t.FullName;
                    info.Actions = new Dictionary<string, string>();
                    MethodInfo[] methodInfo = t.GetMethods();

                    foreach (MethodInfo m in methodInfo)
                    {
                        if(m.IsStatic)
                        {
                            if(m.GetCustomAttribute<InitializeAttribute>() != null)
                            {
                                m.Invoke(null, BindingFlags.Static, null, null, null);
                            }
                        }
                        else if(m.IsPublic && !m.IsConstructor)
                        {
                            info.Actions[m.Name.ToLower()] = m.Name;
                        }
                    }

                    typeMapper[t.FullName.ToLower()] = info;
                }
            }
        }

        private static void GetContextCallBack(IAsyncResult ar)
        {
            try
            {
                var listener = ar.AsyncState as HttpListener;

                HttpListenerContext context = listener.EndGetContext(ar);

                Task.Factory.StartNew(() => ProcessRequest(context));

                listener.BeginGetContext(new AsyncCallback(GetContextCallBack), listener);

            }
            catch { }
        }

        private static void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                context.Response.StatusCode = 200;
                string[] routes = context.Request.RawUrl.Split('?')[0].Split('/');
                string controller = null;
                string action = null;
                if (routes.Length <= 1)
                {
                    controller = DefaultController;
                    action = DefaultAction;
                }
                else if (routes.Length == 2)
                {
                    controller = string.IsNullOrEmpty(routes[1]) ? DefaultController : routes[1];
                    action = DefaultAction;
                }
                else
                {
                    controller = string.IsNullOrEmpty(routes[1]) ? DefaultController : routes[1];
                    action = string.IsNullOrEmpty(routes[2]) ? DefaultAction : routes[2];
                }

                string typeName = ("CsHttpApi.Controller." + controller).ToLower();

                if (!typeMapper.ContainsKey(typeName))
                {
                    Response(context, "404 not found");
                    return;
                }

                ControllerInfo controllerInfo = typeMapper[typeName];

                Type type = appAssembly.GetType(controllerInfo.Controller);

                if (type == null)
                {
                    Response(context, "404 not found");
                    return;
                }

                action = action.ToLower();

                if (!controllerInfo.Actions.ContainsKey(action))
                {
                    Response(context, "404 not found");
                    return;
                }

                MethodInfo method = type.GetMethod(controllerInfo.Actions[action]);

                if (method == null)
                {
                    Response(context, "404 not found");
                    return;
                }

                object ins = Activator.CreateInstance(type, new object[] { context });

                method.Invoke(ins, new object[] { });
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public static void Response(HttpListenerContext context, string response)
        {
            var buffer = Encoding.UTF8.GetBytes(response);
            try
            {
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();
                context.Response.Close();
            }
            catch
            {
                context.Response.OutputStream.Close();
                context.Response.Close();
            }
        }
    }
}
