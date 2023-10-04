using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.Linq;

public class TripleTileGameDesginEditorWindow : EditorWindow
{
    private class TileLayers
    {
        public List<TileLayerEditor> Layers; 
        public TileLayers()
        {
            this.Layers = new List<TileLayerEditor>();
        }
    }

    public class TileLayerEditor
    {
        public List<Tile> Tiles = new List<Tile>();
        public ushort RowCountX;
        public ushort ColCountY;
        public ushort MatchCount;
    }

    static TripleTileGameDesginEditorWindow wnd;
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;
    private TileLayers tileLayers;
    private ObjectField tileLayersSOObjField;

    private ListView listView;
    private Button showAllLayersButton;
    private VisualTreeAsset listViewElement;

    private IntegerField rowCountField;
    private IntegerField colCountField;

    private VisualTreeAsset tileButtonGroup;
    private ScrollView tilesScrollPanel;
    private bool refreshTilesPanel = true;
    private List<VisualElement> tileElementList = new List<VisualElement>();


    [MenuItem("三消遊戲編輯器/TripleTileGameDesginEditorWindow")]
    public static void ShowExample()
    {
        wnd = GetWindow<TripleTileGameDesginEditorWindow>();
        wnd.titleContent = new GUIContent("三消遊戲編輯器");
    }

    public void CreateGUI()
    {
        m_VisualTreeAsset.CloneTree(rootVisualElement);
        tileLayers = new TileLayers();
        tileLayersSOObjField = rootVisualElement.Q<ObjectField>("TileLayersSO");
        tileLayersSOObjField.objectType = typeof(TileLayersSO); 
        rootVisualElement.Q<Button>("ImportButton").RegisterCallback<ClickEvent>(onImportTempleteButton);

        rootVisualElement.Q<Button>("AddLayerButton").clickable.clicked += addLayer;
        rootVisualElement.Q<Button>("RemoveSelectedLayerButton").clickable.clicked += deleteOnSelectLayer;
        listView = rootVisualElement.Q<ListView>("ListView");

        showAllLayersButton = rootVisualElement.Q<Button>("ShowAllLayersButton");
        showAllLayersButton.RegisterCallback<ClickEvent>(onShowAllTileLayersButton);
        listViewElement = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/ListViewElement.uxml");
        initListViewPanel();

        tileButtonGroup = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/TileButtonGroup.uxml");
        tilesScrollPanel = rootVisualElement.Q<ScrollView>("TilesScrollPanel");
        rowCountField = rootVisualElement.Q<IntegerField>("RowCountField");
        colCountField = rootVisualElement.Q<IntegerField>("ColCountField");
        rowCountField.value = 0;
        colCountField.value = 0;
        rowCountField.RegisterValueChangedCallback((evt) => limitRowColValue(evt, true));
        colCountField.RegisterValueChangedCallback((evt) => limitRowColValue(evt, false));
        //DragAndDropManipulator manipulator = new(rootVisualElement.Q<VisualElement>("object"));

    }

    private void addLayer()
    {
        int selectId = listView.selectedIndex;
        tileLayers.Layers.Insert( 0 ,new TileLayerEditor() { RowCountX = 5, ColCountY = 5});
        if( tileLayers.Layers.Count > 1)
            refreshTilesPanel = false;
        listView.Rebuild();
        if(selectId != -1)
            listView.selectedIndex = selectId + 1;
        else
            refreshTilesPanel = true;
    }

    private void deleteOnSelectLayer()
    {
        var selectedListIndex = listView.selectedIndex;
        if(selectedListIndex < 0)
            return;

        tilesScrollPanel.Clear();
        tileLayers.Layers.RemoveAt(selectedListIndex);
        listView.Rebuild();  
        listView.selectedIndex = -1;
    }

    private void onImportTempleteButton(ClickEvent _clickEvent)
    {
        if(tileLayersSOObjField.value == null)
            return;

        TileLayersSO tileLayersSO = tileLayersSOObjField.value as TileLayersSO;
        tileLayers.Layers.Clear();
        var layerCount = tileLayersSO.TileLayers.Length;
        for(int index = layerCount - 1; index >= 0; index--)
        {
            var data = tileLayersSO.TileLayers[index];
            tileLayers.Layers.Add(new TileLayerEditor
            {
                RowCountX = data.RowCountX,
                ColCountY = data.ColCountY,
                Tiles = data.Tiles.ToList(),
                MatchCount = data.MatchCount
            });
        }

        tilesScrollPanel.Clear();
        listView.Rebuild();
    }

    private void initListViewPanel()
    {
        listView.itemsSource = tileLayers.Layers;
        listView.makeItem = makeListItem;
        listView.bindItem = bindListItem;
        listView.selectionChanged += listOnClick;
    }

    private void onShowAllTileLayersButton(ClickEvent _clickEvent)
    {
        listView.selectedIndex = -1;
    }

    private VisualElement makeListItem()
    {
       TemplateContainer templateContainer = listViewElement.Instantiate();
       return templateContainer;
    }

