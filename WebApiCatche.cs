 //[CacheWebApi(Duration = 60)]
 
using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
 
namespace WebUI.Filters
{
    public class CacheWebApiAttribute : ActionFilterAttribute
    {
        public int Duration { get; set; }
        public override void OnActionExecuting(HttpActionContext context)
        {
            if (context != null)
            {
                string key = $"{context.ControllerContext.Controller.ToString()}_{context.ActionDescriptor.ActionName}";
                if (MemoryCacher.Contains(key))
                {
                    var val = (string)MemoryCacher.GetValue(key);
                    if (val != null)
                    {
                        try
                        {
                            context.Response = context.Request.CreateResponse();
                            context.Response.Content = new StringContent(
                                val,
                                MemoryCacher.GetValue($"{key}_Encoding") as System.Text.Encoding ?? System.Text.Encoding.UTF8,
                                MemoryCacher.GetValue($"{key}_MediaType").ToString() ?? context.Request.Headers.Accept.FirstOrDefault()?.ToString());
                        }
                        catch (Exception ex)
                        {
                            Logger.AppLogger.GetLogger().Error("CacheWebApi OnActionExecuting", ex);
                        }
                        return;
                    }
                }
            }
        }
        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            string key = $"{context.ActionContext.ControllerContext.Controller.ToString()}_{context.ActionContext.ActionDescriptor.ActionName}";
            if (context.Response != null && context.Response.Content != null)
            {
                string body = context.Response.Content.ReadAsStringAsync().Result;
                if (!MemoryCacher.Contains(key))
                {
                    MemoryCacher.Add(key, body, DateTime.Now.AddSeconds(Duration));
                    MemoryCacher.Add($"{key}_MediaType", context.Response.Content.Headers.ContentType.MediaType, DateTime.Now.AddSeconds(Duration));
                    MemoryCacher.Add($"{key}_Encoding", context.Response.Content.Headers.ContentType.CharSet, DateTime.Now.AddSeconds(Duration));
                }
            }
        }
    }

    public static class MemoryCacher
    {
        public static object GetValue(string key)
        {
            MemoryCache memoryCache = MemoryCache.Default;
            return memoryCache.Get(key);
        }
        public static bool Add(string key, object value, DateTimeOffset absExpiration)
        {
            MemoryCache memoryCache = MemoryCache.Default;
            return memoryCache.Add(key, value, absExpiration);
        }
        public static void Delete(string key)
        {
            MemoryCache memoryCache = MemoryCache.Default;
            if (memoryCache.Contains(key))
                memoryCache.Remove(key);
        }
        public static bool Contains(string key)
        {
            MemoryCache memoryCache = MemoryCache.Default;
            return memoryCache.Contains(key);
        }
    }
}
