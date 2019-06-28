import ReactAce from "react-ace-editor"
import React, { Component } from "react"

class CodeEditor extends Component {
  constructor(props) {
    super(props)
    this.onChange = this.onChange.bind(this)
  }

  onChange(newValue) {
    if (this.props.onChange) {
      this.props.onChange(newValue)
    }
  }

  get startingText() {
    return `InstallCommands:
  - apt-get install -y nginx
StartCommands:
  - echo "starting nginx!" >> /var/log/some.log
  - service nginx restart
StopCommands:
  - echo "stopping nginx!" >> /var/log/some.log
  - service nginx stop
UninstallCommands:
  - apt-get remove -y nginx`
  }

  render() {
    return (
      <ReactAce
        mode="yaml"
        theme="monokai"
        setReadOnly={false}
        onChange={this.onChange}
        setValue={this.startingText}
        style={{ height: "300px", width: "40vw" }}
        ref={instance => {
          this.ace = instance
        }}
      />
    )
  }
}
export default CodeEditor
