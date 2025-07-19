use futures_util::StreamExt;
use reqwest::Client;
use serde::{Deserialize, Serialize};
use std::error::Error;

const BASE_URL: &str = "http://localhost:8000/api/v1";
const USERNAME: &str = "admin";
const PASSWORD: &str = "wexflow2018";
const WORKFLOW_ID: u32 = 41;

#[derive(Serialize)]
struct LoginPayload<'a> {
    username: &'a str,
    password: &'a str,
    #[serde(rename = "stayConnected")]
    stay_connected: bool,
}

#[derive(Deserialize)]
struct LoginResponse {
    access_token: String,
}

#[tokio::main]
async fn main() -> Result<(), Box<dyn Error>> {
    let client = Client::new();

    let token = login(&client).await?;
    let job_id = start_workflow(&client, &token).await?;

    println!("Workflow {} started. Job ID: {}", WORKFLOW_ID, job_id);

    let sse_url = format!("{}/sse/{}/{}", BASE_URL, WORKFLOW_ID, job_id);
    listen_to_sse(&client, &sse_url, &token).await?;

    Ok(())
}

async fn login(client: &Client) -> Result<String, Box<dyn Error>> {
    let payload = LoginPayload {
        username: USERNAME,
        password: PASSWORD,
        stay_connected: false,
    };

    let res = client
        .post(&format!("{}/login", BASE_URL))
        .json(&payload)
        .send()
        .await?;

    if !res.status().is_success() {
        return Err(format!("Login failed: {}", res.status()).into());
    }

    let data: LoginResponse = res.json().await?;
    Ok(data.access_token)
}

async fn start_workflow(client: &Client, token: &str) -> Result<String, Box<dyn Error>> {
    let url = format!("{}/start?w={}", BASE_URL, WORKFLOW_ID);

    let res = client.post(&url).bearer_auth(token).send().await?;

    if !res.status().is_success() {
        return Err(format!("Start workflow failed: {}", res.status()).into());
    }

    let job_id: String = res.json().await?;
    Ok(job_id)
}

async fn listen_to_sse(client: &Client, url: &str, token: &str) -> Result<(), Box<dyn Error>> {
    let res = client
        .get(url)
        .bearer_auth(token)
        .header("Accept", "text/event-stream")
        .send()
        .await?;

    println!("SSE connection opened");

    let mut lines = res.bytes_stream();

    while let Some(chunk) = lines.next().await {
        let bytes = chunk?;
        let line = String::from_utf8_lossy(&bytes);

        for l in line.lines() {
            if l.starts_with("data: ") {
                let json_data = &l[6..];
                match serde_json::from_str::<serde_json::Value>(json_data) {
                    Ok(value) => {
                        println!(
                            "Received SSE JSON:\n{}",
                            serde_json::to_string_pretty(&value)?
                        );
                        println!("SSE connection closed");
                        return Ok(()); // Exit after first message
                    }
                    Err(err) => {
                        println!("Failed to parse SSE JSON: {}", err);
                        return Ok(());
                    }
                }
            }
        }
    }

    Ok(())
}
