using System;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ImaginationGUI.ViewModels;
using StudioElevenLib.Level5.Resource;
using StudioElevenLib.Level5.Resource.RES;
using StudioElevenLib.Level5.Resource.XRES;
using StudioElevenLib.Level5.Resource.Types;
using StudioElevenLib.Level5.Resource.Types.Scene3D;
using Level5ResourceEditor.Models;

namespace Level5ResourceEditor.ViewModels.Editor
{
    public class Level5ResourceViewModel : BaseViewModel, INotifyPropertyChanged
    {
        // Scene3D Material Items
        private ObservableCollection<TypeListViewItem> _scene3DMaterialItems;
        private TypeListViewItem _selectedScene3DMaterialItem;

        // Scene3D Node Items
        private ObservableCollection<TypeListViewItem> _scene3DNodeItems;
        private TypeListViewItem _selectedScene3DNodeItem;

        // Scene2D Material Items
        private ObservableCollection<TypeListViewItem> _scene2DMaterialItems;
        private TypeListViewItem _selectedScene2DMaterialItem;

        // Scene2D Node Items
        private ObservableCollection<TypeListViewItem> _scene2DNodeItems;
        private TypeListViewItem _selectedScene2DNodeItem;

        private ObservableCollection<RESElement> _elements;
        private RESElement _selectedElement;
        private Dictionary<RESType, List<RESElement>> _items;

        private bool _isInternalUpdate = false;

        private string _currentResourceType = "RES";
        private string _loadedFilePath;

        public ObservableCollection<TypeListViewItem> Scene3DMaterialItems
        {
            get => _scene3DMaterialItems;
            set
            {
                _scene3DMaterialItems = value;
                OnPropertyChanged(nameof(Scene3DMaterialItems));
            }
        }

        public TypeListViewItem SelectedScene3DMaterialItem
        {
            get => _selectedScene3DMaterialItem;
            set
            {
                _selectedScene3DMaterialItem = value;
                OnPropertyChanged(nameof(SelectedScene3DMaterialItem));
                ClearOtherSelections("Scene3DMaterial");
                UpdateElementsList();
            }
        }

        public ObservableCollection<TypeListViewItem> Scene3DNodeItems
        {
            get => _scene3DNodeItems;
            set
            {
                _scene3DNodeItems = value;
                OnPropertyChanged(nameof(Scene3DNodeItems));
            }
        }

        public TypeListViewItem SelectedScene3DNodeItem
        {
            get => _selectedScene3DNodeItem;
            set
            {
                _selectedScene3DNodeItem = value;
                OnPropertyChanged(nameof(SelectedScene3DNodeItem));
                ClearOtherSelections("Scene3DNode");
                UpdateElementsList();
            }
        }

        public ObservableCollection<TypeListViewItem> Scene2DMaterialItems
        {
            get => _scene2DMaterialItems;
            set
            {
                _scene2DMaterialItems = value;
                OnPropertyChanged(nameof(Scene2DMaterialItems));
            }
        }

        public TypeListViewItem SelectedScene2DMaterialItem
        {
            get => _selectedScene2DMaterialItem;
            set
            {
                _selectedScene2DMaterialItem = value;
                OnPropertyChanged(nameof(SelectedScene2DMaterialItem));
                ClearOtherSelections("Scene2DMaterial");
                UpdateElementsList();
            }
        }

        public ObservableCollection<TypeListViewItem> Scene2DNodeItems
        {
            get => _scene2DNodeItems;
            set
            {
                _scene2DNodeItems = value;
                OnPropertyChanged(nameof(Scene2DNodeItems));
            }
        }

        public TypeListViewItem SelectedScene2DNodeItem
        {
            get => _selectedScene2DNodeItem;
            set
            {
                _selectedScene2DNodeItem = value;
                OnPropertyChanged(nameof(SelectedScene2DNodeItem));
                ClearOtherSelections("Scene2DNode");
                UpdateElementsList();
            }
        }

        public ObservableCollection<RESElement> Elements
        {
            get => _elements;
            set
            {
                if (_elements != null)
                    _elements.CollectionChanged -= OnElementsCollectionChanged;

                _elements = value;

                if (_elements != null)
                    _elements.CollectionChanged += OnElementsCollectionChanged;

                OnPropertyChanged(nameof(Elements));
            }
        }

        public RESElement SelectedElement
        {
            get => _selectedElement;
            set
            {
                _selectedElement = value;
                OnPropertyChanged(nameof(SelectedElement));
            }
        }

