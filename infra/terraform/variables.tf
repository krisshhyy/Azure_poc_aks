variable "location" {
  description = "Azure region"
  type        = string
  default     = "eastus"
}

variable "environment" {
  description = "Environment name"
  type        = string
  default     = "poc"
}

variable "project_name" {
  description = "Project name prefix for all resources"
  type        = string
  default     = "myproject"
}

variable "aks_node_count" {
  description = "Number of AKS nodes (keep low for trial)"
  type        = number
  default     = 1
}

variable "aks_node_vm_size" {
  description = "VM size for AKS nodes — trial subscriptions are restricted to DC/EC/FX series"
  type        = string
  default     = "Standard_DC2s_v3"
}

variable "kubernetes_version" {
  description = "Kubernetes version — null lets AKS pick the latest supported version in the region"
  type        = string
  default     = null
}
