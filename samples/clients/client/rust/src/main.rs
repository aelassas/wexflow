use reqwest::StatusCode;
use serde::{Deserialize, Serialize};
use std::error::Error;

const BASE_URL: &str = "http://localhost:8000/api/v1";
const USERNAME: &str = "admin";
const PASSWORD: &str = "wexflow2018";
const WORKFLOW_ID: u32 = 41;

#[derive(Serialize)]
struct LoginRequest<'a> {
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
    let token = login(USERNAME, PASSWORD).await?;
    let job_id = start_workflow(&token, WORKFLOW_ID).await?;
    println!("Workflow {} started successfully. Job ID: {}", WORKFLOW_ID, job_id);
    Ok(())
}

async fn login(username: &str, password: &str) -> Result<String, Box<dyn Error>> {
    let client = reqwest::Client::new();
    let login_body = LoginRequest {
        username,
        password,
        stay_connected: false,
    };

    let res = client
        .post(format!("{}/login", BASE_URL))
        .json(&login_body)
        .send()
        .await?;

    if res.status() != StatusCode::OK {
        return Err(format!("Login failed: HTTP {} {}", res.status(), res.text().await?).into());
    }

    let login_response: LoginResponse = res.json().await?;
    Ok(login_response.access_token)
}

async fn start_workflow(token: &str, workflow_id: u32) -> Result<String, Box<dyn Error>> {
    let client = reqwest::Client::new();
    let url = format!("{}/start?w={}", BASE_URL, workflow_id);

    let res = client
        .post(&url)
        .bearer_auth(token)
        .send()
        .await?;

    if res.status() != StatusCode::OK {
        return Err(format!("Failed to start workflow: HTTP {} {}", res.status(), res.text().await?).into());
    }

    let job_id = res.text().await?;
    Ok(job_id)
}
