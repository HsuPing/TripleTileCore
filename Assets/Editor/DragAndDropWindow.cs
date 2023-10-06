using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class DragAndDropWindow : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;
    TileSpritesForEditorSO tileSpritesForEditorSO;
    List<VisualElement> tileSlotsList = new List<VisualElement>();
    DragAndDropController dragAndDropController;

    [MenuItem("三消遊戲編輯器/Drag And Drop")]
    public static void ShowExample()
    {
        DragAndDropWindow wnd = GetWindow<DragAndDropWindow>();
        wnd.titleContent = new GUIContent("Drag And Drop");
    }

    public void CreateGUI()
    {
        m_VisualTreeAsset.CloneTree(rootVisualElement);
        tileSpritesForEditorSO = AssetDatabase.LoadAssetAtPath<TileSpritesForEditorSO>("Assets/Editor/TileSpritesForEditorSO.asset");

        if(tileSpritesForEditorSO == null)
            return;

        // for(int index = 0; index < tileSpritesForEditorSO.TileSprites.Length; index++)
        // {
        //     VisualElement draggableObjectSlot = new VisualElement();
        //     draggableObjectSlot.AddToClassList("draggable-object-default-slot");
        //     ushort id = (ushort)(index + 1);
        //     draggableObjectSlot.name = id.ToString();
        //     createNewDragObject(draggableObjectSlot, id);
        //     scrollView.Add(draggableObjectSlot);
        //     tileSlotsList.Add(draggableObjectSlot);
        // }
        // DragAndDropControllerModel dragAndDropControllerModel = new DragAndDropControllerModel()
        // {
        //     ScrollView = rootVisualElement.Q<ScrollView>("OjectsSlotScrollView"),
        //     ItemSprites = tileSpritesForEditorSO.TileSprites,
        //     SearchSlotRoot = rootVisualElement.Q<VisualElement>("DragAndDropRoot"),
        //     DefaultSlotClassName = "draggable-object-default-slot",
        //     DraggableObjectClassName = "draggable-object",
        //     SlotClassName = "slot"
        // };

        //dragAndDropController = new DragAndDropController(dragAndDropControllerModel);
    }

    // void createNewDragObject(VisualElement _parent, ushort _id)
    // {
    //     VisualElement draggableObject = new VisualElement();
    //     draggableObject.AddToClassList("draggable-object");
    //     draggableObject.style.backgroundImage = new StyleBackground(tileSpritesForEditorSO.TileSprites[_id - 1]);
    //     _parent.Add(draggableObject);
    //     DragAndDropManipulator dragAndDropManipulator = new( draggableObject, rootVisualElement, _id, tileMoveToSlot);
    // }

    // void tileMoveToSlot(ushort _id)
    // {
    //     createNewDragObject(tileSlotsList[_id - 1], _id);
    // }
}