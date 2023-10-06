using System;
using UnityEngine;
using UnityEngine.UIElements;

public class DragAndDropController
{
    DragAndDropControllerModel model { get;}
    public Action<int, int> DragInSlotCallback => model.DragInSlotCallback;
    public Action<int> RemoveFromSlotCallback => model.RemoveFromSlotCallback;

    public DragAndDropController(DragAndDropControllerModel _model)
    {
        model = _model;

        for(int id = 1; id <= model.DragAndDropSprites.Length; id++)
        {
            var defaultSlot = createDefaultSlot(id.ToString());
            var draggableObject = createNewDragObject(id - 1);   
            defaultSlot.Add(draggableObject);   
            model.ScrollView.Add(defaultSlot);
            DragAndDropManipulator dragAndDropManipulator = new(this, draggableObject, model.SearchSlotRoot, id);
        }
    }
   
    public VisualElement CreateNewDragObject(int _id)
    {
        var element = createNewDragObject(_id - 1);
        DragAndDropManipulator dragAndDropManipulator = new(this, element, model.SearchSlotRoot, _id, false);
        return element;
    }

    private VisualElement createDefaultSlot(string _name)
    {
        VisualElement defaultSlot = new VisualElement();
        defaultSlot.AddToClassList(model.DefaultSlotClassName);
        defaultSlot.name = _name;
        return defaultSlot;
    }

    private VisualElement createNewDragObject(int _spriteIndex)
    {
        VisualElement draggableObject = new VisualElement();
        draggableObject.AddToClassList(model.DraggableObjectClassName);
        draggableObject.style.backgroundImage = new StyleBackground(model.DragAndDropSprites[_spriteIndex]);
        return draggableObject;
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
    public Action<int, int> DragInSlotCallback { get; set;}
    public Action<int> RemoveFromSlotCallback { get; set;}
}
