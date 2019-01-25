using Halloumi.Notez.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Halloumi.Notez.Api.Controllers
{
    public class RandomRiffController : ApiController
    {

        [HttpGet]
        public HttpResponseMessage Generate()
        {
            var midiPath = @"RandomRiff.mid";

            var generator = new PhraseGenerator();
            var phrase = generator.GeneratePhrase();
            MidiHelper.SaveToMidi(phrase, midiPath);
 
            var stream = new FileStream(midiPath, FileMode.Open, FileAccess.Read);
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(stream)
            };

            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = midiPath
            };


            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            return result;
        }
    }
}
