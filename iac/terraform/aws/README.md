# Deploy da Games API no AWS EKS

Este diretorio cria a infraestrutura AWS para executar a `ms-games` como Web API .NET 8 no Amazon EKS.

## Recursos criados

- VPC com subnets publicas e privadas em 3 AZs.
- Amazon EKS com managed node group.
- AWS Load Balancer Controller via Helm.
- Metrics Server via Helm.
- Tabela DynamoDB `Games`.
- IAM Role IRSA para a ServiceAccount `games-api` acessar DynamoDB.

## Execucao

```powershell
cd iac/terraform/aws
Copy-Item terraform.tfvars.example terraform.tfvars
terraform init
terraform plan
terraform apply
```

Depois do `apply`, copie o valor de `games_api_role_arn` para a anotacao `eks.amazonaws.com/role-arn` em `../../../k8s/eks/02-serviceaccount.yaml`.

Configure o `kubectl`:

```powershell
aws eks update-kubeconfig --region us-east-1 --name fase3-games-api-dev
```

Publique a imagem no Docker Hub e aplique os manifests:

```powershell
kubectl apply -f ../../../k8s/eks/00-namespace.yaml
kubectl apply -f ../../../k8s/eks/01-secrets.yaml
kubectl apply -f ../../../k8s/eks/02-serviceaccount.yaml
kubectl apply -f ../../../k8s/eks/03-rabbitmq.yaml
kubectl apply -f ../../../k8s/eks/04-games-api.yaml
kubectl apply -f ../../../k8s/eks/05-service.yaml
kubectl apply -f ../../../k8s/eks/06-hpa.yaml
kubectl apply -f ../../../k8s/eks/07-ingress.yaml
```

## Ajustes antes de producao

- Trocar os secrets JWT/RabbitMQ dos YAMLs por AWS Secrets Manager ou External Secrets.
- Usar RabbitMQ gerenciado/HA, como Amazon MQ, se o requisito de disponibilidade exigir.
- Configurar dominio e certificado ACM no Ingress.
- Revisar custos do EKS, NAT Gateway e node group antes de manter o ambiente ativo.