        public string CurrentResourceType
        {
            get => _currentResourceType;
            set
            {
                _currentResourceType = value;
                OnPropertyChanged(nameof(CurrentResourceType));
            }
        }

        public string LoadedFilePath
        {
            get => _loadedFilePath;
            set
            {
                _loadedFilePath = value;
                OnPropertyChanged(nameof(LoadedFilePath));
            }
        }

        public Dictionary<RESType, List<RESElement>> GetItems()
        {
            return _items;
        }

        public ICommand AddElementCommand { get; private set; }
        public ICommand DeleteElementCommand { get; private set; }

        public Level5ResourceViewModel()
        {
            _elements = new ObservableCollection<RESElement>();
            _elements.CollectionChanged += OnElementsCollectionChanged;

            Scene3DMaterialItems = new ObservableCollection<TypeListViewItem>();
            Scene3DNodeItems = new ObservableCollection<TypeListViewItem>();
            Scene2DMaterialItems = new ObservableCollection<TypeListViewItem>();
            Scene2DNodeItems = new ObservableCollection<TypeListViewItem>();
            Elements = new ObservableCollection<RESElement>();

            AddElementCommand = new RelayCommand(AddElement, _ => GetSelectedItem() != null);
            DeleteElementCommand = new RelayCommand(DeleteElement, _ => SelectedElement != null);

            InitializeEmptyResource();
        }

        private void InitializeEmptyResource()
        {
            _items = new Dictionary<RESType, List<RESElement>>();

            InitializeScene3DTypes();
        }

        private void InitializeScene3DTypes()
        {
            // Scene3D Material Types
            var scene3DMaterialTypes = new[]
            {
                (RESType.Ref, null, "Ref"),
                (RESType.Material1, null, "Material 1"),
                (RESType.Material2, null, "Material 2"),
                (RESType.TextureData, typeof(RESTextureData), "Texture Data (RES)"),
                (RESType.TextureData, typeof(XRESTextureData), "Texture Data (XRES)"),
                (RESType.MaterialData, typeof(ResMaterialData), "Material Data")
            };

            Scene3DMaterialItems.Clear();
            foreach (var (type, filterType, displayName) in scene3DMaterialTypes)
            {
                var key = $"{type}_{filterType?.Name ?? "default"}";

                Scene3DMaterialItems.Add(new TypeListViewItem
                {
                    DisplayName = displayName,
                    Type = type,
                    FilterType = filterType,
                    ElementCount = 0
                });
            }

            // Scene3D Node Types
            var scene3DNodeTypes = new[]
            {
                (RESType.MeshName, "Mesh"),
                (RESType.Bone, "Bone"),
                (RESType.AnimationMTN2, "MTN2"),
                (RESType.AnimationMTN3, "MTN3"),
                (RESType.AnimationIMN2, "IMN2"),
                (RESType.AnimationMTM2, "MTM2"),
                (RESType.Shading, "Shading"),
                (RESType.Properties, "Property"),
                (RESType.MTNINF,  "MTNINF"),
                (RESType.MTNINF2,  "MTNINF2"),
                (RESType.IMMINF, "IMMINF"),
                (RESType.MTMINF,"MTMINF"),
                (RESType.Textproj, "Textproj")
            };

            Scene3DNodeItems.Clear();
            foreach (var (type, displayName) in scene3DNodeTypes)
            {
                var key = $"{type}_{typeof(RESElement)?.Name ?? "default"}";

                Scene3DNodeItems.Add(new TypeListViewItem
                {
                    DisplayName = displayName,
                    Type = type,
                    FilterType = null,
                    ElementCount = 0
                });
            }
        }

        public void LoadFile(string filePath)
        {
            LoadedFilePath = filePath;

            IResource resource = Resourcer.GetResource(File.ReadAllBytes(filePath));

            // Detect resource type
            CurrentResourceType = resource.Name;

            _items = resource.Items;

            RefreshAllLists();
        }

        private void RefreshAllLists()
        {
            // Refresh Scene3D Material Items
            foreach (var item in Scene3DMaterialItems)
            {
                if (_items.ContainsKey(item.Type))
                {
                    if (item.FilterType != null)
                    {
                        // Count only items of the specified type
                        item.ElementCount = _items[item.Type].Count(e => e.GetType() == item.FilterType);
                    }
                    else
                    {
                        item.ElementCount = _items[item.Type].Count;
                    }
                }
                else
                {
                    item.ElementCount = 0;
                }
            }

            // Refresh Scene3D Node Items
            foreach (var item in Scene3DNodeItems)
            {
                item.ElementCount = _items.ContainsKey(item.Type) ? _items[item.Type].Count : 0;
            }
        }

