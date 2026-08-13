import type { ReactNode } from 'react'

type AppProps = {
  children: ReactNode
}

export default function App({ children }: AppProps) {
  return children
}
