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
        public List<TileLayer> Layers; 
        public TileLayers()
        {
            this.Layers = new List<TileLayer>();
        }
    }

    static TripleTileGameDesginEditorWindow wnd;
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;
    private TileLayers tileLayers;
    private ObjectField tileLayersSOObjField;

    private ListView listView;
    private Button showAllLayersButton;
    private VisualTreeAsset listViewElement;
    private Dictionary<int, VisualElement> listViewVisualElementPairs = new Dictionary<int, VisualElement>();


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
    }

    private void addLayer()
    {
        tileLayers.Layers.Insert( 0 ,new TileLayer() { RowCountX = 8, ColCountY = 8});
        listView.Rebuild();
    }

    private void deleteOnSelectLayer()
    {
        var selectedListIndex = listView.selectedIndex;
        if(selectedListIndex < 0)
            return;
        
        tileLayers.Layers.RemoveAt(selectedListIndex);
        listView.Rebuild();
    }

    private void onImportTempleteButton(ClickEvent _clickEvent)
    {
        if(tileLayersSOObjField.value == null)
            return;
        Debug.Log("Import Template.");
        TileLayersSO tileLayersSO = tileLayersSOObjField.value as TileLayersSO;
        tileLayers.Layers.Clear();
        var layerCount = tileLayersSO.TileLayers.Length;
        for(int index = layerCount - 1; index >= 0; index--)
        {
            tileLayers.Layers.Add(tileLayersSO.TileLayers[index]);
        }

        listView.Rebuild();
    }

    private void initListViewPanel()
    {
        listView.itemsSource = tileLayers.Layers;
        listView.makeItem = makeListItem;
        listView.bindItem = bindListItem;
        //listView.selectionChanged += onClick;
        listView.itemIndexChanged += listItemIndexChanged;
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
        /*假設有4層
            0 -> 第四關 tileLayers.Layers.Count
        */
        return  tileLayers.Layers.Count - _listViewIndex;
    }

    private void unbindListItem(VisualElement _ve, int _index)
    {
        Debug.Log("unbindList " + _ve.name + " ,index: " + _index);
    }

    private void destroyListItem(VisualElement _ve)
    {
        Debug.Log("destroy " + _ve.name);
    }

    private void deleteListElement(ClickEvent _clickEvent, int index)
    {
        Debug.Log("deleteListElement");
    }

    private void listViewItemAdd(IEnumerable<int> _index)
    {
        Debug.Log("Item Add" + _index.First());
    }

    private void listViewItemRemove(IEnumerable<int> _index)
    {
        Debug.Log("Item Remove " + _index.First());
    }

    private void listItemIndexChanged(int _index1, int _index2)
    {
        Debug.Log(_index1 + ", " + _index2);
        // var index1 = getTileLayerIndex(_index1);
        // var index2 = getTileLayerIndex(_index2);
        // Debug.Log("轉換" + index1 + ", " + index2);
        // TileLayer tileLayer = tileLayers.Layers[index2];
        // tileLayers.Layers[index2] = tileLayers.Layers[index1];
        // tileLayers.Layers[index1] = tileLayer;
        //listView.Rebuild();
    }

    private void onClick(IEnumerable<object> _objs)
    {
        // var selectedIndex = listView.selectedIndex;
        // var element = listViewVisualElementPairs[selectedIndex];
        //Debug.Log(onClick);
    }

    private void onClick()
    {
        Debug.Log("onClick");
    }
}
