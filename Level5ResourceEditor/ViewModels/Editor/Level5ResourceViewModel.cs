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
                _elements = value;
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

        public ICommand AddElementCommand { get; private set; }
        public ICommand DeleteElementCommand { get; private set; }

        public Level5ResourceViewModel()
        {
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
                (RESType.Material1, null, "Material1"),
                (RESType.Material2, null, "Material2"),
                (RESType.TextureData, typeof(RESTextureData), "TextureData (RES)"),
                (RESType.TextureData, typeof(XRESTextureData), "TextureData (XRES)"),
                (RESType.MaterialData, null, "MaterialData")
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
                RESType.MeshName,
                RESType.Bone,
                RESType.AnimationMTN2,
                RESType.AnimationMTN3,
                RESType.AnimationIMN2,
                RESType.AnimationMTM2,
                RESType.Shading,
                RESType.Properties,
                RESType.MTNINF,
                RESType.MTNINF2,
                RESType.IMMINF,
                RESType.MTMINF,
                RESType.Textproj
            };

            Scene3DNodeItems.Clear();
            foreach (var type in scene3DNodeTypes)
            {
                if (!_items.ContainsKey(type))
                {
                    _items[type] = new List<RESElement>();
                }

                Scene3DNodeItems.Add(new TypeListViewItem
                {
                    DisplayName = type.ToString(),
                    Type = type,
                    FilterType = null,
                    ElementCount = _items[type].Count
                });
            }

            // Initialize other standard types
            foreach (var type in scene3DMaterialTypes.Where(t => t.Item2 == null).Select(t => t.Item1).Distinct())
            {
                if (!_items.ContainsKey(type))
                {
                    _items[type] = new List<RESElement>();
                }
            }
        }

        public void LoadFile(string filePath)
        {
            IResource resource = Resourcer.GetResource(File.ReadAllBytes(filePath));

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
            if (keepSelection != "Scene3DMaterial")
                _selectedScene3DMaterialItem = null;

            if (keepSelection != "Scene3DNode")
                _selectedScene3DNodeItem = null;

            if (keepSelection != "Scene2DMaterial")
                _selectedScene2DMaterialItem = null;

            if (keepSelection != "Scene2DNode")
                _selectedScene2DNodeItem = null;
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
            Elements.Clear();

            var selectedItem = GetSelectedItem();
            if (selectedItem == null)
                return;

            if (_items != null && _items.ContainsKey(selectedItem.Type))
            {
                IEnumerable<RESElement> elementsToShow;

                if (selectedItem.FilterType != null)
                {
                    // Filter by specific type (RESTextureData or XRESTextureData)
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
            }
        }

        private void AddElement(object parameter)
        {
            var selectedItem = GetSelectedItem();
            if (selectedItem == null || _items == null)
                return;

            RESElement newElement = CreateNewElement(selectedItem.Type, selectedItem.FilterType);

            if (!_items.ContainsKey(selectedItem.Type))
            {
                _items[selectedItem.Type] = new List<RESElement>();
            }

            _items[selectedItem.Type].Add(newElement);
            Elements.Add(newElement);

            RefreshAllLists();
        }

        private void DeleteElement(object parameter)
        {
            if (SelectedElement == null)
                return;

            var selectedItem = GetSelectedItem();
            if (selectedItem == null || _items == null)
                return;

            if (_items.ContainsKey(selectedItem.Type))
            {
                _items[selectedItem.Type].Remove(SelectedElement);
                Elements.Remove(SelectedElement);

                RefreshAllLists();
            }
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}