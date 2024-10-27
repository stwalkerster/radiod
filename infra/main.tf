terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }

    dns = {
      source = "hashicorp/dns"
      version = "~> 3.0"
    }
  }

  backend "s3" {
    bucket = "stwalkerster-terraform-state"
    key    = "state/Production/radiod/terraform.tfstate"
    region = "eu-west-1"

    dynamodb_table = "terraform-state-lock"

    assume_role = {
      role_arn = "arn:aws:iam::XXXXXXXXX:role/TerraformState"
    }
  }

  required_version = "~> 1.8"
}

provider "aws" {
  region = "eu-west-1"

  assume_role {
    role_arn = "arn:aws:iam::XXXXXXXXX:role/Terraform"
  }

  default_tags {
    tags = {
      "Terraform"   = "yes"
      "Project"     = "radiod"
      "Environment" = "Production"
    }
  }
}
