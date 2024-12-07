data "aws_caller_identity" "this" {
}

data "dns_a_record_set" "linode" {
  for_each = toset(["golbat"])

  host = "${each.value}.lon.stwalkerster.net"
}

data "dns_aaaa_record_set" "linode" {
  for_each = toset(["golbat"])

  host = "${each.value}.lon.stwalkerster.net"
}

locals {
  bucket_ip_policy = {
    Effect    = "Allow"
    Principal = "*"
    Action = [
      "s3:GetObject",
      "s3:ListBucket"
    ]
    Resource = [
      aws_s3_bucket.bucket.arn,
      "${aws_s3_bucket.bucket.arn}/*",
    ]
    Condition = {
      IpAddress = {
        "aws:SourceIp" = flatten([
          [for rrset in data.dns_a_record_set.linode : rrset.addrs],
          [for rrset in data.dns_aaaa_record_set.linode : rrset.addrs],
          var.extra_source_ips
        ])
      }
    }
  }
}
