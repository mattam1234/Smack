# Security Considerations for Smack Plugin

## Overview

The Smack plugin enables browsing and streaming media from remote Jellyfin servers. This document outlines important security considerations when using this plugin.

## API Key Storage

### Current Implementation

- **Plain Text Storage**: API keys for remote servers are currently stored in plain text within the Jellyfin plugin configuration file.
- **Location**: The configuration is typically stored in Jellyfin's plugin configuration directory.
- **Access**: Anyone with access to the Jellyfin server's file system can read these API keys.

### Recommendations

1. **Use Dedicated API Keys**: Create a separate API key specifically for the Smack plugin on each remote server.
2. **Minimal Permissions**: Ensure the user account associated with the API key has only the minimum necessary permissions (e.g., read-only access to specific libraries).
3. **Avoid Admin Keys**: Never use an administrator API key with the Smack plugin.
4. **Regular Rotation**: Periodically rotate API keys, especially if you suspect unauthorized access.

## Network Security

### HTTPS Requirement

- **Production Use**: Always use HTTPS for both the local and remote Jellyfin servers in production environments.
- **Development/Testing**: HTTP may be acceptable in isolated test environments but should never be used in production.

### Network Exposure

- **Remote Server Access**: Ensure remote Jellyfin servers are properly secured and not unnecessarily exposed to the internet.
- **Firewall Rules**: Configure appropriate firewall rules to restrict access to remote servers.

## Configuration UI Security

### API Key Handling

The configuration UI implements several security measures:

1. **Masked Input**: The API key input field is of type `password`, preventing shoulder surfing.
2. **No Display**: API keys are never displayed in the server list - only a status of "configured" or "not set".
3. **Re-entry Required**: When editing a server, the API key is not pre-filled. You must re-enter it to change it.

### Input Validation

The configuration page validates:

- Server names (required, max 100 characters)
- Server URLs (must be valid absolute URLs with http:// or https://)
- API keys (required for new servers)

## XSS Protection

### HTML Rendering

- The plugin uses `textContent` and `createElement` methods instead of `innerHTML` to prevent XSS attacks.
- User-provided data (server names, URLs) is never directly injected into HTML.

## Data Flow

### Authentication

1. **API Key Transmission**: API keys are sent as query parameters in URLs when making requests to remote Jellyfin servers.
   - **Security Consideration**: Query parameters may be logged in server logs, browser history, and proxy logs.
   - **Mitigation**: This approach is required by the Jellyfin API. Always use HTTPS to encrypt the entire URL including query parameters.
   - **Best Practice**: Use dedicated API keys with minimal permissions to limit exposure if keys are logged.

2. The plugin does not implement any additional authentication layer.
3. All security depends on the remote server's API key validation.

### API Key Exposure Risks

When using API keys in query parameters:

- **Server Logs**: The API key will appear in remote server access logs
- **Browser History**: URLs with API keys may be stored in browser history (for stream URLs opened in new tabs)
- **Referrer Headers**: If the stream page navigates elsewhere, the API key might be sent in HTTP Referrer headers

**Mitigation Strategies**:

1. Use HTTPS exclusively to prevent network interception
2. Use API keys with read-only permissions
3. Regularly rotate API keys
4. Monitor remote server access logs for suspicious activity
5. Use private/incognito browsing when testing stream URLs directly

### Streaming

- Media streams are opened directly from the remote server using its download API.
- The local Jellyfin server does not proxy the media content.
- Users must have network access to the remote server to stream media.

## Threat Model

### Potential Threats

1. **API Key Compromise**: If an attacker gains access to the configuration file, they can access the remote servers.
2. **Man-in-the-Middle**: If HTTPS is not used, API keys and media content could be intercepted.
3. **Malicious Remote Server**: A compromised remote server could serve malicious content.

### Mitigations

1. **File System Security**: Secure the Jellyfin server's file system and limit access to configuration files.
2. **Use HTTPS**: Always use HTTPS for all connections in production.
3. **Trust Verification**: Only configure remote servers that you control or explicitly trust.
4. **Least Privilege**: Use API keys with minimal required permissions.

## Recommendations for Future Enhancements

1. **Encrypted Storage**: Consider implementing encrypted storage for API keys using Jellyfin's built-in secrets management (if available).
2. **OAuth/Token-Based Auth**: Explore OAuth or token-based authentication as an alternative to API keys.
3. **Certificate Validation**: Implement strict certificate validation for HTTPS connections.
4. **Audit Logging**: Add logging for all remote server access attempts and configuration changes.
5. **Connection Encryption**: Consider implementing end-to-end encryption for sensitive metadata.

## Responsible Disclosure

If you discover a security vulnerability in the Smack plugin, please report it responsibly:

1. Do not publicly disclose the vulnerability until a fix is available.
2. Contact the repository maintainers with details of the vulnerability.
3. Allow reasonable time for the issue to be addressed before public disclosure.

## Security Updates

Users should:

- Keep the plugin updated to the latest version.
- Monitor the repository for security advisories.
- Review API key permissions periodically.
- Audit remote server access logs regularly.
