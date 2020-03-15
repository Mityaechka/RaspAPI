using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RaspAPI.Services
{
    public class CacheData
    {
         public string Cache { get; set; }
         public double MaxTime { get;  set; } = 10000;
         public double StartTime { get; set; }

        public CacheData()
        {
        }

        CacheData(int maxTime,string cache)
        {
            StartTime = DateTime.Now.Subtract(DateTime.MinValue).TotalSeconds;
            MaxTime = maxTime;
            Cache = cache;
        }
        public static void SaveCache(string key, string cache,int maxTime)
        {
            string path = Path.Combine( "Cache", key + ".json");
            var data = new CacheData( maxTime, cache);
            
            File.WriteAllText(path, JsonConvert.SerializeObject(data));
        }
        public static (bool,string) GetCache(string key)
        {
            string path = Path.Combine( "Cache", key + ".json");
            if (File.Exists(path))
            {
                var data = JsonConvert.DeserializeObject<CacheData>(File.ReadAllText(path));
                var m = (data.StartTime + data.MaxTime);
                var c = DateTime.Now.Subtract(DateTime.MinValue).TotalSeconds;
                bool IsAvaible =  m>c ;
                if (IsAvaible)
                {

                    return (IsAvaible, data.Cache);
                }
                else
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    return (IsAvaible, "");
                }
            }
            else
            {
                return (false, "");
            }

        }
    }
}
