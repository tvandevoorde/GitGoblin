using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using GitGoblin.Core;
using GitGoblin.Git.Abstractions;

namespace GitGoblin.App;

public partial class MainPage : ContentPage
{
	private readonly IGitRepositoryService _gitRepositoryService;
	private string _repositoryPath = Environment.CurrentDirectory;
	private string _statusSummary = "Enter a local repository path to begin.";
	private string _branchSummary = "";
	private bool _isLoading;

	public MainPage(IGitRepositoryService gitRepositoryService)
	{
		_gitRepositoryService = gitRepositoryService;
		InitializeComponent();
		BindingContext = this;
	}

	public ObservableCollection<GitFileChange> Changes { get; } = [];

	public string RepositoryPath
	{
		get => _repositoryPath;
		set => SetProperty(ref _repositoryPath, value);
	}

	public string StatusSummary
	{
		get => _statusSummary;
		set => SetProperty(ref _statusSummary, value);
	}

	public string BranchSummary
	{
		get => _branchSummary;
		set => SetProperty(ref _branchSummary, value);
	}

	public bool IsLoading
	{
		get => _isLoading;
		set => SetProperty(ref _isLoading, value);
	}

	private async void OnLoadStatusClicked(object? sender, EventArgs e)
	{
		if (IsLoading)
		{
			return;
		}

		IsLoading = true;

		try
		{
			RepositoryStatus status = await _gitRepositoryService.GetStatusAsync(RepositoryPath);
			Changes.Clear();

			foreach (GitFileChange change in status.Changes.OrderBy(c => c.Path, StringComparer.OrdinalIgnoreCase))
			{
				Changes.Add(change);
			}

			BranchSummary = $"{status.CurrentBranch} (↑{status.AheadBy} ↓{status.BehindBy})";
			StatusSummary = $"{status.Changes.Count} change(s)";
		}
		catch (Exception ex)
		{
			StatusSummary = ex.Message;
			BranchSummary = string.Empty;
			Changes.Clear();
		}
		finally
		{
			IsLoading = false;
		}
	}

	private void SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(storage, value))
		{
			return;
		}

		storage = value;
		OnPropertyChanged(propertyName);
	}
}
