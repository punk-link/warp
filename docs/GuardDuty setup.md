# Amazon GuardDuty Malware Protection Setup

This document describes the AWS infrastructure configuration required to enable GuardDuty Malware Protection for images uploaded to the Warp S3 bucket.

## Overview

When a creator uploads images, Warp polls the `GuardDutyMalwareScanStatus` S3 object tag that GuardDuty writes after scanning. Images flagged as malicious are excluded from the entry and deleted from S3.

## Prerequisites

- AWS account with GuardDuty enabled in the target region
- S3 bucket used for image uploads (referenced in `ImageCacheOptions.BucketName`)
- IAM role used by the Warp application (ECS task role or EC2 instance profile)

## Step 1 — Enable GuardDuty

If GuardDuty is not yet enabled in the account:

1. Open the [GuardDuty console](https://console.aws.amazon.com/guardduty/).
2. Choose **Get started** and then **Enable GuardDuty**.

## Step 2 — Enable Malware Protection for S3

1. In the GuardDuty console, go to **Protection plans → Malware Protection for S3**.
2. Choose **Enable**.
3. Select the S3 bucket used for image uploads.
4. Under **Object prefixes**, leave blank to scan all objects (or restrict to the image prefix if needed).
5. Enable **Tag on scan completion** — this writes the `GuardDutyMalwareScanStatus` tag that Warp polls.
6. Choose or create a GuardDuty service role that allows it to read objects and write tags.

## Step 3 — IAM Permissions for the Application

The Warp application role needs to **read** S3 object tags:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": "s3:GetObjectTagging",
      "Resource": "arn:aws:s3:::<bucket-name>/*"
    }
  ]
}
```

Replace `<bucket-name>` with the value of `ImageCacheOptions.BucketName`.

> The application already has `s3:PutObject` and `s3:DeleteObject` permissions for the image bucket. Only `s3:GetObjectTagging` needs to be added if it is not already present.

## Step 4 — S3 Bucket Policy for GuardDuty

GuardDuty's service role needs to read uploaded objects and write scan result tags. Add the following statement to the bucket policy:

```json
{
  "Effect": "Allow",
  "Principal": {
    "Service": "malware-protection-plan.guardduty.amazonaws.com"
  },
  "Action": [
    "s3:GetObject",
    "s3:GetObjectVersion",
    "s3:PutObjectTagging"
  ],
  "Resource": "arn:aws:s3:::<bucket-name>/*",
  "Condition": {
    "StringEquals": {
      "aws:SourceAccount": "<account-id>"
    }
  }
}
```

Replace `<bucket-name>` and `<account-id>` with the appropriate values.

## GuardDuty Tag Values

| Tag value         | Warp interpretation |
|-------------------|---------------------|
| `NO_THREATS_FOUND` | Clean — included in entry |
| `THREATS_FOUND`    | Malicious — excluded and deleted |
| `UNSUPPORTED`      | Unsupported file type — treated as clean |
| `FAILED`           | Scan error — treated as clean, warning logged |
| *(absent)*         | Scan pending — treated as clean until timeout |

## Application Configuration

| Setting | Description | Default |
|---------|-------------|---------|
| `MalwareScanOptions:ScanTimeoutMs` | Total time (ms) to wait for all image scans to complete. Set to `0` to disable scanning entirely (used in E2E environments). | `10000` |
| `MalwareScanOptions:PollingIntervalMs` | Interval (ms) between tag polling attempts per image. | `500` |

All images in an upload are scanned concurrently. The timeout applies to the entire batch, not per image.

## Verification

After setup, upload a test EICAR file (standard malware test string) to the bucket and verify:

1. GuardDuty detects the threat and writes `GuardDutyMalwareScanStatus=THREATS_FOUND` to the object tags.
2. The Warp application excludes the image and returns it in the `excludedImages` field of the entry creation response.
3. The creator sees a warning toast notification.
4. The `malicious_images_detected_total` and `entries_with_malicious_images_total` metrics are incremented.
