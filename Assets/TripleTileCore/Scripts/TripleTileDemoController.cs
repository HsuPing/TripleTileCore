using UnityEngine;
using TripleTileCore;
using TMPro;
using UnityEngine.UI;

public class TripleTileDemoController : MonoBehaviour
{
    [SerializeField] private TripleTileController tripleTileController;
    [Header("關卡資料")]
    [SerializeField] private TripleTileStageDataSO stageDataSO;
#if UNITY_EDITOR
    [Header("編輯器與場景仲介資料")]
    public TripleTileCoreEditor.EditorToGameMediatorSO editorToGameMediatorSO;
#endif

    [SerializeField] private TripleTileStageDataSO[] stageDatas; 
    [SerializeField] private int level = 0;
    [SerializeField] private TextMeshProUGUI resultText;

    [SerializeField] private Button lastStepButton;
    [SerializeField] private Button autoMatchButton;
    [SerializeField] private Button rearrangeButton;

    [SerializeField] private Button restartButton;
    [SerializeField] private Button nextStageButton;

    bool isPlayDemo = false;

    private void Awake() 
    {
        restartButton.onClick.AddListener(restart);
        nextStageButton.onClick.AddListener(nextStage);

        lastStepButton.onClick.AddListener(LastStepItem);
        autoMatchButton.onClick.AddListener(AutoMatchItem);
        rearrangeButton.onClick.AddListener(RearrangeTilesItem);

        tripleTileController.Init();
    }

    void Start()
    {
        tripleTileController.GameResultEvent += showGameResult;

    #if UNITY_EDITOR
        if(editorToGameMediatorSO != null)
        {
            isPlayDemo = editorToGameMediatorSO.PlayDemo;
            if(isPlayDemo)
            {
                stageDataSO = editorToGameMediatorSO.PlayDemoStageData;
                tripleTileController.SetGameData(stageDataSO);
                return;
            }
        }
    #endif
  
        if(stageDataSO != null)
            tripleTileController.SetGameData(stageDataSO);
        else
        {
            if(stageDatas != null && stageDatas.Length > 0)
            {
                if(stageDatas[level] != null)
                    tripleTileController.SetGameData(stageDatas[level]);
            }
        }

        itemsEnable(true);
    }

    private void restart()
    {
        tripleTileController.Restart();
        resultText.text = "";
        itemsEnable(true);
    }

    private void nextStage()
    {
        if(isPlayDemo || stageDatas == null || stageDatas.Length == 0)
        {
            restart();
            return;
        }

        level += 1;
    
        if(stageDatas.Length <= level)
            level = 0;

        if(stageDatas[level] != null)
        {
            tripleTileController.SetGameData(stageDatas[level]);
            itemsEnable(true);
        }
        else
        {
            restart();
            return;
        }

        resultText.text = "";
    }

    private void showGameResult(bool _isWin)
    {
        resultText.text = _isWin ? "WIN": "LOSE";
        itemsEnable(false);
    }

    public void LastStepItem()
    {
        tripleTileController.ReturnLastStep();
    }

    public void AutoMatchItem()
    {
        tripleTileController.AutoMatch();
    }

    public void RearrangeTilesItem()
    {
        tripleTileController.RearrangeTiles();
    }

    private void itemsEnable(bool _enable)
    {
        lastStepButton.enabled = _enable;
        autoMatchButton.enabled = _enable;
        rearrangeButton.enabled = _enable;
    }

    private void OnDestroy()
    {
    #if UNITY_EDITOR
        editorToGameMediatorSO.PlayDemo = false;
    #endif
        tripleTileController.GameResultEvent -= showGameResult;
    }
}
