resource "vault_policy" "bot" {
  name = "kube-radio-bot"

  policy = <<-EOT
    path "pki_iam/sign/kube-radio-bot" {
      capabilities = ["create", "update"]
    }
  EOT
}

resource "vault_kubernetes_auth_backend_role" "bot" {
  backend   = "kube-linode"
  role_name = "kube-radio-bot"

  bound_service_account_names      = ["vault-auth"]
  bound_service_account_namespaces = ["radio"]

  token_policies = [vault_policy.bot.name]
  token_ttl      = 15 * 60
}

resource "vault_pki_secret_backend_role" "bot" {
  backend = "pki_iam"
  name    = "kube-radio-bot"

  ttl     = 7 * 24 * 60 * 60
  max_ttl = 7 * 24 * 60 * 60

  allowed_domains = ["bot.radio.k8s.stwalkerster.net"]

  allow_bare_domains          = true
  allow_subdomains            = false
  allow_glob_domains          = false
  allow_localhost             = false
  allow_ip_sans               = false
  allow_wildcard_certificates = false

  enforce_hostnames = true
  cn_validations    = ["hostname"]

  client_flag = true
  server_flag = false
  key_type    = "ec"
  key_bits    = 256

  no_store = false
}
