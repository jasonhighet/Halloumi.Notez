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
                    var stream = new FileStream(midFile, FileMode.Open);
                    var content = new StreamContent(stream);
                    content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = "midi",
                    };

                    form.Add(content, "midi");
                }

                var task = client.PostAsync(url, form);
                var result = task.Result.Content.ReadAsStringAsync().Result;
            }




            return new List<string>();
        }
    }
}
