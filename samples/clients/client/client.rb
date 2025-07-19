require 'net/http'
require 'json'
require 'uri'

BASE_URL = 'http://localhost:8000/api/v1'
USERNAME = 'admin'
PASSWORD = 'wexflow2018'
WORKFLOW_ID = 41

def login(user, pass, stay_connected = false)
  uri = URI("#{BASE_URL}/login")
  req = Net::HTTP::Post.new(uri, 'Content-Type' => 'application/json')
  req.body = {
    username: user,
    password: pass,
    stayConnected: stay_connected
  }.to_json

  res = Net::HTTP.start(uri.hostname, uri.port) { |http| http.request(req) }

  unless res.is_a?(Net::HTTPSuccess)
    raise "Login failed: HTTP #{res.code} #{res.message}"
  end

  data = JSON.parse(res.body)
  data['access_token']
end

def start_workflow(token, workflow_id)
  uri = URI("#{BASE_URL}/start?w=#{workflow_id}")
  req = Net::HTTP::Post.new(uri)
  req['Authorization'] = "Bearer #{token}"

  res = Net::HTTP.start(uri.hostname, uri.port) { |http| http.request(req) }

  unless res.is_a?(Net::HTTPSuccess)
    raise "Failed to start workflow: HTTP #{res.code} #{res.message}"
  end

  res.body
end

begin
  token = login(USERNAME, PASSWORD)
  job_id = start_workflow(token, WORKFLOW_ID)
  puts "Workflow #{WORKFLOW_ID} started successfully. Job ID: #{job_id}"
rescue => e
  puts "Error: #{e}"
end
