# Smack Plugin Plan

## Goal
Use a local Jellyfin instance to browse and stream media from one or more remote Jellyfin servers, via a Jellyfin plugin called `Smack`.

## MVP Scope
- Configure one or more remote Jellyfin servers in the plugin settings.
- Authenticate to each remote server using an API key.
- Expose a simple web UI page (inside Jellyfin) to:
  - List configured remote servers.
  - Browse libraries and items on a selected remote server.
  - Start playback of a selected remote item.

## Architecture
- `Plugin` (main plugin class)
  - Provides metadata (Id, Name, Description).
  - Implements `IHasWebPages` to register configuration and browser pages.
  - Uses `SmackPluginConfiguration` for persistent settings.

- `SmackPluginConfiguration`
  - Root config model saved by Jellyfin.
  - Contains a collection of `SmackRemoteServer` entries.

- `SmackRemoteServer`
  - `Id` (local GUID string).
  - `Name` (friendly name for admin).
  - `ServerUrl` (base URL of remote Jellyfin).
  - `ApiKey` (remote Jellyfin API key, stored as plain text in Jellyfin config).
  - `RemoteUserId` (user id on remote server, resolved later).

- `SmackRemoteClient`
  - Encapsulates HTTP calls to remote Jellyfin servers.
  - Methods:
    - `GetLibrariesAsync(server)`
    - `GetItemsAsync(server, parentId)`
    - `GetStreamUrlAsync(server, itemId)`

- `SmackController` (API controller)
  - Endpoints:
    - `GET /Smack/Servers` – list configured remote servers.
    - `GET /Smack/Libraries/{serverId}` – libraries from remote.
    - `GET /Smack/Items/{serverId}/{parentId}` – items under a remote parent.
    - `GET /Smack/Stream/{serverId}/{itemId}` – return a simple stream/download URL.

- Web UI pages
  - `configPage.html` – manage remote servers (add/edit/delete, validate URL, handle API key).
  - `smackBrowser.html` – user-facing browser for remote content with folder navigation and basic playback.

## Implementation Phases

### Phase 1 – Base plugin + configuration
- [x] Rename plugin and assign a new GUID (implemented as `Plugin` using `SmackPluginConfiguration`).
- [x] Create `SmackPluginConfiguration` and `SmackRemoteServer` models.
- [x] Replace `configPage.html` with Smack-specific UI for adding remote servers.
- [x] Clean up old template `PluginConfiguration` to a stub.

### Phase 2 – Remote API access
- [x] Implement `SmackRemoteClient` using `HttpClient` for remote calls.
- [x] Implement library retrieval from remote Jellyfin API (`GetLibrariesAsync`).
- [x] Implement item retrieval from remote Jellyfin API (`GetItemsAsync`).
- [x] Add basic error handling (invalid ServerUrl, HTTP failures propagated).

### Phase 3 – Plugin API surface
- [x] Implement `SmackController` with endpoints to list servers and libraries.
- [x] Add `/Smack/Items/{serverId}/{parentId}` endpoint for item browsing.
- [x] Add `/Smack/Stream/{serverId}/{itemId}` endpoint for playback URL.
- [x] Add simple unit tests for client logic (`SmackRemoteClient.GetStreamUrlAsync`).

### Phase 4 – Browser UI and playback
- [x] Wire configuration page via `Plugin.GetPages()`.
- [x] Add `smackBrowser.html` to browse servers, libraries, and items via `SmackController`.
- [x] Integrate basic playback by returning a remote stream/download URL and opening it in a new tab.
- [x] Add folder navigation, breadcrumb path, and an `Up` button in `smackBrowser.html`.
- [x] Integrate with Jellyfin's native playback APIs with fallback to `window.open` (implemented in `trySmackNativePlayback`).

### Phase 5 – Polish & enhancements
- [x] Improve UX in config page (edit/delete servers, URL validation).
- [x] Improve API key handling (mask input, don't show raw key, require re-entry only when needed).
- [x] Improve UX in browser page (status line, better error display, placeholders).
- [x] Expand test coverage (comprehensive HTTP mocked tests for SmackRemoteClient, parsing tests for libraries/items).
- [ ] Add optional background sync or caching, if needed.
- [ ] Document security considerations more deeply (threat model, recommended use).

## Notes
- Target framework: .NET 9 (C# 13 features are available but used only when they add clear value).
- Coding style: follow Jellyfin plugin template conventions and analyzers.
- Avoid copying third-party plugin code; rely on Jellyfin API docs and public examples for guidance only.
