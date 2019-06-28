import React from "react"
import Page from "../layouts/page"
import Router from "next/router"
import Menu from "../components/menu"
import AgentTable from "../components/agent-table"
import apiClient from "../services/api-client"
import CodeEditor from "../components/codeEditor"

export default class Index extends React.Component {
  constructor(props) {
    super(props)
    this.state = {
      agents: {}
    }
  }

  componentDidMount() {
    const client = apiClient()
    client
      .getAgents()
      .unwrap({})
      .then(i => {
        this.setState({
          agents: i
        })
      })
      .catch(e => console.log(e))
  }

  onChangeEditor(newValue) {
    this.setState({
      code: newValue
    })
  }

  onChangeName(e) {
    this.setState({
      name: e.target.value
    })
  }

  onChangeVersion(e) {
    this.setState({
      version: e.target.value
    })
  }

  onAgentSelectionChanged(i) {
    this.setState({
      selectedAgents: i
    })
  }

  onClick(e) {
    e.preventDefault()
    const { code: yaml, name, version, selectedAgents: agents } = this.state
    const payload = {
      Name: name,
      Version: version,
      Yaml: yaml,
      Agents: agents
    }
    console.log(payload)
    const client = apiClient()
    client.postDeployment(payload).then(i => {
      console.log(i)
      Router.replace("/")
    })
  }

  render() {
    return (
      <Page>
        <section className="columns is-fullheight">
          <Menu className="is-2 column is-narrow-mobile is-fullheight section is-hidden-mobile" />

          <div className="container column is-fluid">
            <h1 className="title">DeployMe! Management Portal</h1>
            <hr />
            <AgentTable
              agents={this.state.agents}
              onSelectionChanged={i => this.onAgentSelectionChanged(i)}
            />
            <hr />

            <div className="column is-4">
              <div className="field">
                <label className="label">Name</label>
                <div className="control">
                  <input
                    className="input"
                    type="text"
                    placeholder="my-nginx"
                    onInput={e => this.onChangeName(e)}
                  />
                </div>
              </div>

              <div className="field">
                <label className="label">Version</label>
                <div className="control">
                  <input
                    className="input"
                    type="text"
                    placeholder="0.1"
                    onInput={e => this.onChangeVersion(e)}
                  />
                </div>
              </div>
            </div>
            <br />
            <div>
              <label className="label">DeployMe YAML</label>
              <div>
                <CodeEditor onChange={i => this.onChangeEditor(i)} />
              </div>
            </div>
            <br />
            <div>
              <a className="button is-primary" onClick={e => this.onClick(e)}>
                DeployMe!
              </a>
            </div>
          </div>
        </section>
      </Page>
    )
  }
}
