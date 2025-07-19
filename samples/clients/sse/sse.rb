require 'net/http'
require 'uri'
require 'json'
require 'em-eventsource'

BASE_URL = 'http://localhost:8000/api/v1'
USERNAME = 'admin'
PASSWORD = 'wexflow2018'
WORKFLOW_ID = 41

def login(username, password, stay_connected = false)
  uri = URI("#{BASE_URL}/login")
  req = Net::HTTP::Post.new(uri, 'Content-Type' => 'application/json')
  req.body = { username: username, password: password, stayConnected: stay_connected }.to_json

  res = Net::HTTP.start(uri.hostname, uri.port) do |http|
    http.request(req)
  end

  unless res.is_a?(Net::HTTPSuccess)
    raise "Login failed: #{res.code} #{res.message}"
  end

  json = JSON.parse(res.body)
  json['access_token']
end

def start_workflow(token, workflow_id)
  uri = URI("#{BASE_URL}/start?w=#{workflow_id}")
  req = Net::HTTP::Post.new(uri, 'Authorization' => "Bearer #{token}")

  res = Net::HTTP.start(uri.hostname, uri.port) do |http|
    http.request(req)
  end

  unless res.is_a?(Net::HTTPSuccess)
    raise "Start workflow failed: #{res.code} #{res.message}"
  end

  JSON.parse(res.body)
end

begin
  token = login(USERNAME, PASSWORD)
  job_id = start_workflow(token, WORKFLOW_ID)
  puts "Workflow #{WORKFLOW_ID} started. Job ID: #{job_id}"

  sse_url = "#{BASE_URL}/sse/#{WORKFLOW_ID}/#{job_id}"

  EM.run do
    source = EventMachine::EventSource.new(sse_url, nil, { 'Authorization' => "Bearer #{token}" })

    source.message do |msg|
      begin
        data = JSON.parse(msg)
        puts "Received SSE JSON: #{data}"
        source.close
      rescue => e
        puts "Failed to parse SSE JSON: #{e.message}"
      end
    end

    source.error do |err|
      puts "SSE error: #{err}"
      source.close
    end

    source.open do
      puts "SSE connection opened"
    end

    source.start
  end

rescue => e
  puts "Error: #{e.message}"
end
