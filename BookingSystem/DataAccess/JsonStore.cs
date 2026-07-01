using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;
using System.Web.Script.Serialization;

namespace BookingSystem.DataAccess
{
    public static class JsonStore
    {
        private static readonly object _lock = new object();

        public static List<T> ReadAll<T>(string fileName)
        {
            lock (_lock)
            {
                string path = HostingEnvironment.MapPath("~/App_Data/" + fileName);
                if (path == null || !File.Exists(path))
                {
                    return new List<T>();
                }

                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<T>();
                }

                return new JavaScriptSerializer().Deserialize<List<T>>(json);
            }
        }

        public static void WriteAll<T>(string fileName, List<T> items)
        {
            lock (_lock)
            {
                string path = HostingEnvironment.MapPath("~/App_Data/" + fileName);
                string json = new JavaScriptSerializer().Serialize(items);
                File.WriteAllText(path, json);
            }
        }
    }
}
