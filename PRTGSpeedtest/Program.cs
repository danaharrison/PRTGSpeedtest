using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PRTGSpeedtest
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> stResult = new List<string>();

            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {                    
                    //FileName = ConfigurationSettings.AppSettings["exePath"],
                    FileName = "cmd.exe",
                    Arguments = "/C fast --upload --single-line",
                    UseShellExecute = false,                    
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
            };

            p.Start();           

            while (!p.StandardOutput.EndOfStream)
            {
                stResult.Add(p.StandardOutput.ReadLine());                
            }

            string[] fastOut = stResult[stResult.Count - 1].Split('/');
            string download = fastOut[0];
            string upload = fastOut[1];

            upload = upload.Substring(upload.IndexOf("Mbps") - 4, 4).Trim();
            download = download.Substring(download.IndexOf("Mbps") - 4, 4).Trim();

            var prtgResultList = new List<PrtgResult>();
            //prtgResultList.Add(new PrtgResult { channel = "Latency (ms)", value = stResult[5].Split(':')[1].Split(new string[] { "ms" }, StringSplitOptions.None)[0].Trim().Split('.')[0] });
            prtgResultList.Add(new PrtgResult { channel = "Download", value = download, unit = "custom", customunit = "Mbps" });
            prtgResultList.Add(new PrtgResult { channel = "Upload", value = upload, unit = "custom", customunit = "Mbps" });
            var jsonResult = new PrtgRoot
            {
                prtg = new Prtg
                {
                    result = prtgResultList,
                    text = ""
                }
            };

            string[] piHoles = ConfigurationSettings.AppSettings["PiHoles"].Split('|');
            int dnsQueries = 0, adsBlocked = 0;

            foreach( var pi in piHoles )
            {
                try
                {
                    using (var c = new HttpClient())
                    {
                        var r = c.GetAsync("http://" + pi + "/admin/api.php?summaryRaw&auth=" + ConfigurationSettings.AppSettings["PiHolesKey"]).Result.Content.ReadAsStringAsync().Result;
                        dynamic rJson = JObject.Parse(r);

                        dnsQueries += rJson.dns_queries_today.Value;
                        adsBlocked += rJson.ads_blocked_today.Value;
                    }
                }
                catch
                {

                }
            }

            prtgResultList.Add(new PrtgResult { channel = "PiHole Queries", value = dnsQueries.ToString() });
            prtgResultList.Add(new PrtgResult { channel = "Ads Blocked", value = adsBlocked.ToString() });

            Console.Write(JsonSerializer.Serialize(jsonResult));
            if(File.Exists(ConfigurationSettings.AppSettings["outPath"]))
            {
                File.Delete(ConfigurationSettings.AppSettings["outPath"]);
            }
            File.WriteAllText(ConfigurationSettings.AppSettings["outPath"], JsonSerializer.Serialize(jsonResult));
        }        
    }

    public class PrtgResult
    {
        public string channel { get; set; }
        public string value { get; set; }
        public string unit { get; set; }
        public string customunit { get; set; }
    }

    public class Prtg
    {
        public List<PrtgResult> result { get; set; }
        public string text { get; set; }
    }

    public class PrtgRoot
    {
        public Prtg prtg { get; set; }
    }
}
