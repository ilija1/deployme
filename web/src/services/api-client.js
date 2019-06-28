import axios from "axios"

const wrap = promise => {
  const wrappedPromise = promise
    .then(i => ({
      error: false,
      status: i.status,
      data: i.data.Result
    }))
    .catch(({ response = {} }) => ({
      error: true,
      status: response.status,
      data: response.data,
      statusText: response.statusText,
      headers: response.headers
    }))
  wrappedPromise.unwrap = (val = undefined) =>
    wrappedPromise.then(i => (i.error ? val : i.data))
  return wrappedPromise
}

const getAgents = baseUrl => () => wrap(axios.get(`${baseUrl}/agents`))

const postDeployment = baseUrl => data =>
  wrap(axios.post(`${baseUrl}/deployments`, data))

export default () => ({
  getAgents: getAgents("/api"),
  postDeployment: postDeployment("/api")
})
