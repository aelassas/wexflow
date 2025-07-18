import requests

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

try:
    token = login(username, password)
    headers = {
        'Authorization': f'Bearer {token}'
    }
    response = requests.post(f'{base_url}/start?w={workflow_id}', headers=headers)
    if response.status_code != 200:
        raise Exception(f'HTTP {response.status_code} - {response.reason}')

    job_id = response.json()
    print(f'Workflow {workflow_id} started successfully. Job ID: {job_id}')
except Exception as e:
    print(f'Failed to start workflow {workflow_id}: {e}')
  