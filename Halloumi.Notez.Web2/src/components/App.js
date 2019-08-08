import React, { Component } from "react";
import Header from "./Header";
import MidiPlayer from "midi-player-js";
import Soundfont from "soundfont-player";
import MidiParser from "midi-parser-js";

class App extends Component {
  constructor(props) {
    super(props);

    this.state = {};

    this.audioContext = new AudioContext();

    Soundfont.instrument(this.audioContext, "clavinet").then(instrument => {
      this.instrument = instrument;
    });

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
    // // Initialize player and register event handler
    var player = new MidiPlayer.Player(event => {
      console.log(event);
      if (event.name === "Note on" && event.velocity > 0) {
        this.instrument.play(event.noteName, this.audioContext.currentTime, {
          gain: event.velocity / 100
        });
      }
    });

    this.loadMidiFromUri("http://localhost:9000/api/randomriff").then(midi => {
      player.loadDataUri(midi);
      player.play();

      //   var midiData = MidiParser.parse(midi);
      //   var tracks = [];
      //   midiData.track.forEach((track, trackIndex) => {
      //     var trackData = {
      //       index: trackIndex,
      //       isDrums: track.event.find(event => {
      //         return event.channel === 9;
      //       })
      //         ? true
      //         : false
      //     };
      //     var programChange = track.event.find(event => {
      //       return event.type === 12;
      //     });

      //     if (trackData.isDrums || programChange) {
      //       trackData.instrument = programChange ? programChange.data : 0;
      //       console.log(trackData);
      //       tracks.push(trackData);
      //     }
      //   });
    });
  }

  render() {
    return (
      <>
        <Header />
      </>
    );
  }
}

export default App;
