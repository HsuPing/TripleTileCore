using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

public class UIController : MonoBehaviour
{
    private VisualElement _bottomContainer;
    private Button _openButton;
    private Button _closeButton;
    private VisualElement _bottomSheet;
    private VisualElement _scrim;
    private VisualElement _image_1;
    private VisualElement _image_fruit;
    private Label _message;
    const string messageContent = "\"這是一個火火火火火龍龍龍龍龍果果果果果！\"";

    
    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _bottomContainer = root.Q<VisualElement>("Container_Bottom");
        _openButton = root.Q<Button>("Btn_Open");
        _closeButton = root.Q<Button>("Btn_Close");
        _bottomSheet = root.Q<VisualElement>("BottomSheet");
        _scrim = root.Q<VisualElement>("Scrim");
        _image_1 = root.Q<VisualElement>("image_1");
        _image_fruit = root.Q<VisualElement>("Image_Fruit");
        _message = root.Q<Label>("message");

        _bottomContainer.style.display = DisplayStyle.None;

        _openButton.RegisterCallback<ClickEvent>((e) => showBottomPanel(e, true));
        _closeButton.RegisterCallback<ClickEvent>((e) => showBottomPanel(e, false));

        //StartCoroutine(animateImage1());

        //_bottomSheet.RegisterCallback<TransitionEndEvent>(onBottomSheetDown);
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(_bottomSheet.ClassListContains("bottom-sheet-up"));
        }
        else if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            _bottomContainer.style.display = DisplayStyle.Flex;
            // if(!_bottomSheet.ClassListContains("bottom-sheet-up"))
            //     _bottomSheet.AddToClassList("bottom-sheet-up");
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            if(!_bottomSheet.ClassListContains("bottom-sheet-up"))
                _bottomSheet.AddToClassList("bottom-sheet-up");
        }
    }

    private void showBottomPanel(ClickEvent _event, bool _active)
    {
        if(_active)
        {
            _bottomContainer.style.display = DisplayStyle.Flex;
            _bottomSheet.AddToClassList("bottom-sheet-up");
            _scrim.AddToClassList("scrim-show");
            //animateFruit();
        }
        else
        {
            _bottomContainer.style.display = DisplayStyle.None;
            _bottomSheet.RemoveFromClassList("bottom-sheet-up");
            _scrim.RemoveFromClassList("scrim-show");
        }
    }

    void onBottomSheetDown(TransitionEndEvent evt)
    {
        if(!_bottomSheet.ClassListContains("bottom-sheet-up"))
        {
            _bottomContainer.style.display = DisplayStyle.None;
        }
    }

    void animateFruit()
    {
        _image_fruit.ToggleInClassList("image-fruit-up");
        _image_fruit.RegisterCallback<TransitionEndEvent>((transEvent)=> _image_fruit.ToggleInClassList("image-fruit-up"));

        _message.text = string.Empty;
        DOTween.To(() => _message.text, x => _message.text = x, messageContent, 3f).SetEase(Ease.Linear);
    }

    IEnumerator animateImage1()
    {
        yield return new WaitForSeconds(0.05f);
        _image_1.RemoveFromClassList("image-1-out");
    }
}
