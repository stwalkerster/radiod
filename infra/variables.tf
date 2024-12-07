variable "extra_source_ips" {
  type        = list(string)
  default     = []
  nullable    = false
  description = "Extra source IPs allowed to access bucket resources"
}