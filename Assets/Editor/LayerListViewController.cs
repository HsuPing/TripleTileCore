using System;
using UnityEditor;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

public class LayerListViewController
{
    public int GetLayerIndex => selectedListIndex;
    private VisualTreeAsset listElement { get;}
    private ListView listView { get;}
    private Button addLayerButton { get;}
    private Button removeLayerButton { get;}
    private Button showAllButton { get;}
    private Action<int> listItemClickCallback { get;}
    private Func<int, string> getLayerInfo { get;}
    private Action addLayerCallback { get;}
    private Action<int> removeLayerCallback { get;}

    private int selectedListIndex;

    public LayerListViewController(LayerListViewControllerModel _model)
    {
        listView = _model.RootElemnt.Q<ListView>("ListView");
        addLayerButton = _model.RootElemnt.Q<Button>("AddLayerButton");
        removeLayerButton = _model.RootElemnt.Q<Button>("RemoveLayerButton");
        showAllButton = _model.RootElemnt.Q<Button>("ShowAllButton");
        listElement = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/ListViewElement.uxml");
        listItemClickCallback = _model.ListItemClickCallback;
        getLayerInfo = _model.GetLayerInfoCallback;
        addLayerCallback = _model.AddLayerCallback;
        removeLayerCallback = _model.RemoveLayerCallback;

        addLayerButton.clickable.clicked += addLayer;
        removeLayerButton.clickable.clicked += removeLayer;

        selectedListIndex = -1;
        listView.selectedIndex = selectedListIndex;
        listView.makeItem = makeListItem;
        listView.bindItem = bindListItem;
        listView.itemsSource = _model.ItemSource;
        listView.selectionChanged += listOnClick;
    }

    public void Rebuild(int _selectIndex = int.MaxValue)
    {
        if(_selectIndex != int.MaxValue)
            setSelectIndex(_selectIndex);
        listView.Rebuild();
    }

    private VisualElement makeListItem()
    {
       TemplateContainer templateContainer = listElement.Instantiate();
       return templateContainer;
    }

    private void bindListItem(VisualElement _element, int _index)
    {
        Label label = _element.Q<Label>("LabelContent");
        label.text = getLayerInfo?.Invoke(_index);
    }

    private void listOnClick(IEnumerable<object> _objs)
    {
        selectedListIndex = listView.selectedIndex;
       
        if(selectedListIndex >= 0)
            listItemClickCallback?.Invoke(selectedListIndex);
    }

    private void addLayer()
    {
        addLayerCallback?.Invoke();
        listView.Rebuild();
        if(selectedListIndex == -1)
            return;

        setSelectIndex(selectedListIndex + 1);
    }

    private void removeLayer()
    {
        if(selectedListIndex < 0)
            return;

        removeLayerCallback?.Invoke(selectedListIndex);
        listView.Rebuild();  
        setSelectIndex(-1);
    }

    private void setSelectIndex(int _index)
    {
        selectedListIndex = _index;
        listView.selectedIndex = selectedListIndex;
    }
}

public class LayerListViewControllerModel
{
    public VisualElement RootElemnt { get; set;}
    public IList ItemSource { get; set;}
    public Func<int, string> GetLayerInfoCallback { get; set;}
    public Action<int> ListItemClickCallback { get; set;}
    public Action AddLayerCallback { get; set;}
    public Action<int> RemoveLayerCallback { get; set;}
}
