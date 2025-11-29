# GitGoblin

A cross-platform GitHub client built with .NET MAUI, offering repository browsing, issues management, commits viewing, notifications, and comprehensive pull request management. GitGoblin supports both GitHub (including GitHub Enterprise) and Azure DevOps Server.

## Features

- **Repository Browsing**: View and search your repositories from GitHub and Azure DevOps
- **Issues Management**: Browse, filter, and view issues with labels and assignees
- **Commits Viewing**: Explore commit history with branch selection and detailed commit information
- **Notifications**: Stay updated with GitHub notifications and mark them as read
- **Pull Request Management**:
  - View pull requests with status, labels, and branch information
  - Review changes with file diff viewing
  - Submit reviews (Comment, Approve, Request Changes)
  - Create new pull requests with branch selection
- **Multi-Provider Support**:
  - GitHub.com
  - GitHub Enterprise Server
  - Azure DevOps Services
  - Azure DevOps Server (TFS)

## Project Structure

```
GitGoblin/
├── src/
│   ├── GitGoblin.Core/          # Core library with models, services, and ViewModels
│   │   ├── Models/              # Data models (Repository, Issue, Commit, PullRequest, etc.)
│   │   ├── Services/            # Git service interfaces and implementations
│   │   │   ├── GitHub/          # GitHub API integration using Octokit
│   │   │   └── AzureDevOps/     # Azure DevOps API integration
│   │   └── ViewModels/          # MVVM ViewModels using CommunityToolkit.Mvvm
│   └── GitGoblin.App/           # .NET MAUI application
│       ├── Converters/          # Value converters for XAML bindings
│       ├── Views/               # XAML pages and code-behind
│       ├── Platforms/           # Platform-specific code
│       └── Resources/           # Images, fonts, and styles
└── GitGoblin.sln               # Solution file
```

## Requirements

- .NET 10 SDK
- .NET MAUI workload (`dotnet workload install maui`)
- For Windows builds: Windows 10/11 with Visual Studio 2022

## Building

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Build for Windows (on Windows)
dotnet build -f net10.0-windows10.0.19041.0
```

## Development

The project uses the MVVM pattern with:
- **CommunityToolkit.Mvvm** for ViewModels with source generators
- **Octokit** for GitHub API integration
- **Microsoft.TeamFoundationServer.Client** for Azure DevOps integration

### Adding a New Feature

1. Add models to `GitGoblin.Core/Models/`
2. Update service interfaces in `IGitService.cs`
3. Implement in both `GitHubService.cs` and `AzureDevOpsService.cs`
4. Create ViewModel in `GitGoblin.Core/ViewModels/`
5. Create View in `GitGoblin.App/Views/`
6. Register in `MauiProgram.cs` and `AppShell.xaml.cs`

## Configuration

To connect to GitHub:
1. Generate a Personal Access Token at https://github.com/settings/tokens
2. Enter the token in Settings
3. For GitHub Enterprise, also provide the server URL

To connect to Azure DevOps:
1. Generate a PAT in Azure DevOps (User Settings → Personal Access Tokens)
2. Enter the organization URL and token in Settings

## Screenshots

The application features a modern, clean UI with:
- Flyout navigation
- Card-based layouts
- Status indicators
- Pull-to-refresh functionality
- Dark mode support

## License

MIT

## Contributing

Contributions are welcome! Please feel free to submit pull requests.

