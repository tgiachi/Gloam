# Installation Guide

This guide covers all the ways to install and set up Gloam for development.

## Prerequisites

### Required Software

- **.NET 9.0 SDK** - The runtime and development tools
- **Git** - Version control system
- **Task Runner** (optional but recommended) - Build automation tool

### System Requirements

- **Operating System**: Windows 10+, macOS 10.15+, Linux (Ubuntu 18.04+, CentOS 7+)
- **RAM**: Minimum 4GB, Recommended 8GB+
- **Disk Space**: ~500MB for source code and dependencies
- **Display**: Terminal/console with ANSI color support

## Installation Methods

### Method 1: Clone from Source (Recommended for Development)

```bash
# Clone the repository
git clone https://github.com/tgiachi/gloam.git
cd gloam

# Restore dependencies
dotnet restore Gloam.slnx

# Build the solution
dotnet build Gloam.slnx --configuration Release

# Run tests
dotnet test Gloam.slnx --configuration Release
```

### Method 2: Using Task Runner (Recommended)

```bash
# Install Task runner first (see below)
# Then clone and build
git clone https://github.com/tgiachi/gloam.git
cd gloam

# Full development setup
task dev
```

### Method 3: Download Release

1. Go to [Releases](https://github.com/tgiachi/gloam/releases)
2. Download the latest release archive
3. Extract to your preferred location
4. Run `dotnet restore` and `dotnet build`

## Installing Task Runner

Task is a modern task runner that simplifies build processes.

### Windows (Chocolatey)
```powershell
choco install go-task
```

### macOS (Homebrew)
```bash
brew install go-task/tap/go-task
```

### Linux
```bash
# Download binary
curl -L https://github.com/go-task/task/releases/latest/download/task_linux_amd64.tar.gz | tar xz
sudo mv task /usr/local/bin/

# Or using snap
sudo snap install task --classic
```

### Manual Installation
```bash
# Download from https://github.com/go-task/task/releases
# Extract and add to PATH
```

## Development Environment Setup

### 1. IDE Setup

#### Visual Studio Code (Recommended)
```bash
# Install C# extension
code --install-extension ms-dotnettools.csharp

# Install recommended extensions
code --install-extension ms-vscode.vscode-json
code --install-extension redhat.vscode-yaml
```

#### Visual Studio 2022
- Install ".NET desktop development" workload
- Ensure .NET 9.0 SDK is selected

#### Rider
- Install JetBrains Rider
- Open the `Gloam.slnx` solution file

### 2. Environment Configuration

Create a `.env` file in the project root (optional):

```bash
# Development settings
DOTNET_ENVIRONMENT=Development
DOTNET_CLI_TELEMETRY_OPTOUT=1
GLOAM_LOG_LEVEL=Information
```

### 3. Build Configuration

The project uses `Directory.Build.props` for centralized configuration:

```xml
<!-- Key settings -->
<TargetFramework>net9.0</TargetFramework>
<LangVersion>preview</LangVersion>
<Nullable>enable</Nullable>
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
```

## Verification

### Test Your Installation

```bash
# Run the demo
dotnet run --project src/Gloam.Demo

# Expected output: Interactive console game demo
```

### Run Tests

```bash
# Using Task
task test

# Using dotnet
dotnet test Gloam.slnx
```

### Build Documentation

```bash
# Build API docs
task docs-build

# Serve docs locally
task docs-serve
```

## Troubleshooting

### Common Issues

#### Build Errors
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

#### Missing Dependencies
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore --force
```

#### Permission Issues
```bash
# On Linux/macOS
chmod +x tools/Gloam.Cli/bin/Debug/net9.0/gloam
```

### Getting Help

- **Documentation**: [docs.gloam.dev](https://yourdocs.github.io/gloam/)
- **Issues**: [GitHub Issues](https://github.com/tgiachi/gloam/issues)
- **Discussions**: [GitHub Discussions](https://github.com/tgiachi/gloam/discussions)

## Next Steps

1. **Read Getting Started** - Learn basic concepts
2. **Try the Tutorials** - Hands-on learning
3. **Explore Examples** - See Gloam in action
4. **Join the Community** - Connect with other developers