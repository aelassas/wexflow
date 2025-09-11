# Copy local download of wexflow-linux-netcore.zip
# which was pre-downloaded and unzipped from https://github.com/aelassas/wexflow/releases/latest

# tag_name v9.9
# download https://github.com/aelassas/wexflow/releases/download/v9.9/wexflow-9.9-linux-netcore.zip
# unzip wexflow-9.9-linux-netcore.zip
docker build -t aelassas/wexflow:v9.9 -t aelassas/wexflow:latest wexflow
docker push aelassas/wexflow:v9.9
docker push aelassas/wexflow:latest

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
