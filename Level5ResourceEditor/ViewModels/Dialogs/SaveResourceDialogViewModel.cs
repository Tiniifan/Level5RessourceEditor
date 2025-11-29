using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ImaginationGUI.ViewModels;
using ImaginationGUI.Services;
using Microsoft.Win32;
using Level5ResourceEditor.Services;

namespace Level5ResourceEditor.ViewModels.Dialogs
{
    public class SaveResourceDialogViewModel : INotifyPropertyChanged
    {
        private string _selectedFilePath;
        private string _selectedResourceType;
        private string _selectedSceneType;
        private string _magic;

        public event PropertyChangedEventHandler PropertyChanged;

        public SaveResourceDialogViewModel(string currentResourceType, string currentSceneType)
        {
            InitializeCommands();
            InitializeOptions();
            LoadTranslations();

            // Set defaults based on current context
            _selectedResourceType = currentResourceType ?? "RES";
            _selectedSceneType = currentSceneType ?? "Scene3D";

            // Set default magic based on resource and scene type
            UpdateMagicDefault();
        }

        // Properties
        public string SelectedFilePath
        {
            get => _selectedFilePath;
            set
            {
                _selectedFilePath = value;
                OnPropertyChanged(nameof(SelectedFilePath));
            }
        }

        public string SelectedResourceType
        {
            get => _selectedResourceType;
            set
            {
                _selectedResourceType = value;
                OnPropertyChanged(nameof(SelectedResourceType));
                UpdateMagicDefault();
            }
        }

        public string SelectedSceneType
        {
            get => _selectedSceneType;
            set
            {
                _selectedSceneType = value;
                OnPropertyChanged(nameof(SelectedSceneType));
                UpdateMagicDefault();
            }
        }

        public string Magic
        {
            get => _magic;
            set
            {
                _magic = value;
                OnPropertyChanged(nameof(Magic));
            }
        }

        public ObservableCollection<string> ResourceTypes { get; private set; }
        public ObservableCollection<string> SceneTypes { get; private set; }

        // Translation properties
        public string FilePathLabel { get; private set; }
        public string BrowseButtonText { get; private set; }
        public string ResourceTypeLabel { get; private set; }
        public string SceneTypeLabel { get; private set; }
        public string MagicLabel { get; private set; }
        public string SaveButtonText { get; private set; }
        public string CancelButtonText { get; private set; }
        public string Title { get; private set; }

        // Commands
        public ICommand BrowseCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        // Events
        public event EventHandler<bool> DialogClosed;

        private void InitializeCommands()
        {
            BrowseCommand = new RelayCommand(ExecuteBrowse);
            SaveCommand = new RelayCommand(ExecuteSave);
            CancelCommand = new RelayCommand(ExecuteCancel);
        }

        private void InitializeOptions()
        {
            ResourceTypes = new ObservableCollection<string> { "RES", "XRES" };
            SceneTypes = new ObservableCollection<string> { "Scene3D", "Scene2D" };
        }

        private void LoadTranslations()
        {
            var ts = TranslationService.Instance;
            FilePathLabel = ts.GetTranslation("Globals.FileDialog", "filePath");
            BrowseButtonText = ts.GetTranslation("Globals.FileDialog", "browse");
            ResourceTypeLabel = ts.GetTranslation("Views.App", "resourceTypeLabel");
            SceneTypeLabel = ts.GetTranslation("Views.App", "sceneTypeLabel");
            MagicLabel = ts.GetTranslation("Views.App", "magicLabel");
            SaveButtonText = ts.GetTranslation("Globals.FileDialog", "save");
            CancelButtonText = ts.GetTranslation("Globals.FileDialog", "cancel");
            Title = ts.GetTranslation("Globals.FileDialog", "saveFile");

            OnPropertyChanged(nameof(FilePathLabel));
            OnPropertyChanged(nameof(BrowseButtonText));
            OnPropertyChanged(nameof(ResourceTypeLabel));
            OnPropertyChanged(nameof(SceneTypeLabel));
            OnPropertyChanged(nameof(MagicLabel));
            OnPropertyChanged(nameof(SaveButtonText));
            OnPropertyChanged(nameof(CancelButtonText));
            OnPropertyChanged(nameof(Title));
        }

        private void UpdateMagicDefault()
        {
            if (SelectedResourceType == "RES")
            {
                if (SelectedSceneType == "Scene3D")
                {
                    Magic = "CHRC00";
                }
                else // Scene2D
                {
                    Magic = "ANMC00";
                }
            }
            else // XRES
            {
                if (SelectedSceneType == "Scene3D")
                {
                    Magic = "XRES";
                }
                else // Scene2D
                {
                    Magic = "XA01";
                }
            }
        }

        private void ExecuteBrowse()
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "RES Files (*.bin)|*.bin|All Files (*.*)|*.*",
                DefaultExt = ".bin",
                FileName = "RES.bin"
            };

            if (saveDialog.ShowDialog() == true)
            {
                SelectedFilePath = saveDialog.FileName;
            }
        }

        private void ExecuteSave()
        {
            if (string.IsNullOrEmpty(SelectedFilePath))
            {
                MessageBox.Show(
                    TranslationService.Instance.GetTranslation("Globals.Messages", "selectFilePath"),
                    TranslationService.Instance.GetTranslation("Globals.Messages", "error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(Magic))
            {
                MessageBox.Show(
                    TranslationService.Instance.GetTranslation("Globals.Messages", "enterMagicValue"),
                    TranslationService.Instance.GetTranslation("Globals.Messages", "error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            DialogClosed?.Invoke(this, true);
        }

        private void ExecuteCancel()
        {
            DialogClosed?.Invoke(this, false);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}