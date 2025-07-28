# Copy local download of wexflow-linux-netcore.zip
# which was pre-downloaded and unzipped from https://github.com/aelassas/wexflow/releases/latest

docker build -t aelassas/wexflow:v9.9 -t aelassas/wexflow:latest .
docker run -d -p 8000:8000 --name wexflow aelassas/wexflow:latest
docker push aelassas/wexflow:v9.9
docker push aelassas/wexflow:latest
docker run -p 8000:8000 aelassas/wexflow:latest
docker stop wexflow
docker rm wexflow

# Test locally
docker build -t wexflow-test .
docker run -d -p 8000:8000 --name wexflow-test wexflow-test
docker logs -f wexflow-test
docker stop wexflow-test
docker rm wexflow-test
