﻿// Copyright (c) Files Community
// Licensed under the MIT License.

using System.IO;

namespace Files.App.Actions
{
	internal sealed partial class OpenFileLocationAction : ObservableObject, IAction
	{
		private readonly IContentPageContext context;

		public string Label
			=> Strings.OpenFileLocation.GetLocalizedResource();

		public string Description
			=> Strings.OpenFileLocationDescription.GetLocalizedResource();

		public RichGlyph Glyph
			=> new(baseGlyph: "\uE8DA");

		public bool IsExecutable =>
			context.ShellPage is not null &&
			context.HasSelection &&
			context.SelectedItem is IShortcutItem;

		public OpenFileLocationAction()
		{
			context = Ioc.Default.GetRequiredService<IContentPageContext>();

			context.PropertyChanged += Context_PropertyChanged;
		}

		public async Task ExecuteAsync(object? parameter = null)
		{
			if (context.ShellPage?.ShellViewModel is null)
				return;

			var item = context.SelectedItem as IShortcutItem;

			if (string.IsNullOrWhiteSpace(item?.TargetPath))
				return;

			// Check if destination path exists
			var folderPath = Path.GetDirectoryName(item.TargetPath);
			var destFolder = await context.ShellPage.ShellViewModel.GetFolderWithPathFromPathAsync(folderPath);

			if (destFolder)
			{
				context.ShellPage?.NavigateWithArguments(context.ShellPage.InstanceViewModel.FolderSettings.GetLayoutType(folderPath), new NavigationArguments()
				{
					NavPathParam = folderPath,
					SelectItems = [Path.GetFileName(item.TargetPath.TrimPath())],
					AssociatedTabInstance = context.ShellPage
				});
			}
			else if (destFolder == FileSystemStatusCode.NotFound)
			{
				await DialogDisplayHelper.ShowDialogAsync(Strings.FileNotFoundDialog_Title.GetLocalizedResource(), Strings.FileNotFoundDialog_Text.GetLocalizedResource());
			}
			else
			{
				await DialogDisplayHelper.ShowDialogAsync(Strings.InvalidItemDialogTitle.GetLocalizedResource(),
					string.Format(Strings.InvalidItemDialogContent.GetLocalizedResource(), Environment.NewLine, destFolder.ErrorCode.ToString()));
			}
		}

		private void Context_PropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName is nameof(IContentPageContext.HasSelection))
				OnPropertyChanged(nameof(IsExecutable));
		}
	}
}
