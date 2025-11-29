using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ImaginationGUI.Services;
using ImaginationGUI.ViewModels;
using Level5ResourceEditor.Services;
using Level5ResourceEditor.ViewModels.Editor;
using Level5ResourceEditor.Views.Editor;

namespace Level5ResourceEditor
{
    public partial class App : Application
    {
        private Button _saveButton;
        private Button _openButton;
        private Level5ResourceMainContent _mainContent;
        private Level5ResourceViewModel _editorViewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            TranslationService.Instance.Initialize();

            var menuButtonStyle = Application.Current.FindResource("MenuButtonStyle") as Style;

            _openButton = new Button
            {
                Content = TranslationService.Instance.GetTranslation("Views.App", "buttonOpen"),
                Style = menuButtonStyle,
                Margin = new Thickness(0, 0, 5, 0)
            };
            _openButton.Click += OpenButton_Click;

            _saveButton = new Button
            {
                Content = TranslationService.Instance.GetTranslation("Views.App", "buttonSave"),
                Style = menuButtonStyle,
                IsEnabled = false,
                Margin = new Thickness(0, 0, 5, 0)
            };
            _saveButton.Click += SaveButton_Click;

            _editorViewModel = new Level5ResourceViewModel();

            _mainContent = new Level5ResourceMainContent
            {
                DataContext = _editorViewModel
            };

            var viewModel = new MainViewModel
            {
                IsLoading = false,
                Title = "Level5 Resource Editor",
                MainContent = _mainContent,
                CustomTitleBarButtons = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        _openButton,
                        _saveButton
                    }
                }
            };

            var workingArea = SystemParameters.WorkArea;

            var mainWindow = new ImaginationGUI.Views.MainWindow
            {
                DataContext = viewModel,
                IsMaximized = true,
                Left = workingArea.Left,
                Top = workingArea.Top,
                Width = workingArea.Width,
                Height = workingArea.Height,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            mainWindow.Show();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = TranslationService.Instance.GetTranslation("Globals.FileDialog", "openFile"),
                    Filter = "RES/XRES Files (RES.bin)|RES.bin|All Files (*.*)|*.*",
                    FilterIndex = 1
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;
                    _editorViewModel.LoadFile(filePath);

                    string fileName = System.IO.Path.GetFileName(filePath);
                    var mainViewModel = Application.Current.MainWindow?.DataContext as MainViewModel;
                    if (mainViewModel != null)
                    {
                        mainViewModel.Title = $"Level5 Resource Editor - {fileName}";
                    }

                    _saveButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{TranslationService.Instance.GetTranslation("Globals.Messages", "errorOpeningFile")}: {ex.Message}",
                    TranslationService.Instance.GetTranslation("Globals.Messages", "error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // TODO: Implement save logic
                MessageBox.Show(
                    "Save functionality not yet implemented",
                    "Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{TranslationService.Instance.GetTranslation("Globals.Messages", "errorSavingFile")}: {ex.Message}",
                    TranslationService.Instance.GetTranslation("Globals.Messages", "error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}