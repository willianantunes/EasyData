# Sample Project SSO

Demonstrates SAML SSO integration with the EasyData Admin Dashboard using AWS IAM Identity Center as the IdP.

## Running

```bash
docker compose up -d db
cd sample-project-sso/src && dotnet run -- api
# Dashboard at http://localhost:8000/admin/
```

## SAML Configuration

The app reads SAML settings from `appsettings.json`:

| Setting | Description |
|---------|-------------|
| `Saml:MetadataUrl` | IdP metadata URL — certificate and SSO URL are auto-extracted at startup |
| `Saml:Issuer` | SP entity ID / SAML audience — must match "Application SAML audience" in AWS |
| `Saml:AcsUrl` | ACS callback URL — must match "Application ACS URL" in AWS |

`SamlGroupsAttribute` is set to `http://schemas.xmlsoap.org/claims/Group` in code, which is the attribute name AWS IAM Identity Center uses to send group memberships in the SAML response.

## Group-Based Permissions

1. In AWS IAM Identity Center, assign groups to the application
2. In the admin dashboard, create an `Auth Group` whose `name` matches the AWS group UUID (e.g., `24588478-d081-707b-76e6-f055985913b3`)
3. Assign permissions to that group via `Auth Group Permissions`
4. On each SSO login, user group memberships are fully replaced with the current SAML response groups

## Known Issues

### SP-Initiated Login Does Not Work with AWS IAM Identity Center

**IdP-initiated login works correctly.** Users can log in by clicking "Identity - DEV" from the AWS access portal (`https://<directory>.awsapps.com/start/#/?tab=applications`).

**SP-initiated login fails.** Clicking "Try single sign-on (SSO)" on the login page redirects to AWS, but AWS returns a 403 "No access" error on its internal `/saml/v2/assertion/{relayId}` endpoint.

**What was investigated:**

- The SAML AuthnRequest XML is valid — correct Issuer, ACS URL, ProtocolBinding, and NameIDPolicy
- The `Issuer` matches the "Application SAML audience" configured in AWS (`http://localhost:8000/admin`)
- The `AssertionConsumerServiceURL` matches the "Application ACS URL" (`http://localhost:8000/api/security/saml/callback`)
- Both HTTP-Redirect and HTTP-POST bindings were tested — same result
- Setting "Application start URL" in AWS did not resolve the issue
- The user IS assigned to the application (confirmed by IdP-initiated working)

**Root cause:** Unknown. AWS IAM Identity Center accepts the AuthnRequest (creates a relayId and redirects to the portal), but the portal fails when attempting to generate the assertion. This appears to be an AWS-side issue with custom SAML 2.0 applications and SP-initiated flows.

**Workaround:** Use IdP-initiated login from the AWS access portal.
