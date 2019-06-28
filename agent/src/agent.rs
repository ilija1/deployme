use log::{info};
use pnet::datalink;
use serde::{Serialize, Deserialize};
use serde_json::Value;
use std::collections::HashMap;
use std::error::Error;
use std::process::Command;
use std::time::Instant;

pub struct Agent {
    api_endpoint: String,
    start_time: Instant,
    client: reqwest::Client,
    agent_info: AgentInfo,
    deployment: Option<Deployment>,
}

impl Agent {
    pub fn new(api_endpoint: &str, start_time: Instant) -> Agent {
        let client = reqwest::Client::new();
        Agent { 
            api_endpoint: api_endpoint.to_string(), 
            start_time: start_time, 
            client: client, 
            deployment: None,
            agent_info: AgentInfo {
                deployment_package: None,
                active_deployment_id: None,
                reported_ip_addresses: vec![],
                status: AgentStatus::Ready,
            }
        }
    }

    pub fn poll(&mut self) -> Result<(), Box<Error>> {
        self.refresh_ips();
        info!("{:?}: {:?}", self.start_time.elapsed(), self.agent_info);

        let url = format!("{}/communication/poll", self.api_endpoint);
        let resp: ResponseContainer<DeploymentCommand> = self.client.post(&url).json(&self.agent_info).send()?.json()?;

        if self.agent_info.status != AgentStatus::Ready {
            return Ok(())
        }

        let deployment_command = resp.result.unwrap();
        let new_deployment = deployment_command.deployment;
        if deployment_command.execute {
            match new_deployment {
                Some(i) => {
                    self.agent_info.status = AgentStatus::Busy;
                    let result = self.deploy(i);
                    match result {
                        Ok(j) => {
                            self.report(j)?;
                            self.agent_info.status = AgentStatus::Ready;
                        },
                        Err(j) => {
                            self.agent_info.status = AgentStatus::Error;
                            self.report(j)?;
                        }
                    }
                },
                None => {}
            }
        } else {
            match new_deployment {
                Some(i) => {
                    self.update_agent_deployment(i);
                },
                None => {}
            }
        }

        Ok(())
    }

    pub fn stop(&self) {
        info!("agent stopped: {:?}", self.start_time.elapsed());
    }

    fn deploy(&mut self, curr: Deployment) -> Result<Vec<CommandResult>, Vec<CommandResult>> {
        let prev = self.deployment.clone();
        info!("old deployment: {:?}", prev);
        info!("new deployment: {:?}", curr);

        let mut outputs = Vec::new();
        match prev {
            Some(i) => {
                for cmd in i.stop_commands.iter().chain(i.uninstall_commands.iter()) {
                    let output = self.run_cmd(cmd.to_string());
                    let success = output.success;
                    outputs.push(output);
                    if ! success {
                        return Err(outputs)
                    }
                }
                
            },
            None => {}
        }

        for cmd in curr.install_commands.iter().chain(curr.start_commands.iter()) {
            let output = self.run_cmd(cmd.to_string());
            let success = output.success;
            outputs.push(output);
            if ! success {
                return Err(outputs)
            }
        }

        info!("outputs: {:?}", outputs);
        self.update_agent_deployment(curr);
        Ok(outputs)
    }

    fn report(&self, outputs: Vec<CommandResult>) -> Result<(), Box<Error>> {
        let url = format!("{}/communication/report", self.api_endpoint);
        let deployment_result = DeploymentResult { agent_info: self.agent_info.clone(), command_results: outputs };
        self.client.post(&url).json(&deployment_result).send()?;
        Ok(())
    }

    fn run_cmd(&self, cmd: String) -> CommandResult {
        let mut iter = cmd.split(" ");
        let first = iter.next();
        match first {
            Some(i) => {
                let args: Vec<&str> = iter.collect();
                let output = Command::new(i)
                    .args(args)
                    .output();
                match output {
                    Ok(j) => {
                        CommandResult { 
                            command: cmd,
                            success: j.status.success(), 
                            error: None, 
                            code: j.status.code(), 
                            stdout: Some(String::from_utf8_lossy(&j.stdout).to_string()), 
                            stderr: Some(String::from_utf8_lossy(&j.stderr).to_string()),
                        }
                    },
                    Err(e) => {
                        CommandResult { 
                            command: cmd, 
                            success: false, 
                            error: Some(e.to_string()), 
                            code: None, 
                            stdout: None, 
                            stderr: None 
                        }
                    }
                }
            },
            None => {
                CommandResult { 
                    command: cmd, 
                    success: false, 
                    error: Some("Found empty command line".to_string()), 
                    code: None, 
                    stdout: None, 
                    stderr: None 
                }
            }
        }
    }

    fn update_agent_deployment(&mut self, deployment: Deployment) {
        self.agent_info.active_deployment_id = Some(deployment.id.clone());
        self.agent_info.deployment_package = Some(deployment.deployment_package.clone());
        self.deployment = Some(deployment);
    }

    fn refresh_ips(&mut self) {
        self.agent_info.reported_ip_addresses = self.get_ips();
    }

    fn get_ips(&self) -> Vec<String> {
        let interfaces = datalink::interfaces();
        let ips: Vec<String> = interfaces.into_iter()
            .flat_map(|i| i.ips)
            .filter(|i| i.is_ipv4())
            .map(|i| i.ip())
            .filter(|i| !i.is_loopback())
            .map(|i| i.to_string())
            .collect();
        ips
    }
}

#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
struct ResponseContainer<U> {
    result: Option<U>,
    error_message: Option<String>,
    code: i16,
    details: Option<HashMap<String, Value>>,
}

#[derive(Debug, Serialize, Deserialize, Clone, PartialEq)]
enum AgentStatus {
    Ready,
    Busy,
    Error,
}

#[derive(Debug, Serialize, Deserialize, PartialEq, Clone)]
#[serde(rename_all = "PascalCase")]
struct DeploymentPackage {
    name: String,
    version: String,
}

#[derive(Debug, Serialize, Deserialize, Clone)]
#[serde(rename_all = "PascalCase")]
struct AgentInfo {
    deployment_package: Option<DeploymentPackage>,
    active_deployment_id: Option<String>,
    reported_ip_addresses: Vec<String>,
    status: AgentStatus,
}

#[derive(Debug, Deserialize, Clone)]
#[serde(rename_all = "PascalCase")]
struct Deployment {
    deployment_package: DeploymentPackage,
    install_commands: Vec<String>,
    start_commands: Vec<String>,
    stop_commands: Vec<String>,
    uninstall_commands: Vec<String>,
    timestamp: i64,
    id: String,
}

#[derive(Debug, Deserialize, Clone)]
#[serde(rename_all = "PascalCase")]
struct DeploymentCommand {
    deployment: Option<Deployment>,
    execute: bool,
}

#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
struct CommandResult {
    success: bool,
    command: String,
    error: Option<String>,
    code: Option<i32>,
    stdout: Option<String>,
    stderr: Option<String>,
}

#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
struct DeploymentResult {
    agent_info: AgentInfo,
    command_results: Vec<CommandResult>,
}
