# Smack Jellyfin Plugin

Smack is a Jellyfin plugin that lets one Jellyfin server browse and stream media from one or more remote Jellyfin servers.

> Status: **Experimental / WIP** – APIs and behavior may change.
> 
> **Note**: The plugin GUID `11111111-2222-3333-4444-555555555555` is intentionally simple for development. If you fork this project for production use, generate a new unique GUID to avoid conflicts.

## Features
- Configure one or more remote Jellyfin servers in the Jellyfin admin UI.
- Browse remote libraries, folders, and items from within the local Jellyfin UI.
- Open remote streams in a new browser tab using the remote server's download URL.

## How It Works
- You configure remote Jellyfin instances in the Smack plugin configuration page.
- Smack uses the remote server's public HTTP API and an API key you provide.
- A separate "Smack Browser" page inside Jellyfin lets you:
  - Select a configured remote server.
  - Select a library on that server.
  - Navigate folders within that library.
  - Open streams for playable items.

Smack does **not** import or permanently sync remote items into your local Jellyfin library; it simply browses and streams them on demand.

## Setup

### Requirements
- Jellyfin Server compatible with this plugin template (same Jellyfin version as in `Jellyfin.Plugin.Template` references).
- .NET 9 SDK for building from source.

### Build

From the solution root:

```bash
# Build the plugin
 dotnet build
```

### Install in Jellyfin
1. Build the project in Release.
2. Locate the compiled plugin output under `Jellyfin.Plugin.Template/bin/Release`.
3. Copy the plugin folder into your Jellyfin `plugins` directory (or a dev plugins path).
4. Restart Jellyfin.
5. In the Jellyfin dashboard, go to `Plugins` → `Smack` to configure remote servers.

### Configure Remote Servers
1. Open the Smack plugin configuration page.
2. Use the **Add / Edit Remote Server** section:
   - `Name`: A friendly name for the remote server.
   - `Server URL`: Base URL of the remote Jellyfin server (e.g. `https://remote.example.com/jellyfin`).
   - `API Key`: An API key for an account on the remote server with access to the media you want to stream.
3. Click `Save Server`.
4. Existing servers can be edited (name/URL, optionally API key) or deleted from the list.

### Browse and Stream
1. Open the **SmackBrowser** page from the plugin pages.
2. Select a remote server.
3. Select a library.
4. Use the folder view to navigate:
   - `Open` on folders to drill down.
   - `Up` to go back up one level.
   - A breadcrumb path shows your current location.
5. Click `Open stream` on a non-folder item to open the remote stream in a new tab.

## Security Notes

- The remote API key is stored in plain text in the Jellyfin configuration for this plugin.
  - Do not use Smack with untrusted or sensitive remote servers.
  - Use a restricted user account on the remote Jellyfin if possible.
- The Smack configuration UI:
  - Masks the API key input field.
  - Shows only whether a key is `configured` or `not set` in the server list.
  - Does not pre-fill the key when editing; you need to re-enter it to change.
- Remote streams are opened directly from the remote Jellyfin server using its download API.
  - Ensure your Jellyfin server and remote servers are using HTTPS for production use.

## Roadmap
See `docs/PLAN.md` for the detailed implementation plan and remaining optional enhancements.

## Development

- Target framework: .NET 9
- Coding style: matches Jellyfin plugin template conventions.

## License
This project follows the same license as the original Jellyfin plugin template (see the repository root for details).
