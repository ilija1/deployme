FROM node

WORKDIR /usr/src/deployme/web
COPY . .

WORKDIR /usr/src/deployme/web/src
RUN npm install
RUN npm run build

CMD ["npm", "run", "pm2-runtime"]
