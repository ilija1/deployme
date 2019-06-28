import Head from "next/head"
import "bulma/css/bulma.min.css"
import css from "./meta.styl"

export default () => (
  <div>
    <Head>
      <meta name="viewport" content="width=device-width, initial-scale=1" />
      <meta charSet="utf-8" />
      <title>DeployMe</title>
    </Head>
    <style jsx global>
      {css}
    </style>
  </div>
)
