const express = require("express");
const bodyParser = require("body-parser");
const cors = require("cors");
const multer = require("multer");

const mode = require("@magenta/music/node/music_vae");
const core = require("@magenta/music/node/core");
const VAE_CHECKPOINTS =
  "https://storage.googleapis.com/magentadata/js/checkpoints/music_vae/mel_4bar_small_q2";

// // These hacks below are needed because the library uses performance and fetch which
// // exist in browsers but not in node. We are working on simplifying this!
var globalAny = global;
globalAny.performance = Date;
globalAny.fetch = require("node-fetch");

const model = new mode.MusicVAE(VAE_CHECKPOINTS);
model.initialize();

const app = express();
const port = 3000;

app.use(cors());
app.use(bodyParser.urlencoded({ extended: false }));
app.use(bodyParser.json());

var upload = multer();

app.post("", upload.array("midi"), (req, res) => {
  console.log(req.files[0]);
  var file = req.files[0];

  res.writeHead(200, {
    "Content-Type": file.mimetype,
    "Content-disposition": "attachment;filename=" + file.originalname,
    "Content-Length": file.size
  });
  res.end(file.buffer);
});

app.listen(port, () => console.log(`Listening on port ${port}`));
