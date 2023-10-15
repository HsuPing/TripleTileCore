using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace TripleTileGameDesginEditor
{
    public class TileGroupController
    {
        public class TileElement
        {
            public VisualElement Root { get; set;}

            public TileElement(VisualTreeAsset _tileVisualTreeAsset, int _index, Action<int> _addCallback, Action<int> _removeCallback)
            {
                var tileTemplate = _tileVisualTreeAsset.Instantiate();
                tileTemplate.name = _index.ToString();
                Button btn = tileTemplate.Q<Button>("AddTileButton");
                btn.style.display = DisplayStyle.Flex;
                Root = tileTemplate.Q<VisualElement>("Tile");

                btn.clickable.clicked += () => 
                {
                    OpenPanelActive(true);
                    _addCallback(_index); 
                };

                Button deleteBtn = tileTemplate.Q<Button>("DeleteButton");
                deleteBtn.clickable.clicked += () => 
                {
                    OpenPanelActive(false);
                    _removeCallback(_index);
                    Root.Q<VisualElement>("TileOpen").Clear();
                };

                VisualElement tileOpen = tileTemplate.Q<VisualElement>("TileOpen");
                tileOpen.AddToClassList("slot");

                OpenPanelActive(false);

            }

            public void OpenPanelActive(bool _open)
            {
                VisualElement tileOpenPanel = Root.Q<VisualElement>("TileOpenPanel");
                tileOpenPanel.style.display = _open ? DisplayStyle.Flex: DisplayStyle.None;
            }
        }

        private DragAndDropController dragAndDropController { get;}
        private VisualTreeAsset tileVisualTreeAsset { get;}
        private IntegerField rowCountField { get;}
        private IntegerField colCountField { get;}
        private ScrollView tilesScrollView { get;}
        private Action<int> onAddCallback { get;}
        private Action<int> onRemoveCallback { get;}    
        private Action<int> onRowCountChange { get;}
        private Action<int> onColCountChange { get;}
        private Action<int, int> setTileIdCallback { get;}
        private List<TileElement> tileElementList = new List<TileElement>();
        private VisualElement tilesGroupRootPanel;
        private VisualElement elementSample;
  
        public TileGroupController(TileGroupControllerModel _model)
        {
            rowCountField = _model.RootElement.Q<IntegerField>("RowCountField");
            colCountField = _model.RootElement.Q<IntegerField>("ColCountField");
            tileVisualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/TileButtonGroup.uxml");
            tilesScrollView = _model.RootElement.Q<ScrollView>("TilesScrollView");
            onAddCallback = _model.OnAddCallback;
            onRemoveCallback = _model.OnRemoveCallback;

            VisualElement content = tilesScrollView.Q<VisualElement>("unity-content-container");
            content.style.flexDirection = FlexDirection.Row;
            content.style.flexWrap = Wrap.Wrap;
            content.style.alignItems = Align.Center;
            content.style.alignSelf = Align.Center;
            content.style.justifyContent = Justify.Center;
            content = tilesScrollView.Q<VisualElement>("unity-content-viewport");
            content.style.alignSelf = Align.Center;

            tilesGroupRootPanel = setTilesPanel();
            tilesScrollView.Add(tilesGroupRootPanel);

            onRowCountChange = _model.OnRowCountChange;
            onColCountChange = _model.OnColCountChange;
            rowCountField.value = 0;
            colCountField.value = 0;
            rowCountField.RegisterValueChangedCallback((evt) => intputRowColValueChange(evt, true));
            colCountField.RegisterValueChangedCallback((evt) => intputRowColValueChange(evt, false));

            setTileIdCallback = _model.SetTileId;

            var templateContainer = tileVisualTreeAsset.Instantiate();
            elementSample = templateContainer.Q<VisualElement>("Tile");
            foreach(var element in elementSample.Children())
            {
                element.style.display = DisplayStyle.None;
            }
            var tileGroup = _model.RootElement.Q<VisualElement>("TileGroup");
            tileGroup.Add(elementSample);

            dragAndDropController = new DragAndDropController(new DragAndDropControllerModel()
            {
                ScrollView = _model.RootElement.Q<ScrollView>("DefaultSlotsScrollView"),
                DragAndDropSprites = _model.DragAndDropSprites,
                SearchSlotRoot = tileGroup,
                DefaultSlotClassName = "draggable-object-default-slot",
                DraggableObjectClassName = "draggable-object",
                SlotClassName = "slot",
                DefaultSlotCountClassName = "default-slot-count",
                DragInSlotCallback = (element, value) => setTileId(element, value),  
                RemoveFromSlotCallback = (element) => setTileId(element, 0),  
            });
        }

        public void SetGrid(int _row, int _col)
        {
            RemoveTilePanel();
            tilesGroupRootPanel.style.width = _col * elementSample.resolvedStyle.width;
            int tilesCount = _row * _col;

            for(short index = 0; index < tilesCount; index++)
            {
                TileElement tile = new TileElement(tileVisualTreeAsset, index, onAddCallback, onRemoveCallback);
                tileElementList.Add(tile);
                tilesGroupRootPanel.Add(tile.Root);
            }

            rowCountField.value = _row;
            colCountField.value = _col;
        }

        public void SetData(List<Tile> _dataList, int _maxRow, int _maxCol)
        {
            for(short index = 0; index < _dataList.Count; index++)
            {
                var tileData = _dataList[index];
                var tileIndex = TripleTileGameDesginProcesser.GetTileElementIndex(_maxCol, tileData.RowY, tileData.ColX);

                if(tileData.RowY <= _maxRow - 1 && tileData.ColX <= _maxCol - 1)
                {
                    var element = tileElementList[tileIndex];
                    element.OpenPanelActive(true);
                    if(tileData.Id > 0)
                    {
                        var dragAndDropElement = dragAndDropController.CreateNewDragObject(tileData.Id - 1);
                        element.Root.Q<VisualElement>("TileOpen").Add(dragAndDropElement);
                        dragAndDropController.AddDragAndDrop(dragAndDropElement, tileData.Id);
                    }
                }
            }
        }

        public void RemoveTilePanel()
        {
            tilesGroupRootPanel.Clear();
            tileElementList.Clear();
        }

        private VisualElement setTilesPanel()
        {
            VisualElement element = new VisualElement();
            element.name = "TilesGroupRootPanel";
            element.style.flexGrow = 0;
            element.style.flexDirection = FlexDirection.Row;
            element.style.flexWrap = Wrap.Wrap;
            element.style.alignItems = Align.FlexEnd;
            element.style.justifyContent = Justify.FlexStart;
            return element;
        }

        private void intputRowColValueChange(ChangeEvent<int> _changeValue, bool _isRow)
        {
            var value = Mathf.Clamp(_changeValue.newValue, 1, 10);
            if(_isRow)
            {
                rowCountField.value = value;
                onRowCountChange.Invoke(value);
            }
            else
            {
                colCountField.value = value;
                onColCountChange.Invoke(value);
            }
        }

        private void setTileId(VisualElement _element, int _value)
        {
            var parent = getTileFromChild(_element);
            if(parent == null)
                return;
            var index = tilesGroupRootPanel.IndexOf(parent);
            setTileIdCallback(index, _value);
        }

        private VisualElement getTileFromChild(VisualElement _element)
        {
            VisualElement element = _element.parent;

            for(short index = 0; index < 10; index++)
            {
                if(element.name != "Tile")
                    element = element.parent;
                else
                    break;
            }
            if(element.name == "Tile")
                return element;
            return null;
        }

        public void SetTileCountAmount(int _index, int _value)
        {
            dragAndDropController.SetTileCountAmount(_index, _value);
        }
    }

    public class TileGroupControllerModel
    {
        public VisualElement RootElement { get; set;}
        public Sprite[] DragAndDropSprites { get; set;}
        public Action<int> OnAddCallback { get; set;}
        public Action<int> OnRemoveCallback { get; set;}
        public Action<int> OnRowCountChange { get; set;}
        public Action<int> OnColCountChange { get; set;}
        public Action<int, int> SetTileId { get; set;}
    }
}


