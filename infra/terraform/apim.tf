# ─── APIM API Registrations ───────────────────────────────────────────────────
# All 5 APIs registered in APIM. Backends point to AKS ClusterIP services.
# In Consumption tier these are public — in real project they'd be private VNet URLs.

locals {
  # In real project: http://api1-svc.apis.svc.cluster.local
  # For POC with Consumption (no VNet): use the AKS public IP via ingress
  # We use a variable so it's easy to swap
  aks_backend_base = var.aks_backend_base_url
}

# ── Api1 — Products ────────────────────────────────────────────────────────────
resource "azurerm_api_management_api" "api1" {
  name                = "products-api"
  resource_group_name = azurerm_resource_group.main.name
  api_management_name = azurerm_api_management.main.name
  revision            = "1"
  display_name        = "Products API"
  path                = "products"
  protocols           = ["https"]
  subscription_required = false

  import {
    content_format = "openapi+json-link"
    content_value  = "https://petstore3.swagger.io/api/v3/openapi.json"  # placeholder, overridden by policy
  }
}

resource "azurerm_api_management_backend" "api1" {
  name                = "backend-api1"
  resource_group_name = azurerm_resource_group.main.name
  api_management_name = azurerm_api_management.main.name
  protocol            = "http"
  url                 = "${local.aks_backend_base}/api1"
}

resource "azurerm_api_management_api_policy" "api1" {
  api_name            = azurerm_api_management_api.api1.name
  api_management_name = azurerm_api_management.main.name
  resource_group_name = azurerm_resource_group.main.name

  xml_content = <<XML
<policies>
  <inbound>
    <base />
    <set-backend-service backend-id="backend-api1" />
    <rate-limit calls="100" renewal-period="60" />
  </inbound>
  <backend><base /></backend>
  <outbound><base /></outbound>
  <on-error><base /></on-error>
</policies>
XML
}

# ── Api2 — Orders ──────────────────────────────────────────────────────────────
resource "azurerm_api_management_api" "api2" {
  name                = "orders-api"
  resource_group_name = azurerm_resource_group.main.name
  api_management_name = azurerm_api_management.main.name
  revision            = "1"
  display_name        = "Orders API"
  path                = "orders"
  protocols           = ["https"]
  subscription_required = false
}

resource "azurerm_api_management_backend" "api2" {
  name                = "backend-api2"
  resource_group_name = azurerm_resource_group.main.name
  api_management_name = azurerm_api_management.main.name
  protocol            = "http"
  url                 = "${local.aks_backend_base}/api2"
}

resource "azurerm_api_management_api_policy" "api2" {
  api_name            = azurerm_api_management_api.api2.name
  api_management_name = azurerm_api_management.main.name
  resource_group_name = azurerm_resource_group.main.name

  xml_content = <<XML
<policies>
  <inbound>
    <base />
    <set-backend-service backend-id="backend-api2" />
    <rate-limit calls="100" renewal-period="60" />
  </inbound>
  <backend><base /></backend>
  <outbound><base /></outbound>
  <on-error><base /></on-error>
</policies>
XML
}

# ── Api3 — Notifications ───────────────────────────────────────────────────────
resource "azurerm_api_management_api" "api3" {
  name                = "notifications-api"
  resource_group_name = azurerm_resource_group.main.name
  api_management_name = azurerm_api_management.main.name
  revision            = "1"
  display_name        = "Notifications API"
  path                = "notifications"
  protocols           = ["https"]
  subscription_required = false
}

resource "azurerm_api_management_backend" "api3" {
  name                = "backend-api3"
  resource_group_name = azurerm_resource_group.main.name
  api_management_name = azurerm_api_management.main.name
  protocol            = "http"
  url                 = "${local.aks_backend_base}/api3"
}

resource "azurerm_api_management_api_policy" "api3" {
  api_name            = azurerm_api_management_api.api3.name
  api_management_name = azurerm_api_management.main.name
  resource_group_name = azurerm_resource_group.main.name

  xml_content = <<XML
<policies>
  <inbound>
    <base />
    <set-backend-service backend-id="backend-api3" />
    <rate-limit calls="100" renewal-period="60" />
  </inbound>
  <backend><base /></backend>
  <outbound><base /></outbound>
  <on-error><base /></on-error>
</policies>
XML
}

# ── Api4 — Users (calls JSONPlaceholder, transforms response) ──────────────────
resource "azurerm_api_management_api" "api4" {
  name                = "users-api"
  resource_group_name = azurerm_resource_group.main.name
  api_management_name = azurerm_api_management.main.name
  revision            = "1"
  display_name        = "Users API"
  path                = "users"
  protocols           = ["https"]
  subscription_required = false
}

resource "azurerm_api_management_backend" "api4" {
  name                = "backend-api4"
  resource_group_name = azurerm_resource_group.main.name
  api_management_name = azurerm_api_management.main.name
  protocol            = "http"
  url                 = "${local.aks_backend_base}/api4"
}

resource "azurerm_api_management_api_policy" "api4" {
  api_name            = azurerm_api_management_api.api4.name
  api_management_name = azurerm_api_management.main.name
  resource_group_name = azurerm_resource_group.main.name

  xml_content = <<XML
<policies>
  <inbound>
    <base />
    <set-backend-service backend-id="backend-api4" />
    <rate-limit calls="100" renewal-period="60" />
    <set-header name="X-Source" exists-action="override">
      <value>apim-gateway</value>
    </set-header>
  </inbound>
  <backend><base /></backend>
  <outbound>
    <base />
    <set-header name="X-Powered-By" exists-action="delete" />
  </outbound>
  <on-error><base /></on-error>
</policies>
XML
}

# ── Api5 — Posts (calls JSONPlaceholder, transforms response) ──────────────────
resource "azurerm_api_management_api" "api5" {
  name                = "posts-api"
  resource_group_name = azurerm_resource_group.main.name
  api_management_name = azurerm_api_management.main.name
  revision            = "1"
  display_name        = "Posts API"
  path                = "posts"
  protocols           = ["https"]
  subscription_required = false
}

resource "azurerm_api_management_backend" "api5" {
  name                = "backend-api5"
  resource_group_name = azurerm_resource_group.main.name
  api_management_name = azurerm_api_management.main.name
  protocol            = "http"
  url                 = "${local.aks_backend_base}/api5"
}

resource "azurerm_api_management_api_policy" "api5" {
  api_name            = azurerm_api_management_api.api5.name
  api_management_name = azurerm_api_management.main.name
  resource_group_name = azurerm_resource_group.main.name

  xml_content = <<XML
<policies>
  <inbound>
    <base />
    <set-backend-service backend-id="backend-api5" />
    <rate-limit calls="100" renewal-period="60" />
    <set-header name="X-Source" exists-action="override">
      <value>apim-gateway</value>
    </set-header>
  </inbound>
  <backend><base /></backend>
  <outbound>
    <base />
    <set-header name="X-Powered-By" exists-action="delete" />
  </outbound>
  <on-error><base /></on-error>
</policies>
XML
}
