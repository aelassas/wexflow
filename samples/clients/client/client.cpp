#include <iostream>
#include <string>
#include <curl/curl.h>

static size_t WriteCallback(void *contents, size_t size, size_t nmemb, std::string *s)
{
  size_t totalSize = size * nmemb;
  s->append((char *)contents, totalSize);
  return totalSize;
}

std::string login(const std::string &baseUrl, const std::string &username, const std::string &password)
{
  CURL *curl = curl_easy_init();
  if (!curl)
    throw std::runtime_error("Failed to init curl");

  std::string url = baseUrl + "/login";
  std::string readBuffer;
  std::string jsonData = "{\"username\":\"" + username + "\",\"password\":\"" + password + "\",\"stayConnected\":false}";

  struct curl_slist *headers = nullptr;
  headers = curl_slist_append(headers, "Content-Type: application/json");

  curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
  curl_easy_setopt(curl, CURLOPT_HTTPHEADER, headers);
  curl_easy_setopt(curl, CURLOPT_POSTFIELDS, jsonData.c_str());

  curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
  curl_easy_setopt(curl, CURLOPT_WRITEDATA, &readBuffer);

  CURLcode res = curl_easy_perform(curl);
  if (res != CURLE_OK)
  {
    curl_slist_free_all(headers);
    curl_easy_cleanup(curl);
    throw std::runtime_error(std::string("curl_easy_perform() failed: ") + curl_easy_strerror(res));
  }

  long http_code = 0;
  curl_easy_getinfo(curl, CURLINFO_RESPONSE_CODE, &http_code);
  curl_slist_free_all(headers);
  curl_easy_cleanup(curl);

  if (http_code != 200)
    throw std::runtime_error("Login failed with HTTP code " + std::to_string(http_code));

  // Simple parsing to extract access_token value from JSON response
  // For production, use a JSON parser like nlohmann/json
  auto startPos = readBuffer.find("\"access_token\":\"");
  if (startPos == std::string::npos)
    throw std::runtime_error("access_token not found in response");

  startPos += strlen("\"access_token\":\"");
  auto endPos = readBuffer.find("\"", startPos);
  if (endPos == std::string::npos)
    throw std::runtime_error("Malformed access_token in response");

  return readBuffer.substr(startPos, endPos - startPos);
}

std::string startWorkflow(const std::string &baseUrl, const std::string &token, int workflowId)
{
  CURL *curl = curl_easy_init();
  if (!curl)
    throw std::runtime_error("Failed to init curl");

  std::string url = baseUrl + "/start?w=" + std::to_string(workflowId);
  std::string readBuffer;

  struct curl_slist *headers = nullptr;
  std::string authHeader = "Authorization: Bearer " + token;

  headers = curl_slist_append(headers, authHeader.c_str());
  headers = curl_slist_append(headers, "Content-Type: application/json");
  headers = curl_slist_append(headers, "Content-Length: 0");

  curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
  curl_easy_setopt(curl, CURLOPT_HTTPHEADER, headers);
  curl_easy_setopt(curl, CURLOPT_POST, 1L);

  curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
  curl_easy_setopt(curl, CURLOPT_WRITEDATA, &readBuffer);

  CURLcode res = curl_easy_perform(curl);
  if (res != CURLE_OK)
  {
    curl_slist_free_all(headers);
    curl_easy_cleanup(curl);
    throw std::runtime_error(std::string("curl_easy_perform() failed: ") + curl_easy_strerror(res));
  }

  long http_code = 0;
  curl_easy_getinfo(curl, CURLINFO_RESPONSE_CODE, &http_code);
  curl_slist_free_all(headers);
  curl_easy_cleanup(curl);

  if (http_code != 200)
    throw std::runtime_error("Start workflow failed with HTTP code " + std::to_string(http_code));

  return readBuffer;
}

int main()
{
  const std::string baseUrl = "http://localhost:8000/api/v1";
  const std::string username = "admin";
  const std::string password = "wexflow2018";
  const int workflowId = 41;

  try
  {
    std::string token = login(baseUrl, username, password);
    std::string jobId = startWorkflow(baseUrl, token, workflowId);
    std::cout << "Workflow " << workflowId << " started successfully. Job ID: " << jobId << std::endl;
  }
  catch (const std::exception &e)
  {
    std::cerr << "Error: " << e.what() << std::endl;
    return 1;
  }
  return 0;
}
