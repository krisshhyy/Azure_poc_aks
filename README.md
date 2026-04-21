# AKS + ACR + Azure DevOps POC

Replicates a real project setup where:
- External team owns AKS infrastructure (provisioned here via Terraform)
- Your team builds .NET APIs, pushes to ACR, and deploys to AKS via Azure DevOps pipelines

## Project Structure

```
infra/
  terraform/       # All Azure resources (RG, VNet, ACR, AKS, SP)
  k8s/             # Kubernetes manifests per API
pipelines/
  templates/       # Reusable build-push-deploy template
  api1-pipeline.yml
  api2-pipeline.yml
  api3-pipeline.yml
src/
  Api1/            # Products service  (GET /products)
  Api2/            # Orders service    (GET /orders, POST /orders)
  Api3/            # Notifications svc (GET /notifications, POST /notifications/send)
```

---

## Step 1 — Provision Azure Resources

### Prerequisites
- Azure CLI logged in: `az login`
- Terraform >= 1.5 installed

```bash
cd infra/terraform
terraform init
terraform plan
terraform apply
```

After apply, note the outputs:
```bash
terraform output acr_login_server        # set as ACR_LOGIN_SERVER in DevOps
terraform output aks_cluster_name
terraform output devops_sp_client_id
terraform output -raw devops_sp_client_secret   # store securely
```

---

## Step 2 — Configure Azure DevOps

### 2a. Create a Variable Group named `poc-common-vars`
Go to **Pipelines → Library → + Variable group** and add:

| Variable | Example Value |
|---|---|
| `ACR_LOGIN_SERVER` | `acrmyprojectpoc001.azurecr.io` |
| `AZURE_CONTAINER_REGISTRY_SERVICE_CONNECTION` | `sc-acr-poc` |
| `AZURE_SERVICE_CONNECTION` | `sc-arm-poc` |
| `AKS_RESOURCE_GROUP` | `rg-myproject-poc` |
| `AKS_CLUSTER_NAME` | `aks-myproject-poc` |
| `AKS_ENVIRONMENT_NAME` | `aks-poc` |

### 2b. Create Service Connections
Go to **Project Settings → Service connections**:

1. **Docker Registry** service connection → Azure Container Registry
   - Name it `sc-acr-poc`
   - Use the SP client ID/secret from Terraform outputs

2. **Azure Resource Manager** service connection
   - Name it `sc-arm-poc`
   - Same SP credentials, scoped to your subscription

### 2c. Create an Environment
Go to **Pipelines → Environments → New environment**
- Name: `aks-poc`
- Resource: Kubernetes → select your AKS cluster → namespace `apis`

### 2d. Create Pipelines
For each API, go to **Pipelines → New pipeline → Azure Repos Git → Existing YAML**:
- `pipelines/api1-pipeline.yml`
- `pipelines/api2-pipeline.yml`
- `pipelines/api3-pipeline.yml`

---

## Step 3 — Apply K8s Namespace

```bash
az aks get-credentials --resource-group rg-myproject-poc --name aks-myproject-poc
kubectl apply -f infra/k8s/namespace.yaml
```

---

## How It Works

```
git push to main
      │
      ▼
Azure DevOps Pipeline (per API)
      │
      ├─ Stage 1: docker build → push to ACR (tagged with BuildId)
      │
      └─ Stage 2: kubectl apply → AKS (image substituted in manifest)
```

Each pipeline only triggers on changes to its own `src/ApiN/**` path, so teams can work independently.

---

## Cost Notes (Trial Subscription)
- AKS: 1x `Standard_B2s` node (~$30/month) — no charge for control plane
- ACR: Basic SKU (~$5/month)
- Total: ~$35/month, well within 30-day trial credits
