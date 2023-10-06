using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class TileGroupController
{
    private DragAndDropController dragAndDropController { get;}
    private VisualTreeAsset gridElement { get;}
    private IntegerField rowCountField { get;}
    private IntegerField colCountField { get;}
    private ScrollView tilesScrollView { get;}
    private Action<int> onClickCallback { get;}
    private Action<int> onRemoveCallback { get;}    
    private List<VisualElement> gridElementList = new List<VisualElement>();
    private VisualElement tilesGroupRootPanel;
    private VisualElement elementSample;

    public TileGroupController(TileGroupControllerModel _model)
    {
        rowCountField = _model.RootElement.Q<IntegerField>("RowCountField");
        colCountField = _model.RootElement.Q<IntegerField>("ColCountField");
        gridElement = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/TileButtonGroup.uxml");
        tilesScrollView = _model.RootElement.Q<ScrollView>("TilesScrollView");
        onClickCallback = _model.OnClickCallback;
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

        var templateContainer = gridElement.Instantiate();
        templateContainer.style.position = new StyleEnum<Position>(Position.Absolute);
        elementSample = templateContainer.Q<VisualElement>("TileButtonGroup");
        foreach(var element in elementSample.Children())
        {
            element.style.display = DisplayStyle.None;
        }
        var tileGroup = _model.RootElement.Q<VisualElement>("TileGroup");
        tileGroup.Add(templateContainer);

        dragAndDropController = new DragAndDropController(new DragAndDropControllerModel()
        {
            ScrollView = _model.RootElement.Q<ScrollView>("DefaultSlotsScrollView"),
            DragAndDropSprites = _model.DragAndDropSprites,
            SearchSlotRoot = tileGroup,
            DefaultSlotClassName = "draggable-object-default-slot",
            DraggableObjectClassName = "draggable-object",
            SlotClassName = "slot",
        });
    }

    public void SetGrid(int _row, int _col)
    {
        tilesGroupRootPanel.Clear();
        tilesGroupRootPanel.style.width = _col * elementSample.resolvedStyle.width;
        int tilesCount = _row * _col;

        for(short index = 0; index < tilesCount; index++)
        {
            var tileElement = gridElement.Instantiate();
            gridElement.name = index.ToString();
            Button btn = tileElement.Q<Button>("AddTileButton");
            Button deleteBtn = tileElement.Q<Button>("DeleteButton");
            VisualElement tile = tileElement.Q<VisualElement>("Tile");
            tile.AddToClassList("slot");

            int tileIndex = index;

            tilesGroupRootPanel.Add(tileElement);
            gridElementList.Add(tileElement);
        }

        rowCountField.value = _row;
        colCountField.value = _col;
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
}

public class TileGroupControllerModel
{
    public VisualElement RootElement { get; set;}
    public Sprite[] DragAndDropSprites { get; set;}
    public Action<int> OnClickCallback { get; set;}
    public Action<int> OnRemoveCallback { get; set;}
}
