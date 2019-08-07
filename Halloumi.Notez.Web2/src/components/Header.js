import React, { Component } from "react";
import { Navbar } from "react-bootstrap";


class Header extends Component {
  render() {
    return (
      <Navbar  bg="dark" variant="dark" expand="sm">
        <Navbar.Brand href="#home">Notez</Navbar.Brand>
        <Navbar.Toggle aria-controls="basic-navbar-nav" />
        <Navbar.Collapse id="basic-navbar-nav">
          {this.props.children}
        </Navbar.Collapse>
      </Navbar>
    );
  }
}

export default Header;
