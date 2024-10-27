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
