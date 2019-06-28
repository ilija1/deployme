import React, { Component } from "react"

class AgentTable extends Component {
  constructor(props) {
    super(props)
    this.state = {
      allChecked: false
    }
  }

  onChange() {
    const checkboxes = document.querySelectorAll(
      "[data-type='agent-selection']"
    )
    let selections = []
    checkboxes.forEach(i => selections.push(i))
    selections = selections.filter(i => i.checked).map(i => i.dataset.key)

    if (this.props.onSelectionChanged) {
      this.props.onSelectionChanged(selections)
    }
  }

  selectAllNone() {
    const checkboxes = document.querySelectorAll(
      "[data-type='agent-selection']"
    )
    if (this.state.allChecked) {
      checkboxes.forEach(i => (i.checked = false))
      this.setState({
        allChecked: false
      })
    } else {
      checkboxes.forEach(i => (i.checked = true))
      this.setState({
        allChecked: true
      })
    }

    this.onChange()
  }

  render() {
    const { agents = {} } = this.props
    const agentRows = Object.keys(agents).map(i => {
      const deploymentPackage = agents[i].DeploymentPackage || {}
      const deploymentName = deploymentPackage.Name
      const deploymentVersion = deploymentPackage.Version
      return (
        <tr key={i}>
          <td>
            <input
              data-type="agent-selection"
              data-key={i}
              onChange={() => this.onChange()}
              type="checkbox"
            />
          </td>
          <td>{i}</td>
          <td>{deploymentName}</td>
          <td>{deploymentVersion}</td>
          <td>{agents[i].ActiveDeploymentId}</td>
          <td>{agents[i].Status}</td>
          <td>{agents[i].ReportedIpAddresses.join(",")}</td>
          <td>{new Date(agents[i].LastUpdate).toLocaleString()}</td>
        </tr>
      )
    })
    return (
      <div>
        <table className="table is-bordered is-striped is-narrow is-hoverable">
          <thead>
            <tr>
              <th>
                <input onChange={() => this.selectAllNone()} type="checkbox" />
              </th>
              <th>AgentId</th>
              <th>Deployment Package</th>
              <th>Version</th>
              <th>Active DeploymentId</th>
              <th>Status</th>
              <th>IP Addresses</th>
              <th>Last Update</th>
            </tr>
          </thead>
          <tbody>{agentRows}</tbody>
        </table>
      </div>
    )
  }
}

export default AgentTable
