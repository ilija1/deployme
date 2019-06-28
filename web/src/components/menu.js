import Link from "next/link"

export default props => (
  <aside className={props.className}>
    <p className="menu-label">deployme</p>
    <ul className="menu-list">
      <li>
        <Link href="/">
          <a>Home</a>
        </Link>
      </li>
      <li>
        <a>About</a>
      </li>
    </ul>
  </aside>
)
