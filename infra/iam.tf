resource "aws_iam_policy" "radiod" {
  name = "radiod-bot"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect   = "Allow"
        Resource = ["*"]
        Action = [
          "polly:StartSpeechSynthesisTask",
          "polly:GetSpeechSynthesisTask",
        ]
      },

      {
        Effect   = "Allow"
        Resource = ["${aws_s3_bucket.bucket.arn}/*"]
        Action = [
          "s3:PutObject",
        ]
      }
    ]
  })
}

resource "aws_iam_role" "radiod" {
  name = "radiod-bot"

  assume_role_policy = jsonencode({
    Statement = [
      {
        Action = [
          "sts:AssumeRole",
          "sts:TagSession",
          "sts:SetSourceIdentity"
        ]
        Effect = "Allow"
        Principal = {
          Service = "rolesanywhere.amazonaws.com"
        }
        Condition = {
          StringEquals = {
            "aws:PrincipalTag/x509Subject/CN" : "radiod-bot"
          }
        }
      }
    ]
    Version = "2012-10-17"
  })
}

resource "aws_iam_role_policy_attachment" "radiod" {
  policy_arn = aws_iam_policy.radiod.arn
  role       = aws_iam_role.radiod.name
}

resource "aws_rolesanywhere_profile" "role" {
  name = aws_iam_role.radiod.name

  role_arns = [aws_iam_role.radiod.arn]
  enabled   = true
}

# Legacy IAM user

resource "aws_iam_user" "radiod" {
  name = "radiod-bot"
}

resource "aws_iam_user_policy_attachment" "radiod" {
  policy_arn = aws_iam_policy.radiod.arn
  user       = aws_iam_user.radiod.name
}

resource "aws_iam_access_key" "radiod" {
  user = aws_iam_user.radiod.name
}
