const express = require("express")
const next = require("next")
const axios = require("axios")

const port = parseInt(process.env.PORT, 10) || 3000
const dev = process.env.NODE_ENV !== "production"
const apiEndpoint = process.env.APP_API_ENDPOINT || "http://api"
const app = next({ dev })
const handle = app.getRequestHandler()

app.prepare().then(() => {
  const server = express()

  server.use(express.json())

  server.get("/api/agents", (req, res) => {
    const client = apiClient(apiEndpoint)
    client
      .getAgents()
      .then(i => res.json(i.data))
      .catch(e => res.status(e.response.status || 500).json(e.response.data))
  })

  server.post("/api/deployments", (req, res) => {
    const client = apiClient(apiEndpoint)
    client
      .postDeployment(req.body)
      .then(i => res.json(i.data))
      .catch(e => res.status(e.response.status || 500).json(e.response.data))
  })

  server.get("*", (req, res) => {
    return handle(req, res)
  })

  server.listen(port, err => {
    if (err) throw err
    console.log(`> Ready on http://localhost:${port}`)
  })
})

const getAgents = baseUrl => () => axios.get(`${baseUrl}/agents`)
const postDeployment = baseUrl => data =>
  axios.post(`${baseUrl}/deployments`, data)
const apiClient = baseUrl => ({
  getAgents: getAgents(baseUrl),
  postDeployment: postDeployment(baseUrl)
})
