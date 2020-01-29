using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Halloumi.Notez.Engine.Midi;

namespace Halloumi.Notez.Engine.Magenta
{
    public static class MagentaHelper
    {
        private static Process _nodeProcess;

        public static void Start()
        {
            if(_nodeProcess != null)
                return;

            _nodeProcess = new Process
            {
                StartInfo =
                {
                    //UseShellExecute = false,
                    FileName = @"C:\Program Files\nodejs\node.exe",
                    WorkingDirectory = Path.GetFullPath(@"..\..\..\Halloumi.Notez.NodeApi"),
                    Arguments = "api.js",
                    //CreateNoWindow = true
                }
            };
            _nodeProcess.Start();
            
        }

        public static void Stop()
        {
            if (_nodeProcess == null)
                return;

            _nodeProcess.Kill();
            _nodeProcess.Dispose();
        }



        public static List<string> Interpolate(List<string> midiFiles)
        {
            if (midiFiles.Count != 2)
                return new List<string>();


            var midi = MidiHelper.ReadMidi(midiFiles[0]);
            var bpm = midi.Bpm;
            var instrument = midi.Phrases[0].Instrument;



            var results = new List<string>();

            var url = "http://localhost:3000/interpolate";

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
                var result = task.Result.Content.ReadAsByteArrayAsync().Result;

                if(!task.Result.IsSuccessStatusCode)
                    throw new Exception("Magenta failed");

                var filename = DateTime.Now.ToString("yyyymmddhhssfff") + ".mid";

                File.WriteAllBytes(filename, result);

                var newMidi = MidiHelper.ReadMidi(filename);
                newMidi.Phrases[0].Bpm = bpm;
                newMidi.Phrases[0].Instrument = instrument;
                MidiHelper.SaveToMidi(newMidi, filename);

                results.Add(filename);
            }

            return results;
        }
    }
}
