# 🎮 Games API - Fase 3 (MVP AWS)

## 📌 Visão Geral

A **Games API** é um serviço backend desenvolvido como parte da Fase 3 do projeto, com foco em **arquitetura escalável, serverless e orientada a serviços**, utilizando recursos da AWS.

Este projeto tem como objetivo fornecer uma API robusta para gerenciamento de dados relacionados a jogos, permitindo operações seguras, performáticas e desacopladas, seguindo boas práticas de engenharia de software.

---

## 🎯 Objetivos do Projeto

* Disponibilizar endpoints para gerenciamento de jogos
* Implementar arquitetura **serverless com AWS**
* Garantir **alta disponibilidade e escalabilidade automática**
* Aplicar princípios de:

  * Clean Architecture
  * SOLID
  * Domain-driven design (DDD - quando aplicável)

---

## 🏗️ Arquitetura

O projeto segue uma abordagem baseada em **arquitetura hexagonal (Ports & Adapters)**, promovendo desacoplamento entre domínio e infraestrutura.

### 🔹 Camadas

* **Domain**

  * Entidades
  * Regras de negócio
  * Interfaces (ports)

* **Application**

  * Casos de uso (use cases)
  * Orquestração das regras de negócio

* **Infrastructure**

  * Integrações externas (AWS, banco, etc)
  * Implementações de repositórios

* **API / EntryPoint**

  * Lambdas / Controllers
  * Mapeamento HTTP

---

## ☁️ Infraestrutura (AWS)

O projeto utiliza uma stack moderna baseada em serviços gerenciados:

* **AWS Lambda**

  * Execução serverless dos endpoints

* **Amazon API Gateway**

  * Exposição dos endpoints HTTP

* **Amazon DynamoDB**

  * Banco de dados NoSQL (alta performance e escalabilidade)

* **AWS IAM**

  * Controle de permissões e segurança

* **AWS CloudWatch**

  * Logs e monitoramento

---

## 🔗 Principais Funcionalidades

* 📥 Cadastro de jogos
* 📄 Consulta de jogos
* ✏️ Atualização de dados
* ❌ Remoção de registros
* 🔍 Filtros e buscas

---

## 🔐 Segurança

* Validação de entrada (input validation)
* Controle de acesso via políticas IAM
* Possível integração com JWT (caso implementado)

---

## 🚀 Tecnologias Utilizadas

* **.NET 8**
* **C#**
* **AWS Lambda**
* **API Gateway**
* **DynamoDB**
* **Terraform / IaC (se aplicável)**

---

## ⚙️ Como Executar o Projeto

### 🔧 Pré-requisitos

* .NET 8 SDK
* AWS CLI configurado
* Conta AWS ativa
* Ferramentas:

  * `dotnet lambda`
  * Terraform (opcional)

---

### ▶️ Execução local

```bash
dotnet restore
dotnet build
dotnet run
```

---

### ☁️ Deploy na AWS

```bash
dotnet lambda deploy-serverless
```

Ou via Terraform:

```bash
terraform init
terraform apply
```

---

## 📦 Estrutura do Projeto

```
src/
 ├── Domain/
 ├── Application/
 ├── Infrastructure/
 ├── API/
 └── Shared/
```