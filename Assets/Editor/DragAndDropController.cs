using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DragAndDropController
{
    DragAndDropControllerModel model { get;}
    public Action<VisualElement, int> DragInSlotCallback => model.DragInSlotCallback;
    public Action<VisualElement> RemoveFromSlotCallback => model.RemoveFromSlotCallback;
    private List<VisualElement> defautSlotList = new List<VisualElement>();

    public DragAndDropController(DragAndDropControllerModel _model)
    {
        model = _model;

        var emptyDefaultSlot = createDefaultSlot("0");
        Label countLabel = new Label();
        countLabel.name = model.DefaultSlotCountClassName;
        countLabel.AddToClassList(model.DefaultSlotCountClassName);
        countLabel.style.display = DisplayStyle.None;
        emptyDefaultSlot.Add(countLabel);
        model.ScrollView.Add(emptyDefaultSlot);
        defautSlotList.Add(emptyDefaultSlot);

        for(int id = 1; id <= model.DragAndDropSprites.Length; id++)
        {
            var defaultSlot = createDefaultSlot(id.ToString());
            var draggableObject = CreateNewDragObject(id - 1);   
            defaultSlot.Add(draggableObject);   

            countLabel = new Label();
            countLabel.name = model.DefaultSlotCountClassName;
            countLabel.AddToClassList(model.DefaultSlotCountClassName);
            countLabel.style.display = DisplayStyle.None;

            defaultSlot.Add(countLabel);
            model.ScrollView.Add(defaultSlot);
            defautSlotList.Add(defaultSlot);
            DragAndDropManipulator dragAndDropManipulator = new(this, draggableObject, model.SearchSlotRoot, id);
        }
    }

    public void AddDragAndDrop(VisualElement _element, ushort _id)
    {
        DragAndDropManipulator dragAndDropManipulator = new(this, _element, model.SearchSlotRoot, _id, false);
    }

    private VisualElement createDefaultSlot(string _name)
    {
        VisualElement defaultSlot = new VisualElement();
        defaultSlot.AddToClassList(model.DefaultSlotClassName);
        defaultSlot.name = _name;
        return defaultSlot;
    }

    public VisualElement CreateNewDragObject(int _spriteIndex)
    {
        VisualElement draggableObject = new VisualElement();
        draggableObject.AddToClassList(model.DraggableObjectClassName);
        draggableObject.style.backgroundImage = new StyleBackground(model.DragAndDropSprites[_spriteIndex]);
        return draggableObject;
    }

    public void CreateDefaultTile(int _id)
    {
        var draggableObject = CreateNewDragObject(_id - 1);   
        defautSlotList[_id].Add(draggableObject);
        DragAndDropManipulator dragAndDropManipulator = new(this, draggableObject, model.SearchSlotRoot, _id);
    }

    public void SetTileCountAmount(int _index, int _value)
    {
        Label countLable = defautSlotList[_index].Q<Label>(model.DefaultSlotCountClassName);
        countLable.style.display = _value > 0? DisplayStyle.Flex: DisplayStyle.None;
        countLable.text = _value.ToString();
    }
}

public class DragAndDropControllerModel
{
    public ScrollView ScrollView { get; set;}
    public Sprite[] DragAndDropSprites { get; set;}
    public VisualElement SearchSlotRoot { get; set;}
    public string DefaultSlotClassName { get; set;}
    public string DraggableObjectClassName { get; set;}
    public string SlotClassName { get; set;}
    public string DefaultSlotCountClassName { get; set;}
    public Action<VisualElement, int> DragInSlotCallback { get; set;}
    public Action<VisualElement> RemoveFromSlotCallback { get; set;}
    public Action<int, int> SetDefaultSlotCount { get; set;}
}
