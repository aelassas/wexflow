const baseUrl = 'http://localhost:8000/api/v1'
const username = 'admin'
const password = 'wexflow2018'
const workflowId = 41

async function login(user, pass, stayConnected = false) {
  const res = await fetch(`${baseUrl}/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username: user, password: pass, stayConnected })
  })
  if (!res.ok) {
    throw new Error(`HTTP ${res.status} - ${res.statusText}`)
  }
  const data = await res.json()
  return data.access_token
}

try {
  const token = await login(username, password)
  const res = await fetch(`${baseUrl}/start?w=${workflowId}`, {
    method: 'POST',
    headers: { 'Authorization': `Bearer ${token}` }
  })
  if (!res.ok) {
    throw new Error(`HTTP ${res.status} - ${res.statusText}`)
  }
  const jobId = await res.json()
  console.log(`Workflow ${workflowId} started successfully. Job ID:`, jobId)
} catch (err) {
  console.error(`Failed to start workflow ${workflowId}:`, err)
}
