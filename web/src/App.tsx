import { useStatus } from './api/status'

export default function App() {
  const status = useStatus()

  return (
    <main>
      <h1>Webhook Relay</h1>
      {status.isPending && <p>Checking API…</p>}
      {status.isError && <p role="alert">API unreachable: {status.error.message}</p>}
      {status.isSuccess && (
        <p>Database: {status.data.databaseReachable ? 'reachable' : 'unreachable'}</p>
      )}
    </main>
  )
}
