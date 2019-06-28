use config::*;
use serde::Deserialize;
use log::{info, error};
use std::{error::Error, thread, io};
use std::time::{Duration, Instant};
use signal_hook::{iterator::Signals, SIGINT, SIGTERM};
use crossbeam_channel::{bounded, tick, Receiver, select};

mod agent;

fn main() -> Result<(), Box<Error>> {
    log4rs::init_file("conf/log4rs.yml", Default::default()).unwrap();

    let settings = Settings::new()?;
    info!(target: "main", "loaded settings: {:?}", settings);
    
    let start = Instant::now();
    let update = tick(Duration::from_millis(settings.poll_interval));
    let signals = signal_notifier().unwrap();

    let mut agent = agent::Agent::new(&settings.api_endpoint, start);

    info!(target: "main", "deployme agent started");
    loop {
        select! {
            recv(update) -> _ => {
                match agent.poll() {
                    Err(e) => { error!("error: {}", e.to_string()) }
                    Ok(()) => {}
                }
            }
            recv(signals) -> _ => {
                agent.stop();
                break;
            }
        }
    }
    info!(target: "main", "deployme agent exited.");
    Ok(())
}

fn signal_notifier() -> io::Result<Receiver<()>> {
    let (s, r) = bounded(100);
    let signals = Signals::new(&[SIGINT, SIGTERM])?;
    thread::spawn(move || {
        for _ in signals.forever() {
            if s.send(()).is_err() {
                break;
            }
        }
    });
    Ok(r)
}

#[derive(Debug, Deserialize)]
pub struct Settings {
    poll_interval: u64,
    api_endpoint: String
}

impl Settings {
    pub fn new() -> Result<Self, ConfigError> {
        let mut s = Config::new();
        // s.merge(Environment::with_prefix("app"))?;
        s.merge(File::with_name("conf/settings.yml"))?;
        s.try_into()
    }
}
