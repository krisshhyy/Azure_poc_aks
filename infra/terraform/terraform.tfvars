# Customize these before running terraform apply
location         = "eastus"
environment      = "poc"
project_name     = "myproject"   # change to your project name (no spaces)
aks_node_count   = 1             # keep at 1 to save trial credits
aks_node_vm_size = "Standard_DC2s_v3"
# kubernetes_version = "1.30"  # leave commented to auto-select latest supported in your region
