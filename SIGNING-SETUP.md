# Code Signing Setup with SignPath.io

This guide will help you set up free code signing for Runner using SignPath.io's open source program.

## Why Code Signing?

Code signing eliminates Windows SmartScreen warnings and builds user trust by:
- ✅ Showing your verified publisher name
- ✅ Proving the executable hasn't been tampered with
- ✅ No "Unknown publisher" warnings

## Step 1: Apply for SignPath.io Open Source Program

1. Go to https://signpath.io
2. Click **"Open Source"** or **"Get Started"**
3. Select **"Open Source Projects"**
4. Fill out the application form:
   - **Project Name**: Runner
   - **Repository URL**: https://github.com/jhasselbring/runner
   - **License**: MIT (or your license)
   - **Project Description**: System Tray Process Manager for Windows
   - **Why you need code signing**: To eliminate SmartScreen warnings for users

5. Submit the application and wait for approval (usually 1-3 business days)

## Step 2: Set Up SignPath Project

Once approved, you'll receive access to the SignPath dashboard:

1. **Create a New Project**:
   - Project Name: `runner`
   - Project Slug: `runner` (use lowercase)

2. **Upload Configuration**:
   - Upload the `SignPath.json` file from your repository
   - This tells SignPath what files to sign

3. **Create a Signing Policy**:
   - Name: `release-signing`
   - Type: **Release Signing**
   - Enable **GitHub integration**

4. **Get Your Credentials**:
   - Navigate to **Settings** → **API Tokens**
   - Create a new API token
   - Copy your **Organization ID** and **API Token**

## Step 3: Configure GitHub Secrets

Add the following secrets to your GitHub repository:

1. Go to your repository: https://github.com/jhasselbring/runner
2. Click **Settings** → **Secrets and variables** → **Actions**
3. Click **"New repository secret"** and add:

   - **Name**: `SIGNPATH_API_TOKEN`
     **Value**: [Your API token from SignPath]

   - **Name**: `SIGNPATH_ORGANIZATION_ID`
     **Value**: [Your organization ID from SignPath]

## Step 4: Test the Workflow

Once everything is configured:

1. Create a new version tag:
   ```bash
   git tag -a v1.0.1 -m "Test signed release"
   git push origin v1.0.1
   ```

2. The GitHub Action will automatically:
   - Build the release
   - Submit to SignPath for signing
   - Wait for signing to complete
   - Create a GitHub release with signed binaries

3. Monitor the workflow at: https://github.com/jhasselbring/runner/actions

## Step 5: Verify the Signature

After the release is created:

1. Download `Runner-v1.0.1-win64.exe`
2. Right-click → **Properties** → **Digital Signatures** tab
3. You should see a valid signature

Alternatively, verify via PowerShell:
```powershell
Get-AuthenticodeSignature .\Runner-v1.0.1-win64.exe
```

Expected output:
```
SignerCertificate      : [Your certificate info]
TimeStamperCertificate : [Timestamp info]
Status                 : Valid
```

## Troubleshooting

### Workflow Fails at Signing Step

- **Check API token**: Make sure `SIGNPATH_API_TOKEN` is correct
- **Check organization ID**: Verify `SIGNPATH_ORGANIZATION_ID` matches your account
- **Check project slug**: Must match the slug in SignPath (usually lowercase)
- **Check signing policy**: Must be named `release-signing` or update the workflow

### SignPath Application Rejected

SignPath requires:
- Active, maintained repository
- Clear open source license
- Real project (not a test/demo)
- Public repository

If rejected, consider:
- Self-signed certificate (free but shows warnings)
- Standard code signing certificate ($100-200/year)

## Alternative: Self-Signed Certificate (No SignPath)

If you want to sign locally without SignPath:

1. **Create certificate**:
   ```powershell
   $cert = New-SelfSignedCertificate `
     -Type CodeSigningCert `
     -Subject "CN=Your Name, O=Your Organization, C=US" `
     -CertStoreLocation Cert:\CurrentUser\My `
     -NotAfter (Get-Date).AddYears(3)
   ```

2. **Export certificate** (to sign on different machines):
   ```powershell
   $password = ConvertTo-SecureString -String "YourPassword" -Force -AsPlainText
   Export-PfxCertificate -Cert $cert -FilePath "CodeSigning.pfx" -Password $password
   ```

3. **Sign the executable**:
   ```powershell
   & "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe" sign `
     /fd SHA256 `
     /a `
     /t http://timestamp.digicert.com `
     Runner.exe
   ```

**Note**: Self-signed certificates still show SmartScreen warnings until you build reputation.

## Next Steps

Once code signing is working:

1. All future releases via tags will be automatically signed
2. Users will see your verified publisher name
3. No more SmartScreen warnings
4. Builds trust and professionalism

## Support

- SignPath Documentation: https://signpath.io/documentation
- SignPath Support: support@signpath.io
- GitHub Actions: https://docs.github.com/actions

---

Created for Runner - System Tray Process Manager
