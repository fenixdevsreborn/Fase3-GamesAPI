param(
  [string]$Namespace = "fase4",
  [string]$Image = "adinteltidev/games-api:latest"
)

$ErrorActionPreference = "Stop"

docker build -t $Image .

kubectl apply -f k8s/local/00-namespace.yaml
kubectl apply -f k8s/local/01-secrets.yaml
kubectl apply -f k8s/local/01-rabbitmq-secrets.yaml
kubectl apply -f k8s/local/02-rabbitmq.yaml
kubectl apply -f k8s/local/03-dynamodb-local.yaml
kubectl apply -f k8s/local/03-dynamodb-init-job.yaml
kubectl apply -f k8s/local/04-games-api.yaml
kubectl apply -f k8s/local/05-service.yaml

kubectl rollout status deployment/games-api -n $Namespace
kubectl get pods -n $Namespace
