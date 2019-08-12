import React, { Component } from "react";
import Header from "./Header";
import MidiPlayer from "midi-player-js";
import Soundfont from "soundfont-player";
import { Button } from "react-bootstrap";
import MidiParser from "midi-parser-js";

class App extends Component {
  constructor(props) {
    super(props);

    this.state = {};
    this.instruments = {};
    this.tracks = [];
    this.player = null;
  }

  handleAddClick() {
    this.init();
  }

  async loadMidiFromUri(midiUri) {
    return new Promise((resolve, reject) => {
      var request = new XMLHttpRequest();
      request.open("GET", midiUri, true);
      request.responseType = "blob";
      request.onload = function() {
        var reader = new FileReader();
        reader.readAsDataURL(request.response);
        reader.onload = function(e) {
          resolve(e.target.result);
        };
        reader.onerror = function(error) {
          reject(error);
        };
      };
      request.send();
    });
  }

  async init() {
    if (!this.audioContext) this.audioContext = new AudioContext();

    if (!this.instruments[1])
      await Soundfont.instrument(this.audioContext, "overdriven_guitar").then(
        instrument => {
          this.instruments[1] = instrument;
          console.log(instrument);
        }
      );

    if (!this.instruments[2])
      await Soundfont.instrument(this.audioContext, "distortion_guitar").then(
        instrument => {
          this.instruments[2] = instrument;
          console.log(instrument);
        }
      );

    if (!this.instruments[3])
      await Soundfont.instrument(
        this.audioContext,
        "electric_bass_finger"
      ).then(instrument => {
        this.instruments[3] = instrument;
        console.log(instrument);
      });

    // // Initialize player and register event handler
    if (!this.player)
      this.player = new MidiPlayer.Player(event => {
        console.log(event);
        if (!this.instruments[event.track]) return;

        if (event.name === "Note on") {
          this.instruments[event.track].play(
            event.noteName,
            this.audioContext.currentTime,
            {
              gain: event.velocity / 100
            }
          );
        }

        if (event.name === "Note off") {
          this.instruments[event.track].stop();
        }
      });

    this.loadMidiFromUri("http://localhost:9000/api/randomriff").then(midi => {
      var midiData = MidiParser.parse(midi);
      midiData.track.forEach((track, trackIndex) => {
        var trackData = {
          index: trackIndex,
          isDrums: track.event.find(event => {
            return event.channel === 9;
          })
            ? true
            : false
        };
        var programChange = track.event.find(event => {
          return event.type === 12;
        });

        if (trackData.isDrums || programChange) {
          trackData.instrumentNumber = programChange ? programChange.data : 0;
          console.log(trackData);
          this.tracks.push(trackData);
        }
      });

      this.player.stop();
      this.player.loadDataUri(midi);
      this.player.play();
    });
  }

  render() {
    return (
      <>
        <Header />
        <Button onClick={this.handleAddClick.bind(this)}>Play</Button>
      </>
    );
  }
}

export default App;
