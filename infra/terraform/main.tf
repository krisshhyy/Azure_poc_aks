locals {
  prefix = "${var.project_name}-${var.environment}"
  tags = {
    environment = var.environment
    project     = var.project_name
    managed_by  = "terraform"
  }
}

# ─── Resource Group ───────────────────────────────────────────────────────────
resource "azurerm_resource_group" "main" {
  name     = "rg-${local.prefix}"
  location = var.location
  tags     = local.tags
}

# ─── Networking ───────────────────────────────────────────────────────────────
resource "azurerm_virtual_network" "main" {
  name                = "vnet-${local.prefix}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  address_space       = ["10.0.0.0/16"]
  tags                = local.tags
}

resource "azurerm_subnet" "aks" {
  name                 = "snet-aks"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.0.1.0/24"]
}

# ─── Azure Container Registry ─────────────────────────────────────────────────
resource "azurerm_container_registry" "main" {
  name                = "acr${replace(local.prefix, "-", "")}001"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = "Basic"   # cheapest tier, fine for POC
  admin_enabled       = false
  tags                = local.tags
}

# ─── AKS Cluster ──────────────────────────────────────────────────────────────
resource "azurerm_kubernetes_cluster" "main" {
  name                = "aks-${local.prefix}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  dns_prefix          = "aks-${local.prefix}"
  kubernetes_version  = var.kubernetes_version

  # Single system node pool — minimum viable for trial subscription
  default_node_pool {
    name                = "system"
    node_count          = var.aks_node_count
    vm_size             = var.aks_node_vm_size
    vnet_subnet_id      = azurerm_subnet.aks.id
    os_disk_size_gb     = 30
    type                = "VirtualMachineScaleSets"

    upgrade_settings {
      max_surge = "10%"
    }
  }

  # System-assigned identity so AKS can pull from ACR
  identity {
    type = "SystemAssigned"
  }

  network_profile {
    network_plugin    = "azure"
    load_balancer_sku = "standard"
    service_cidr      = "10.1.0.0/16"
    dns_service_ip    = "10.1.0.10"
  }

  # Disable expensive add-ons for trial
  http_application_routing_enabled = false
  oidc_issuer_enabled              = true

  tags = local.tags
}

# ─── Azure API Management (Consumption tier) ──────────────────────────────────
resource "azurerm_api_management" "main" {
  name                = "apim-${local.prefix}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  publisher_name      = "MyOrganization"
  publisher_email     = "admin@myorg.com"
  sku_name            = "Consumption_0"  # Consumption tier - pay per use

  tags = local.tags
}

# ─── Grant AKS pull access to ACR ─────────────────────────────────────────────
resource "azurerm_role_assignment" "aks_acr_pull" {
  principal_id                     = azurerm_kubernetes_cluster.main.kubelet_identity[0].object_id
  role_definition_name             = "AcrPull"
  scope                            = azurerm_container_registry.main.id
  skip_service_principal_aad_check = true
}

# ─── Grant DevOps SP access to AKS ───────────────────────────────────────────
resource "azurerm_role_assignment" "devops_aks_user" {
  principal_id         = azuread_service_principal.devops.object_id
  role_definition_name = "Azure Kubernetes Service Cluster User Role"
  scope                = azurerm_kubernetes_cluster.main.id
}

# ─── Azure DevOps Service Connection SP ───────────────────────────────────────
# This SP is used by Azure DevOps pipelines to push images to ACR
resource "azurerm_role_assignment" "devops_acr_push" {
  principal_id         = azuread_service_principal.devops.object_id
  role_definition_name = "AcrPush"
  scope                = azurerm_container_registry.main.id
}

resource "azuread_application" "devops" {
  display_name = "sp-${local.prefix}-devops"
}

resource "azuread_service_principal" "devops" {
  client_id = azuread_application.devops.client_id
}

resource "azuread_service_principal_password" "devops" {
  service_principal_id = azuread_service_principal.devops.id
}
