using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Halloumi.Notez.Engine.Magenta
{
    public static class MagentaHelper
    {
        public static List<string> Interpolate(List<string> midiFiles)
        {
            var url = "http://localhost:3000";

            using (var client = new HttpClient())
            {
                var form = new MultipartFormDataContent();

                foreach (var midFile in midiFiles)
                {
                    var fileContent = new ByteArrayContent(File.ReadAllBytes(midFile));
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/midi");

                    form.Add(fileContent, "\"midi\"", "\"" + midFile + "\"");
                }

                var task = client.PostAsync(url, form);
                var result = task.Result.Content.ReadAsStringAsync().Result;
            }




            return new List<string>();
        }
    }
}
