param(
  [string]$Namespace = "fase4",
  [string]$Region = "us-east-1",
  [string]$ClusterName = "fase3-games-api-dev",
  [string]$Image = "adinteltidev/games-api:latest",
  [string]$GamesApiRoleArn = ""
)

$ErrorActionPreference = "Stop"

aws eks update-kubeconfig --name $ClusterName --region $Region

kubectl apply -f k8s/eks/00-namespace.yaml
kubectl apply -f k8s/eks/01-secrets.yaml
kubectl apply -f k8s/eks/02-serviceaccount.yaml

if ($GamesApiRoleArn -ne "") {
  kubectl annotate serviceaccount games-api `
    eks.amazonaws.com/role-arn=$GamesApiRoleArn `
    -n $Namespace `
    --overwrite
}

kubectl apply -f k8s/eks/03-rabbitmq.yaml
kubectl apply -f k8s/eks/04-games-api.yaml
kubectl apply -f k8s/eks/05-service.yaml
kubectl apply -f k8s/eks/06-hpa.yaml
kubectl apply -f k8s/eks/07-ingress.yaml

kubectl set image deployment/games-api games-api=$Image -n $Namespace
kubectl rollout status deployment/games-api -n $Namespace
kubectl get ingress -n $Namespace
