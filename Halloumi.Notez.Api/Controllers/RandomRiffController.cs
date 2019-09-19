using Halloumi.Notez.Engine.Generator;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace Halloumi.Notez.Api.Controllers
{
    public class RandomRiffController : ApiController
    {
        private const string Folder = @"..\..\..\Halloumi.Notez.Engine\TestMidi\";
        private static SectionGenerator _sectionGenerator;


        [HttpGet]
        public HttpResponseMessage Generate()
        {
            if (_sectionGenerator == null)
                LoadSectionGenerator();

            const string midiPath = @"RandomRiff.mid";


            _sectionGenerator?.GenerateRiffs(midiPath);

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

        private void LoadSectionGenerator()
        {
            if(_sectionGenerator == null)
                _sectionGenerator = new SectionGenerator(Folder);
                _sectionGenerator.LoadLibrary("Doom", false);

        }
    }
}
