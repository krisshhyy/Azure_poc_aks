output "resource_group_name" {
  value = azurerm_resource_group.main.name
}

output "acr_login_server" {
  description = "ACR login server URL — use this in pipeline variables"
  value       = azurerm_container_registry.main.login_server
}

output "acr_name" {
  value = azurerm_container_registry.main.name
}

output "aks_cluster_name" {
  value = azurerm_kubernetes_cluster.main.name
}

output "aks_kube_config_command" {
  description = "Run this to configure kubectl locally"
  value       = "az aks get-credentials --resource-group ${azurerm_resource_group.main.name} --name ${azurerm_kubernetes_cluster.main.name}"
}

output "devops_sp_client_id" {
  description = "Use as service connection client ID in Azure DevOps"
  value       = azuread_application.devops.client_id
}

output "devops_sp_client_secret" {
  description = "Use as service connection client secret in Azure DevOps"
  value       = azuread_service_principal_password.devops.value
  sensitive   = true
}
