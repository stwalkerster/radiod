variable "extra_source_ips" {
  type        = list(string)
  default     = []
  nullable    = false
  description = "Extra source IPs allowed to access bucket resources"
}

variable "terraform_deployment_role" {
  default = "arn:aws:iam::ACCOUNT:role/ROLE"
}

variable "role_trust_anchor" {
  default = "arn:aws:rolesanywhere:REGION:ACCOUNT:trust-anchor/ID"
}