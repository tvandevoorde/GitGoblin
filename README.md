# GitGoblin

GitGoblin is a .NET MAUI Git client focused on a user-friendly desktop workflow inspired by Azure DevOps Server and Visual Studio Code.

## Current implementation status

This first implementation slice includes:

- MAUI solution scaffolding with modular projects.
- Git provider abstraction (`IGitRepositoryService`).
- CLI-based Git status provider (`GitCliRepositoryService`) using `git status --porcelain=v2 --branch`.
- Repository dashboard UI with:
	- Repository path input
	- Load status action
	- Branch/ahead/behind summary
	- Working tree change list (kind/path/status code)

## Solution structure

- `src/GitGoblin.App` - MAUI desktop application shell and pages
- `src/GitGoblin.Core` - domain models (`RepositoryStatus`, `GitFileChange`, `GitChangeKind`)
- `src/GitGoblin.Git.Abstractions` - Git service contracts
- `src/GitGoblin.Git.Cli` - Git CLI provider implementation
- `src/GitGoblin.Integrations` - integration service contracts (Azure DevOps, editor bridge, AI assistant)
- `tests/GitGoblin.Core.Tests` - core tests
- `tests/GitGoblin.Git.Cli.Tests` - Git CLI provider tests

## Build and run

Prerequisites:

- .NET SDK 10+
- MAUI workloads installed
- Git installed and available on PATH

Commands:

```powershell
dotnet restore GitGoblin.slnx
dotnet build GitGoblin.slnx
dotnet build src/GitGoblin.App/GitGoblin.App.csproj -f net10.0-windows10.0.19041.0
dotnet run --project src/GitGoblin.App/GitGoblin.App.csproj -f net10.0-windows10.0.19041.0
```

## Next implementation slices

1. Core Git workflow commands (clone/init/open, stage/unstage, commit/amend, fetch/pull/push).
2. History and diff views with keyboard-first interactions.
3. Azure DevOps Server integration spike and compatibility matrix.
4. VS Code handoff actions and GitHub Copilot CLI-based AI helpers behind feature flags.
