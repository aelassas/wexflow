import requests
import sseclient
import json

base_url = 'http://localhost:8000/api/v1'
username = 'admin'
password = 'wexflow2018'
workflow_id = 41

def login(user, passwd, stay_connected=False):
    url = f'{base_url}/login'
    headers = {'Content-Type': 'application/json'}
    payload = {
        'username': user,
        'password': passwd,
        'stayConnected': stay_connected
    }

    response = requests.post(url, json=payload, headers=headers)
    if response.status_code != 200:
        raise Exception(f'HTTP {response.status_code} - {response.reason}')
    
    return response.json()['access_token']

def start_workflow(token, workflow_id):
    url = f'{base_url}/start?w={workflow_id}'
    headers = {
        'Authorization': f'Bearer {token}'
    }

    response = requests.post(url, headers=headers)
    if response.status_code != 200:
        raise Exception(f'HTTP {response.status_code} - {response.reason}')
    
    return response.json()

def listen_to_sse(url, token):
    headers = {
        'Authorization': f'Bearer {token}',
        'Accept': 'text/event-stream'
    }
    response = requests.get(url, headers=headers, stream=True)

    client = sseclient.SSEClient(response)
    print('SSE connection opened')

    for event in client.events():
        try:
            data = json.loads(event.data)
            print('Received SSE JSON:')
            print(json.dumps(data, indent=2))
            break  # Like JS code, stop after first message
        except Exception as e:
            print(f'Failed to parse SSE JSON: {e}')
            break

    print('SSE connection closed')

# Main execution
try:
    token = login(username, password)
    job_id = start_workflow(token, workflow_id)
    print(f'Workflow {workflow_id} started successfully. Job ID: {job_id}')

    sse_url = f'{base_url}/sse/{workflow_id}/{job_id}'
    listen_to_sse(sse_url, token)

except Exception as e:
    print(f'Error: {e}')
