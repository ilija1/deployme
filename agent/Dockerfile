FROM ubuntu

WORKDIR /usr/src/deployme/agent
COPY . .

RUN ./install.sh

CMD ["sudo", "/usr/src/deployme/agent/target/release/deployme-agent"]
