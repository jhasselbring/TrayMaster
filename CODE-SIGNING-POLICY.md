# Code Signing Policy

**Project**: Runner - System Tray Process Manager
**Repository**: https://github.com/jhasselbring/runner
**License**: MIT License (OSI-approved)
**Last Updated**: 2026-01-10

## Purpose

This document outlines the code signing policy for the Runner project as required by the SignPath Foundation for open source code signing certificates.

## Project Overview

Runner is a lightweight Windows system tray application for managing long-running processes with JSON configuration and HTTP webhook support. The project is:

- **Open Source**: Licensed under the MIT License
- **Non-Commercial**: Free to use, modify, and distribute
- **Actively Maintained**: Regular updates and community support
- **Secure**: No malware, no data collection, no privacy violations

## Code Signing Authority

Runner uses code signing certificates provided by the **SignPath Foundation** for open source projects. All binaries are signed to:

- Verify publisher authenticity
- Ensure binary integrity (no tampering)
- Eliminate Windows SmartScreen warnings
- Build user trust

**Certificate Issuer**: SignPath Foundation
**Signing Service**: https://signpath.io

## Team Roles and Responsibilities

As required by SignPath Foundation, the project maintains clear role separation:

### Authors
**Role**: Write and modify code
**Members**: Project maintainers with repository write access
**Requirements**:
- Multi-factor authentication (MFA) enabled on GitHub account
- Code changes submitted via pull requests for review

### Reviewers
**Role**: Review and approve code changes
**Members**: Project maintainers
**Requirements**:
- MFA enabled on GitHub account
- Review all pull requests before merging
- Verify code quality and security

### Approvers
**Role**: Authorize code signing for releases
**Members**: Project owner (@jhasselbring)
**Requirements**:
- MFA enabled on GitHub and SignPath accounts
- Manual approval required for all signing requests
- Verify release integrity before signing

## Signing Process

### Build and Sign Workflow

1. **Tag Creation**: Release tags (e.g., `v1.0.0`) trigger the automated build
2. **Automated Build**: GitHub Actions builds the executable from source
3. **Signing Request**: Build artifacts are submitted to SignPath
4. **Manual Approval**: Project Approver manually reviews and approves signing request
5. **Certificate Application**: SignPath Foundation certificate is applied
6. **Release Publication**: Signed binaries are published to GitHub Releases

### Requirements

All signed binaries must:
- Be built exclusively from source code in the public GitHub repository
- Include enforced metadata (product name and version)
- Pass all automated tests
- Receive manual approval from designated Approvers
- Not include any proprietary or third-party code

### What We Sign

- `Runner.exe` - Main application executable (Windows x64)

### What We Don't Sign

- Development/debug builds
- Pre-release/beta versions (unless explicitly tagged)
- Third-party libraries or dependencies
- User-generated content or configurations

## Security Commitments

### Code Integrity

- All code is publicly available in the GitHub repository
- No proprietary code from maintainers or affiliates
- All dependencies are from trusted, verified sources
- Regular security reviews of dependencies

### User Privacy

Runner does NOT:
- Collect user data
- Phone home or send telemetry
- Include tracking or analytics
- Access user files without explicit configuration
- Make network requests except for user-configured HTTP webhooks

Runner DOES:
- Run entirely locally on the user's machine
- Only execute commands configured by the user
- Store configuration locally in `runner.json`
- Respect user privacy and data sovereignty

### Prohibited Uses

Runner is NOT designed for:
- Hacking or unauthorized system access
- Exploiting security vulnerabilities
- Circumventing security measures
- Privacy violations
- Malicious activities

## Violation Response

If security issues or policy violations are reported:

1. **Immediate Investigation**: Team investigates within 24 hours
2. **Issue Confirmation**: Verify the reported issue
3. **Remediation**: Fix the issue in the codebase
4. **Release Update**: Issue patched version immediately
5. **User Notification**: Announce the issue and fix in release notes

Report security issues to: https://github.com/jhasselbring/runner/issues

## Multi-Factor Authentication (MFA)

All team members with code access or signing authority MUST:
- Enable MFA on their GitHub account
- Enable MFA on their SignPath account (if applicable)
- Use strong, unique passwords
- Maintain secure development environments

## Compliance

This project complies with:
- SignPath Foundation's Terms of Service
- OSI-approved MIT License terms
- GitHub's Terms of Service and Community Guidelines
- Industry-standard security practices

## Metadata Enforcement

All signed binaries include enforced metadata:
- **Product Name**: Runner Process Manager
- **Version**: Matches git tag (e.g., 1.0.0)
- **Company**: Open Source
- **Copyright**: MIT License
- **File Description**: System Tray Process Manager

## Changes to This Policy

This policy may be updated to:
- Reflect changes in SignPath Foundation requirements
- Improve security practices
- Clarify existing policies

All changes will be:
- Committed to the repository with clear commit messages
- Announced in release notes
- Effective immediately upon commit

## Attribution

Code signing services provided by the **SignPath Foundation**.

Learn more about SignPath Foundation's free code signing for open source:
https://signpath.io

## Contact

For questions about this code signing policy:
- Open an issue: https://github.com/jhasselbring/runner/issues
- Project maintainer: @jhasselbring

---

**Disclaimer**: SignPath Foundation cannot accept any liability for damages resulting from software signed with their certificates. Users install and use Runner at their own risk. See LICENSE for full terms.
