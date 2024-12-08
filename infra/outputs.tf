output "profile_arn" {
  value = aws_rolesanywhere_profile.role.arn
}

output "role_arn" {
  value = aws_iam_role.radiod.arn
}