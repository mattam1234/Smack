# Smack Jellyfin Plugin

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-GPL--3.0-blue.svg)](LICENSE)
[![Status](https://img.shields.io/badge/Status-Experimental-orange)](https://github.com/mattam1234/Smack)

Smack is a Jellyfin plugin that lets one Jellyfin server browse and stream media from one or more remote Jellyfin servers.

> **⚠️ Status: Experimental / WIP** – APIs and behavior may change.
> 
> **Note**: The plugin GUID `11111111-2222-3333-4444-555555555555` is intentionally simple for development. If you fork this project for production use, generate a new unique GUID to avoid conflicts.

## Table of Contents

- [Features](#features)
- [How It Works](#how-it-works)
- [Setup](#setup)
  - [Requirements](#requirements)
  - [Build](#build)
  - [Install in Jellyfin](#install-in-jellyfin)
  - [Configure Remote Servers](#configure-remote-servers)
  - [Browse and Stream](#browse-and-stream)
- [Security Notes](#security-notes)
- [Development](#development)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [Roadmap](#roadmap)
- [License](#license)

## Features

✨ **Core Capabilities**

- 🌐 **Multiple Remote Servers**: Configure and access multiple remote Jellyfin instances from a single interface
- 📁 **Library Browsing**: Navigate remote libraries, folders, and media items seamlessly
- 🎬 **Direct Streaming**: Stream content directly from remote servers without local storage
- 🔐 **Secure Configuration**: API key-based authentication with masked credential entry
- 🎨 **Jellyfin Integration**: Native integration with Jellyfin's admin UI and navigation

**What Smack Does NOT Do**
- Does not copy or sync media to your local server
- Does not transcode remote content
- Does not create local library entries for remote items
- Does not require database changes on either server

## How It Works

```
┌─────────────────────┐         API Calls          ┌──────────────────────┐
│  Local Jellyfin     │◄──────────────────────────►│  Remote Jellyfin     │
│  + Smack Plugin     │      (with API Key)        │  Server              │
└─────────────────────┘                            └──────────────────────┘
         │                                                    │
         │                                                    │
    ┌────▼────┐                                          ┌────▼────┐
    │  User   │                                          │  Media  │
    │  Browser│                                          │  Files  │
    └─────────┘                                          └─────────┘
```

**Workflow**

1. **Configuration**: Add remote Jellyfin servers to Smack using their API endpoints and authentication keys

2. **Authentication**: Smack uses the provided API key to authenticate with remote servers

3. **Browsing**: The Smack Browser page provides a dedicated interface to:
   - Select a configured remote server
   - Browse available libraries (Movies, TV, Music, etc.)
   - Navigate through folders and collections
   - View item metadata

4. **Streaming**: When you select content to play:
   - Smack retrieves the direct stream URL from the remote server
   - Opens the stream in your browser using the remote server's download API
   - Content plays directly from the remote source

**Technical Details**
- Uses Jellyfin's public HTTP API for all remote interactions
- No permanent synchronization or caching of remote metadata
- Real-time browsing ensures you always see the current remote library state
- Lightweight operation with minimal impact on local server performance

## Setup

### Requirements

- **Jellyfin Server**: Version 10.9.x (compatible with plugin template references)
- **.NET SDK**: Version 9.0 or later (for building from source)
- **Remote Jellyfin Server**: One or more remote Jellyfin instances with API access

### Build

From the solution root:

```bash
# Build in Debug mode
dotnet build

# Build in Release mode
dotnet build -c Release
```

The compiled plugin will be located in `Jellyfin.Plugin.Smack/bin/Release/net9.0/`

### Install in Jellyfin

1. **Build the plugin** in Release mode:
   ```bash
   dotnet build -c Release
   ```

2. **Locate the compiled plugin** in `Jellyfin.Plugin.Smack/bin/Release/net9.0/`

3. **Copy the plugin files** to your Jellyfin plugins directory:
   - **Linux**: `/var/lib/jellyfin/plugins/Smack/`
   - **Windows**: `%APPDATA%\Jellyfin\Server\plugins\Smack\`
   - **Docker**: Map to `/config/plugins/Smack/` in your container

4. **Restart Jellyfin** to load the plugin

5. **Configure the plugin**:
   - Navigate to **Dashboard** → **Plugins** → **Smack**
   - Add your remote server configurations

### Configure Remote Servers

1. **Open the Smack configuration page**:
   - Go to **Dashboard** → **Plugins** → **Smack** → **Settings**

2. **Add a remote server**:
   - **Name**: A friendly name for the remote server (e.g., "Home Server", "Friend's Jellyfin")
   - **Server URL**: Base URL of the remote Jellyfin server
     - Example: `https://remote.example.com` or `http://192.168.1.100:8096`
   - **API Key**: An API key from the remote server
     - Generate this in the remote Jellyfin: **Dashboard** → **API Keys** → **Add Key**
     - The key should have permissions to access the media you want to stream

3. **Save the configuration**:
   - Click **Save Server** to add the remote server
   - The server will appear in your configured servers list

4. **Manage existing servers**:
   - **Edit**: Update server name or URL (API key requires re-entry to change)
   - **Delete**: Remove a server from your configuration

### Browse and Stream

1. **Open the Smack Browser**:
   - Navigate to **Dashboard** → **Plugins** → **SmackBrowser**

2. **Select a remote server** from the dropdown menu

3. **Choose a library** to browse (Movies, TV Shows, Music, etc.)

4. **Navigate the content**:
   - Click **Open** on folders to browse deeper
   - Click **Up** to go back to the parent folder
   - Use the breadcrumb path to see your current location

5. **Play content**:
   - Click **Open stream** on any media item
   - The stream will open in a new browser tab using the remote server's URL

> **Note**: Smack does not import or sync content to your local server. It provides on-demand browsing and streaming directly from the remote server.

## Security Notes

⚠️ **Important Security Considerations**

### API Key Storage
- Remote API keys are stored in **plain text** in Jellyfin's configuration files
- **Recommendation**: Use a dedicated, restricted user account on remote servers
- **Do not** use administrator or highly-privileged accounts
- Only grant access to the specific libraries you want to share

### Configuration UI Security
- API key input fields are masked during entry
- Stored keys show only as `configured` or `not set` in the server list
- Keys are not pre-filled when editing; re-enter to update
- This prevents accidental exposure in screenshots or over-the-shoulder viewing

### Network Security
- Remote streams are accessed directly from the remote Jellyfin server
- **Production use**: Always use HTTPS for both local and remote servers
- Ensure proper SSL/TLS certificate configuration
- Consider using VPN or secure tunneling for remote access

### Best Practices
- Regularly rotate API keys
- Use firewall rules to restrict access to trusted networks
- Monitor access logs on remote servers
- Remove unused remote server configurations
- Keep both Jellyfin instances updated to the latest security patches

## Roadmap

See [`docs/PLAN.md`](docs/PLAN.md) for the detailed implementation plan and remaining optional enhancements.

### Potential Future Enhancements
- Background sync/caching of remote content
- Advanced playback integration
- Multi-server search
- Remote library aggregation

## Development

### Technology Stack
- **Target framework**: .NET 9
- **Language**: C# 13
- **Coding style**: Follows Jellyfin plugin template conventions
- **Code analysis**: StyleCop + Jellyfin ruleset

### Building from Source

```bash
# Clone the repository
git clone https://github.com/mattam1234/Smack.git
cd Smack

# Build the plugin
dotnet build

# Run tests
dotnet test
```

### Project Structure
```
Jellyfin.Plugin.Smack/     # Main plugin code
├── Configuration/         # Plugin configuration models
├── Controllers/           # API controllers
├── Models/                # Data models
└── Plugin.cs             # Plugin entry point
Jellyfin.Plugin.Tests/     # Unit tests
docs/                      # Documentation
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run specific test
dotnet test --filter "FullyQualifiedName~SmackRemoteClientTests"
```

## Troubleshooting

### Plugin Not Showing Up in Jellyfin

1. Verify the plugin files are in the correct Jellyfin plugins directory
2. Check Jellyfin logs for any loading errors
3. Ensure the Jellyfin version is compatible (10.9.x)
4. Restart the Jellyfin server after copying plugin files

### Cannot Connect to Remote Server

1. Verify the remote server URL is accessible from your Jellyfin server
2. Check that the API key is valid and has the necessary permissions
3. Ensure the remote server is running and accessible
4. Check network/firewall settings if accessing across networks

### Stream Opens But Won't Play

1. Verify the remote server allows external streaming
2. Check if the remote server requires HTTPS
3. Ensure your browser/player supports the media format
4. Try accessing the remote server directly to verify the content plays

### Configuration Not Saving

1. Check Jellyfin has write permissions to its configuration directory
2. Verify the plugin configuration file is not corrupted
3. Check Jellyfin logs for any save errors

## Contributing

Contributions are welcome! Here's how you can help:

1. **Report bugs**: Open an issue describing the problem and steps to reproduce
2. **Suggest features**: Open an issue with your feature idea and use case
3. **Submit pull requests**: 
   - Fork the repository
   - Create a feature branch (`git checkout -b feature/amazing-feature`)
   - Make your changes with clear commit messages
   - Ensure tests pass (`dotnet test`)
   - Submit a pull request

### Code Guidelines

- Follow the existing code style and conventions
- Add tests for new functionality
- Update documentation as needed
- Ensure all builds and tests pass before submitting

## License

This project follows the same license as the original Jellyfin plugin template. See the [LICENSE](LICENSE) file for details.
