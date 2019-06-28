#!/bin/bash

apt-get update -y
apt-get upgrade -y
apt-get install -y curl sudo gcc libssl-dev openssl pkg-config
curl https://sh.rustup.rs -sSf | sh -s -- -y

source $HOME/.cargo/env

cargo build --release
