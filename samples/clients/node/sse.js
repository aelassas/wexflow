import * as eventsource from 'eventsource'

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

async function startWorkflow(token, workflowId) {
  const res = await fetch(`${baseUrl}/start?w=${workflowId}`, {
    method: 'POST',
    headers: { 'Authorization': `Bearer ${token}` }
  })
  if (!res.ok) {
    throw new Error(`HTTP ${res.status} - ${res.statusText}`)
  }
  const jobId = await res.json()
  return jobId
}


try {
  const token = await login(username, password)
  const jobId = await startWorkflow(token, workflowId)
  console.log(`Workflow ${workflowId} started. Job ID: ${jobId}`)

  const sseUrl = `${baseUrl}/sse/${workflowId}/${jobId}`
  const es = new eventsource.EventSource(sseUrl, {
    fetch: (input, init) =>
      fetch(input, {
        ...init,
        headers: {
          ...init.headers,
          Authorization: `Bearer ${token}`
        },
      }),
  })

  es.onopen = () => {
    console.log('SSE connection opened')
  }

  es.onmessage = (event) => {
    try {
      // This event is triggered when the workflow job finishes or stops.
      // The SSE data arrives as a JSON-formatted string in event.data.
      // Parse this string into a JavaScript object for easy access to properties.
      // Check this doc for full list of statuse: 
      // https://github.com/aelassas/wexflow/wiki/Workflow-Notifications-via-SSE#statuses
      const data = JSON.parse(event.data)
      console.log('Received SSE JSON:', data)

      // Access properties like data.workflowId, data.jobId, data.status, data.name, data.description

      // Close connection if needed, e.g. after final status received
      es.close()
    } catch (err) {
      console.error('Failed to parse SSE JSON:', err)
    }
  }

  es.onerror = (err) => {
    console.error('SSE error:', err)
    es.close()
  }
} catch (err) {
  console.error('Error:', err)
}
