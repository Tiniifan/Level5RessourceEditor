using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ImaginationGUI.Services;
using ImaginationGUI.ViewModels;
using Level5ResourceEditor.Services;
using Level5ResourceEditor.ViewModels.Editor;
using Level5ResourceEditor.Views.Editor;
using Level5ResourceEditor.Infrastructure.Helpers.IO;
using Level5ResourceEditor.ViewModels.Dialogs;
using Level5ResourceEditor.Views.Dialogs;

namespace Level5ResourceEditor
{
    public partial class App : Application
    {
        private Button _saveButton;
        private Button _openButton;
        private ComboBox _languageComboBox;
        private Level5ResourceMainContent _mainContent;
        private Level5ResourceViewModel _editorViewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Load the saved language or use the system language
            string savedLanguage = Level5ResourceEditor.Properties.Settings.Default.Language;

            if (!string.IsNullOrEmpty(savedLanguage))
            {
                // Use the saved language
                TranslationService.Instance.Initialize(savedLanguage);
            }
            else
            {
                // First startup: use the system language
                TranslationService.Instance.Initialize();
            }

            var menuButtonStyle = Application.Current.FindResource("MenuButtonStyle") as Style;
            var comboBoxStyle = Application.Current.FindResource("ImaginationComboBox") as Style;

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
                Margin = new Thickness(0, 0, 5, 0)
            };
            _saveButton.Click += SaveButton_Click;

            // ComboBox for language selection
            var languages = new[]
            {
                new { Code = "de", Name = "Deutsch" },
                new { Code = "en", Name = "English" },
                new { Code = "es", Name = "Español" },
                new { Code = "fr", Name = "Français" },
                new { Code = "it", Name = "Italiano" },
                new { Code = "ja", Name = "日本語" },
                new { Code = "pt", Name = "Português" },
                new { Code = "zh_hans", Name = "简体中文" },
                new { Code = "zh_hant", Name = "繁體中文" }
            };

            _languageComboBox = new ComboBox
            {
                Style = comboBoxStyle,
                Width = 120,
                ItemsSource = languages,
                DisplayMemberPath = "Name",
                SelectedValuePath = "Code",
                Margin = new Thickness(0, 0, 10, 0)
            };

            _languageComboBox.SelectionChanged += LanguageComboBox_SelectionChanged;
            _languageComboBox.SelectedValue = TranslationService.Instance.CurrentLanguage;

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
                        _saveButton,
                        _languageComboBox
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

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_languageComboBox.SelectedValue != null)
            {
                string newLanguage = _languageComboBox.SelectedValue.ToString();
                TranslationService.Instance.ChangeLanguage(newLanguage);

                // Save the selected language
                Level5ResourceEditor.Properties.Settings.Default.Language = newLanguage;
                Level5ResourceEditor.Properties.Settings.Default.Save();

                // Update button texts
                _openButton.Content = TranslationService.Instance.GetTranslation("Views.App", "buttonOpen");
                _saveButton.Content = TranslationService.Instance.GetTranslation("Views.App", "buttonSave");

                // Todo???
                // _editorViewModel?.RefreshTranslations();
            }
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
                // Determine current scene type based on selected tab
                string currentSceneType = "Scene3D";

                var dialogViewModel = new SaveResourceDialogViewModel(
                    _editorViewModel.CurrentResourceType,
                    currentSceneType);

                var dialog = new SaveResourceDialog
                {
                    ViewModel = dialogViewModel
                };

                var dialogWindow = new ImaginationGUI.Views.MainWindow
                {
                    IsMaximized = false,
                    Width = 520,
                    Height = 450,
                    ResizeMode = ResizeMode.NoResize,
                    ShowInTaskbar = false,
                    Title = dialogViewModel.Title,
                    Content = dialog,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    MaxHeight = 550,
                    MinHeight = 550,
                    MaxWidth = 450,
                    MinWidth = 450
                };

                bool? result = null;
                dialogViewModel.DialogClosed += (s, success) =>
                {
                    result = success;
                    dialogWindow.Close();
                };

                dialogWindow.ShowDialog();

                if (result == true)
                {
                    var items = _editorViewModel.GetItems();
                    string filePath = dialogViewModel.SelectedFilePath;
                    string resourceType = dialogViewModel.SelectedResourceType;
                    string sceneType = dialogViewModel.SelectedSceneType;
                    string magic = dialogViewModel.Magic;

                    if (resourceType == "RES")
                    {
                        var handler = new RESFileHandler();
                        if (sceneType == "Scene3D")
                        {
                            handler.SaveScene3D(items, filePath, magic);
                        }
                        else
                        {
                            handler.SaveScene2D(items, filePath, magic);
                        }
                    }
                    else // XRES
                    {
                        var handler = new XRESFileHandler();
                        if (sceneType == "Scene3D")
                        {
                            handler.SaveScene3D(items, filePath, magic);
                        }
                        else
                        {
                            handler.SaveScene2D(items, filePath, magic);
                        }
                    }

                    MessageBox.Show(
                        TranslationService.Instance.GetTranslation("Globals.Messages", "fileSavedSuccessfully"),
                        TranslationService.Instance.GetTranslation("Globals.Messages", "success"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (NotImplementedException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    TranslationService.Instance.GetTranslation("Globals.Messages", "error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
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