        private void ClearOtherSelections(string keepSelection)
        {
            if (keepSelection != "Scene3DMaterial" && _selectedScene3DMaterialItem != null)
            {
                _selectedScene3DMaterialItem = null;
                OnPropertyChanged(nameof(SelectedScene3DMaterialItem));
            }

            if (keepSelection != "Scene3DNode" && _selectedScene3DNodeItem != null)
            {
                _selectedScene3DNodeItem = null;
                OnPropertyChanged(nameof(SelectedScene3DNodeItem));
            }

            if (keepSelection != "Scene2DMaterial" && _selectedScene2DMaterialItem != null)
            {
                _selectedScene2DMaterialItem = null;
                OnPropertyChanged(nameof(SelectedScene2DMaterialItem));
            }

            if (keepSelection != "Scene2DNode" && _selectedScene2DNodeItem != null)
            {
                _selectedScene2DNodeItem = null;
                OnPropertyChanged(nameof(SelectedScene2DNodeItem));
            }
        }

        private TypeListViewItem GetSelectedItem()
        {
            return _selectedScene3DMaterialItem
                ?? _selectedScene3DNodeItem
                ?? _selectedScene2DMaterialItem
                ?? _selectedScene2DNodeItem;
        }

        private void UpdateElementsList()
        {
            // We are reporting that we are performing an internal update
            _isInternalUpdate = true;

            try
            {
                Elements.Clear();

                SelectedElement = null;

                var selectedItem = GetSelectedItem();
                if (selectedItem == null) return;

                if (_items != null && _items.ContainsKey(selectedItem.Type))
                {
                    IEnumerable<RESElement> elementsToShow;

                    if (selectedItem.FilterType != null)
                    {
                        elementsToShow = _items[selectedItem.Type]
                            .Where(e => e.GetType() == selectedItem.FilterType);
                    }
                    else
                    {
                        elementsToShow = _items[selectedItem.Type];
                    }

                    foreach (var element in elementsToShow)
                    {
                        Elements.Add(element);
                    }

                    if (Elements.Count > 0)
                    {
                        SelectedElement = Elements[0];
                    }
                }
            }
            finally
            {
                // We disable the flag no matter what happens.
                _isInternalUpdate = false;
            }
        }

        private void AddElement(object parameter)
        {
            var selectedItem = GetSelectedItem();
            if (selectedItem == null) return;

            // Create the element with the correct type
            RESElement newElement = CreateNewElement(selectedItem.Type, selectedItem.FilterType);

            Elements.Add(newElement);
        }

        private void DeleteElement(object parameter)
        {
            if (SelectedElement == null) return;

            Elements.Remove(SelectedElement);
        }

        private RESElement CreateNewElement(RESType type, Type filterType)
        {
            switch (type)
            {
                case RESType.TextureData:
                    // Create according to the type filter
                    if (filterType == typeof(XRESTextureData))
                    {
                        return new XRESTextureData { Name = "NewTexture" };
                    }
                    else
                    {
                        return new RESTextureData { Name = "NewTexture" };
                    }

                case RESType.MaterialData:
                    return new ResMaterialData
                    {
                        Name = "NewMaterial",
                        Images = new RESImageEntry[4]
                    };

                default:
                    return new RESElement { Name = $"New{type}" };
            }
        }

        private void OnElementsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_isInternalUpdate) return;

            var selectedItem = GetSelectedItem();
            if (selectedItem == null || _items == null) return;

            if (!_items.ContainsKey(selectedItem.Type))
            {
                _items[selectedItem.Type] = new List<RESElement>();
            }

            var list = _items[selectedItem.Type];

            // Manage additions (via DataGrid or command)
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (RESElement item in e.NewItems)
                {
                    if (!list.Contains(item))
                    {
                        list.Add(item);
                    }
                }
            }

            // Manage deletions
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                foreach (RESElement item in e.OldItems)
                {
                    if (list.Contains(item))
                    {
                        list.Remove(item);
                    }
                }
            }

            // Manage Reset
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                // Todo?
            }

            RefreshAllLists();
        }

        public RESElement CreateItemForCurrentContext()
        {
            var selectedItem = GetSelectedItem();

            if (selectedItem == null) return new RESElement();

            return CreateNewElement(selectedItem.Type, selectedItem.FilterType);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}