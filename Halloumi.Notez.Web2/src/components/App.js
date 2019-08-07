import React, { Component } from "react";
import Header from "./Header";
import MIDI from "midicube";

class App extends Component {
  constructor(props) {
    super(props);

    this.state = {

    };

    this.init();
  }

  

  async init() {
    this.player = MIDI.player();
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
