using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class DragAndDropWindow : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;
    TileSpritesForEditorSO tileSpritesForEditorSO;
    List<VisualElement> tilesObject = new List<VisualElement>();

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

        ScrollView scrollView = rootVisualElement.Q<ScrollView>("OjectsSlotScrollView");
       
        for(int index = 0; index < tileSpritesForEditorSO.TileSprites.Length; index++)
        {
            VisualElement draggableObjectSlot = new VisualElement();
            draggableObjectSlot.AddToClassList("draggable-object-default-slot");
            draggableObjectSlot.name = (index + 1).ToString();
            createNewDragObject(draggableObjectSlot, index);
            scrollView.Add(draggableObjectSlot);
        }
    }

    void createNewDragObject(VisualElement _parent, int _index)
    {
        VisualElement draggableObject = new VisualElement();
        draggableObject.AddToClassList("draggable-object");
        draggableObject.style.backgroundImage = new StyleBackground(tileSpritesForEditorSO.TileSprites[_index]);
        _parent.Add(draggableObject);
        //DragAndDropManipulator dragAndDropManipulator = new( draggableObject, rootVisualElement);
    }
}