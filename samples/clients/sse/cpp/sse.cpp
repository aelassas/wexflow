#include <iostream>
#include <string>
#include <curl/curl.h>
#include <nlohmann/json.hpp>
#include <thread>
#include <atomic>

using json = nlohmann::json;

static std::string baseUrl = "http://localhost:8000/api/v1";
static std::string username = "admin";
static std::string password = "wexflow2018";
static int workflowId = 41;

// Helper for libcurl response data
static size_t WriteCallback(void *contents, size_t size, size_t nmemb, void *userp)
{
  ((std::string *)userp)->append((char *)contents, size * nmemb);
  return size * nmemb;
}

// Perform HTTP POST with JSON payload, return response body as string
std::string httpPost(const std::string &url, const std::string &jsonPayload, const std::string &bearerToken = "")
{
  CURL *curl = curl_easy_init();
  if (!curl)
    throw std::runtime_error("Failed to init curl");

  std::string readBuffer;
  struct curl_slist *headers = nullptr;

  headers = curl_slist_append(headers, "Content-Type: application/json");
  if (!bearerToken.empty())
  {
    std::string authHeader = "Authorization: Bearer " + bearerToken;
    headers = curl_slist_append(headers, authHeader.c_str());
  }

  curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
  curl_easy_setopt(curl, CURLOPT_HTTPHEADER, headers);
  curl_easy_setopt(curl, CURLOPT_POSTFIELDS, jsonPayload.c_str());
  curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
  curl_easy_setopt(curl, CURLOPT_WRITEDATA, &readBuffer);

  CURLcode res = curl_easy_perform(curl);

  long httpCode = 0;
  curl_easy_getinfo(curl, CURLINFO_RESPONSE_CODE, &httpCode);

  curl_slist_free_all(headers);
  curl_easy_cleanup(curl);

  if (res != CURLE_OK)
  {
    throw std::runtime_error(std::string("curl_easy_perform() failed: ") + curl_easy_strerror(res));
  }
  if (httpCode < 200 || httpCode >= 300)
  {
    throw std::runtime_error("HTTP error code: " + std::to_string(httpCode));
  }

  return readBuffer;
}

// Login to get token
std::string login(const std::string &user, const std::string &pass)
{
  json j;
  j["username"] = user;
  j["password"] = pass;
  j["stayConnected"] = false;

  std::string url = baseUrl + "/login";
  return json::parse(httpPost(url, j.dump()))["access_token"];
}

// Start workflow, returns jobId string
std::string startWorkflow(const std::string &token, int workflowId)
{
  std::string url = baseUrl + "/start?w=" + std::to_string(workflowId);
  // POST empty body for start
  std::string raw = httpPost(url, "", token);
  json j = json::parse(raw);
  return j.get<std::string>();
}

// SSE event handler
size_t sseWriteCallback(char *ptr, size_t size, size_t nmemb, void *userdata)
{
  size_t totalSize = size * nmemb;
  std::string chunk(ptr, totalSize);
  std::cout << ">>> SSE Chunk received:\n"
            << chunk << std::endl;

  static std::string buffer;
  buffer += chunk;

  size_t pos;
  while ((pos = buffer.find("\n\n")) != std::string::npos)
  {
    std::string eventBlock = buffer.substr(0, pos);
    buffer.erase(0, pos + 2);

    size_t dataPos = eventBlock.find("data: ");
    if (dataPos != std::string::npos)
    {
      std::string jsonData = eventBlock.substr(dataPos + 6);
      std::cout << "Received SSE data: " << jsonData << std::endl;

      try
      {
        auto j = json::parse(jsonData);
        if (j.contains("status") && j["status"] == "Done")
        {
          std::cout << "Workflow status Done received, exiting SSE loop." << std::endl;
          std::atomic<bool> *doneFlag = static_cast<std::atomic<bool> *>(userdata);
          doneFlag->store(true);
        }
      }
      catch (const std::exception &e)
      {
        std::cerr << "JSON parse error: " << e.what() << std::endl;
      }
    }
  }

  return totalSize;
}

// Listen to SSE stream until "Done" status
void listenToSse(const std::string &sseUrl, const std::string &token)
{
  CURL *curl = curl_easy_init();
  if (!curl)
  {
    throw std::runtime_error("Failed to init curl");
  }

  std::atomic<bool> done(false);

  struct curl_slist *headers = nullptr;
  headers = curl_slist_append(headers, "Accept: text/event-stream");
  headers = curl_slist_append(headers, ("Authorization: Bearer " + token).c_str());
  headers = curl_slist_append(headers, "Expect:"); // Disable 'Expect: 100-continue'

  curl_easy_setopt(curl, CURLOPT_URL, sseUrl.c_str());
  curl_easy_setopt(curl, CURLOPT_HTTPHEADER, headers);
  curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, sseWriteCallback);
  curl_easy_setopt(curl, CURLOPT_WRITEDATA, &done);
  curl_easy_setopt(curl, CURLOPT_TIMEOUT, 0L);       // Keep connection open indefinitely
  curl_easy_setopt(curl, CURLOPT_TCP_KEEPALIVE, 1L); // Keep connection alive
  curl_easy_setopt(curl, CURLOPT_NOSIGNAL, 1L);      // Required for multithreaded / Windows apps
  curl_easy_setopt(curl, CURLOPT_VERBOSE, 1L);       // Optional: enable debug output

  CURLcode res = curl_easy_perform(curl);
  if (res != CURLE_OK)
  {
    std::cerr << "SSE connection error: " << curl_easy_strerror(res) << std::endl;
  }

  curl_slist_free_all(headers);
  curl_easy_cleanup(curl);
}

int main()
{
  try
  {
    std::string token = login(username, password);
    std::string jobId = startWorkflow(token, workflowId);

    std::cout << "Workflow " << workflowId << " started. Job ID: " << jobId << std::endl;

    std::string sseUrl = baseUrl + "/sse/" + std::to_string(workflowId) + "/" + jobId;

    listenToSse(sseUrl, token);

    std::cout << "SSE listening finished. Exiting." << std::endl;
  }
  catch (const std::exception &e)
  {
    std::cerr << "Error: " << e.what() << std::endl;
  }
  return 0;
}