    private void bindListItem(VisualElement _ve, int _index)
    {
        Label label = _ve.Q<Label>("LabelContent");
        var data = tileLayers.Layers[_index];
        //Debug.Log("list index: " + _index + ", tileLayerIndex: " + tileLayerIndex + ", row: " + data.RowCountX + ", col: " + data.ColCountY);
        label.text = string.Format("第 {0} 層\n行: {1}, 列: {2}", getTileLayerIndex(_index), data.RowCountX, data.ColCountY);
    }

    private int getTileLayerIndex(int _listViewIndex)
    {
        return  tileLayers.Layers.Count - _listViewIndex;
    }

    private void listOnClick(IEnumerable<object> _objs)
    {
        Debug.Log(refreshTilesPanel);
        if(!refreshTilesPanel)
        {
            refreshTilesPanel = true;
            return;
        }

        var selectedIndex = listView.selectedIndex;
       
        if(selectedIndex >= 0)
            setTilesPanel(tileLayers.Layers[selectedIndex]);
    }

    void setTilesPanel(TileLayerEditor _layerData)
    {
        tilesScrollPanel.Clear();
        VisualElement tilesRoot = new VisualElement();
        tilesRoot.style.width = _layerData.ColCountY * 65;
        tilesRoot.style.flexGrow = 0;
        tilesRoot.style.flexDirection = FlexDirection.Row;
        tilesRoot.style.flexWrap = Wrap.Wrap;
        tilesRoot.style.alignItems = Align.FlexEnd;
        tilesRoot.style.justifyContent = Justify.FlexStart;

        VisualElement content = tilesScrollPanel.Q<VisualElement>("unity-content-container");
        content.style.flexDirection = FlexDirection.Row;
        content.style.flexWrap = Wrap.Wrap;
        content.style.alignItems = Align.Center;
        content.style.alignSelf = Align.Center;
        content.style.justifyContent = Justify.Center;

        content = tilesScrollPanel.Q<VisualElement>("unity-content-viewport");
        content.style.alignSelf = Align.Center;

        int tilesCount = _layerData.RowCountX * _layerData.ColCountY;
        tileElementList.Clear();

        for(short index = 0; index < tilesCount; index++)
        {
            var templateContainer = tileButtonGroup.Instantiate();
            templateContainer.name = index.ToString();
            Button btn = templateContainer.Q<Button>("AddTileButton");
            Button deleteBtn = templateContainer.Q<Button>("DeleteButton");
            IntegerField idInputField = templateContainer.Q<IntegerField>("IdInputField");

            int tileIndex = index;

            btn.clicked += ()=> onCreateTileButton(tileIndex);
            deleteBtn.clicked += ()=> onDeleteTileButton(tileIndex);

            tilesRoot.Add(templateContainer);
            tileElementList.Add(templateContainer);
        }

        if(_layerData.Tiles.Count > 0)
        {
            for(short tileIndex = 0; tileIndex < _layerData.Tiles.Count; tileIndex++)
            {
                var index = getTileElementIndex(_layerData.ColCountY, _layerData.Tiles[tileIndex].RowX, _layerData.Tiles[tileIndex].ColY);
                if(tileElementList.Count > index)
                {
                    var element = tileElementList[index];
                    element.Q<VisualElement>("Tile").style.display = DisplayStyle.Flex;
                }
            }
        }

        tilesScrollPanel.Add(tilesRoot);
        rowCountField.value = _layerData.RowCountX;
        colCountField.value = _layerData.ColCountY;
    }

    private void onCreateTileButton(int _index)
    {
        tileElementList[_index].Q<VisualElement>("Tile").style.display = DisplayStyle.Flex;
        var layer = tileLayers.Layers[listView.selectedIndex];
        int row = _index/layer.ColCountY;
        int col = _index%layer.ColCountY;
        Tile tile = new Tile();
        tile.RowX = (ushort)row;
        tile.ColY = (ushort)col;
        layer.Tiles.Add(tile);
    }

    private void onDeleteTileButton(int _index)
    {
        tileElementList[_index].Q<VisualElement>("Tile").style.display = DisplayStyle.None;
        var layer = tileLayers.Layers[listView.selectedIndex];

        int row = _index/layer.ColCountY;
        int col = _index%layer.ColCountY;

        foreach(var tile in layer.Tiles)
        {
            if(tile.RowX == row && tile.ColY == col)
            {
                layer.Tiles.Remove(tile);
            }
        }
    }

    private int getTileElementIndex(int _maxCol, int _row, int _col)
    {
        return (_row * _maxCol) + _col;
    }

    private void limitRowColValue(ChangeEvent<int> _inputValue, bool _isRow)
    {
        var value = Mathf.Clamp(_inputValue.newValue, 1, 10);
        var listIndex = listView.selectedIndex;
        var field = _isRow? rowCountField: colCountField;
        field.value = value;

        if(listIndex >= 0)
        {
            if(_isRow)
                tileLayers.Layers[listIndex].RowCountX = (ushort)value;
            else
                tileLayers.Layers[listIndex].ColCountY = (ushort)value;

            setTilesPanel(tileLayers.Layers[listIndex]);
        }
        
        listView.Rebuild();
    }
}
