import React, { Component } from "react";
import Header from "./Header";
import MidiPlayer from "midi-player-js";
import Soundfont from "soundfont-player";

class App extends Component {
  constructor(props) {
    super(props);

    this.state = {

    };

    this.audioContext = new AudioContext();

    Soundfont.instrument(this.audioContext, 'clavinet').then(instrument=>{
        this.instrument = instrument;
    });

    this.init();
  }


  async loadMidiFromUri(midiUri) {
    return new Promise((resolve, reject) => {
      var request = new XMLHttpRequest();
      request.open('GET', midiUri, true);
      request.responseType = 'blob';
      request.onload = function() {
          var reader = new FileReader();
          reader.readAsDataURL(request.response);
          reader.onload = function(e){
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
         //console.log(event);
         if (event.name === 'Note on' && event.velocity > 0) {
          this.instrument.play(event.noteName, this.audioContext.currentTime, {gain:event.velocity/100});
        }
    });

    this.loadMidiFromUri("https://bitmidi.com/uploads/105027.mid").then(midi => {
      player.loadDataUri(midi);
      player.play();
    });
  }


  render() {
    return (
      <>
        <Header >
          
        </Header>
      </>
    );
  }
}

export default App;
