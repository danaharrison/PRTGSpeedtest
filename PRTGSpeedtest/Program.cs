using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
                    FileName = ConfigurationSettings.AppSettings["exePath"],
                    Arguments = "--accept-license",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            p.Start();
            while (!p.StandardOutput.EndOfStream)
            {
                stResult.Add(p.StandardOutput.ReadLine());
            }

            var prtgResultList = new List<PrtgResult>();
            prtgResultList.Add(new PrtgResult { channel = "Latency (ms)", value = stResult[5].Split(':')[1].Split(new string[] { "ms" }, StringSplitOptions.None)[0].Trim().Split('.')[0] });
            prtgResultList.Add(new PrtgResult { channel = "Download (Mbps)", value = stResult[7].Split(':')[1].Split(new string[] { "Mbps" }, StringSplitOptions.None)[0].Trim().Split('.')[0] });
            prtgResultList.Add(new PrtgResult { channel = "Upload (Mbps)", value = stResult[9].Split(':')[1].Split(new string[] { "Mbps" }, StringSplitOptions.None)[0].Trim().Split('.')[0] });
            var jsonResult = new PrtgRoot
            {
                prtg = new Prtg
                {
                    result = prtgResultList,
                    text = "Sample Text"
                }
            };

            Console.Write(JsonSerializer.Serialize(jsonResult));
        }        
    }

    public class PrtgResult
    {
        public string channel { get; set; }
        public string value { get; set; }
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
