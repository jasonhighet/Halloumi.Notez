const express = require("express");
const bodyParser = require("body-parser");
const cors = require("cors");
const multer = require("multer");

const music_vae = require("@magenta/music/node/music_vae");
const music_rnn = require("@magenta/music/node/music_rnn");
const core = require("@magenta/music/node/core");

const VAE_CHECKPOINTS =
  "https://storage.googleapis.com/magentadata/js/checkpoints/music_vae/mel_4bar_small_q2";
const RNN_CHECKPOINTS =
  "https://storage.googleapis.com/magentadata/js/checkpoints/music_rnn/melody_rnn";

// // These hacks below are needed because the library uses performance and fetch which
// // exist in browsers but not in node. We are working on simplifying this!
var globalAny = global;
globalAny.performance = Date;
globalAny.fetch = require("node-fetch");

const musicVAE = new music_vae.MusicVAE(VAE_CHECKPOINTS);
musicVAE.initialize();

const musicRNN = new music_rnn.MusicRNN(RNN_CHECKPOINTS);
//musicRNN.initialize();

const app = express();
const port = 3000;

app.use(cors());
app.use(bodyParser.urlencoded({ extended: false }));
app.use(bodyParser.json());

var upload = multer();

app.post("", upload.array("midi"), (req, res) => {
  console.log(req);

  var file = req.files[0];

  var midi1 = req.files[0].buffer.toString("binary");
  var sequence1 = core.midiToSequenceProto(midi1);
  sequence1 = core.sequences.quantizeNoteSequence(sequence1, 8);

  var midi2 = req.files[1].buffer.toString("binary");
  var sequence2 = core.midiToSequenceProto(midi2);
  sequence2 = core.sequences.quantizeNoteSequence(sequence2, 8);

  var numInterpolations = 3;
  musicVAE
    .interpolate([sequence1, sequence2], numInterpolations)
    .then(interpolations => {
      var newSequence = interpolations[1];

      var newMidi = core.sequenceProtoToMidi(newSequence);

      var buffer = Buffer.from(newMidi, "binary");

      res.writeHead(200, {
        "Content-Type": file.mimetype,
        "Content-disposition": "attachment;filename=" + file.originalname,
        "Content-Length": buffer.length
      });
      res.end(buffer);
    });
});

app.listen(port, () => console.log(`Listening on port ${port}`));
