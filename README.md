# AKS + ACR + Azure DevOps POC

Replicates a real project setup where:
- External team owns AKS infrastructure (provisioned here via Terraform)
- Your team builds .NET APIs, pushes to ACR, and deploys to AKS via Azure DevOps pipelines

## Project Structure

```
infra/
  terraform/       # All Azure resources (RG, VNet, ACR, AKS, SP, APIM)
  k8s/             # Kubernetes manifests per API
pipelines/
  templates/       # Reusable build-push-deploy template
  api1-pipeline.yml
  api2-pipeline.yml
  api3-pipeline.yml
  api4-pipeline.yml
  api5-pipeline.yml
src/
  Api1/            # Products service      (GET /products)
  Api2/            # Orders service        (GET /orders, POST /orders)
  Api3/            # Notifications service (GET /notifications)
  Api4/            # Users service         (fetches JSONPlaceholder /users, transforms)
  Api5/            # Posts service         (fetches JSONPlaceholder /posts, transforms)
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
For each API, go to **Pipelines → New pipeline → GitHub → select your repo → Existing Azure Pipelines YAML file**:
- `pipelines/api1-pipeline.yml`
- `pipelines/api2-pipeline.yml`
- `pipelines/api3-pipeline.yml`

> First time you connect GitHub, Azure DevOps will ask you to authorize via OAuth or install the Azure Pipelines GitHub App — use the GitHub App option, it's more reliable.

---

## Step 3 — Apply K8s Namespace

```bash
az aks get-credentials --resource-group rg-myproject-poc --name aks-myproject-poc
kubectl apply -f infra/k8s/namespace.yaml
```

---

## Step 4 — Wire APIM backends to AKS (Consumption tier workaround)

Since Consumption tier has no VNet, APIM can't reach ClusterIP services directly.
You need to expose the APIs via a LoadBalancer and update the backend URL in Terraform.

### Option A — Single Nginx Ingress (recommended)

```bash
# Install nginx ingress controller
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.10.0/deploy/static/provider/cloud/deploy.yaml

# Wait for external IP
kubectl get svc -n ingress-nginx ingress-controller -w
```

Create `infra/k8s/ingress.yaml`:
```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: apis-ingress
  namespace: apis
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /$2
spec:
  ingressClassName: nginx
  rules:
    - http:
        paths:
          - path: /api1(/|$)(.*)
            pathType: Prefix
            backend:
              service: { name: api1-svc, port: { number: 80 } }
          - path: /api2(/|$)(.*)
            pathType: Prefix
            backend:
              service: { name: api2-svc, port: { number: 80 } }
          - path: /api3(/|$)(.*)
            pathType: Prefix
            backend:
              service: { name: api3-svc, port: { number: 80 } }
          - path: /api4(/|$)(.*)
            pathType: Prefix
            backend:
              service: { name: api4-svc, port: { number: 80 } }
          - path: /api5(/|$)(.*)
            pathType: Prefix
            backend:
              service: { name: api5-svc, port: { number: 80 } }
```

```bash
kubectl apply -f infra/k8s/ingress.yaml

# Get the public IP
kubectl get ingress -n apis
```

### Update Terraform with the ingress IP

```hcl
# terraform.tfvars
aks_backend_base_url = "http://<INGRESS_PUBLIC_IP>"
```

```bash
terraform apply
```

APIM will now route to `http://<IP>/api1`, `http://<IP>/api2` etc.

---

## Step 5 — Create Pipelines for Api4 and Api5

Same as before in Azure DevOps:
- `pipelines/api4-pipeline.yml`
- `pipelines/api5-pipeline.yml`

---

## API Endpoints via APIM Gateway

After `terraform output apim_gateway_url`:

```bash
APIM=https://apim-myproject-poc.azure-api.net

# Products
curl $APIM/products/products

# Orders
curl $APIM/orders/orders

# Notifications
curl $APIM/notifications/notifications

# Users (transformed from JSONPlaceholder)
curl $APIM/users/users
curl $APIM/users/users/1

# Posts (transformed from JSONPlaceholder)
curl $APIM/posts/posts
curl $APIM/posts/posts/1
curl $APIM/posts/posts/1/comments
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
