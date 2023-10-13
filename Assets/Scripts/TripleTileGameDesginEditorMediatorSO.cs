using UnityEngine;

public class TripleTileGameDesginEditorMediatorSO : ScriptableObject
{
    public bool PlayDemo = false;
    public string DemoScenePath = "Assets/Scenes/TripleTileDemo.unity";
    public string DemoDataPath = "Assets/Editor/PlayDemoStageData.asset";
    public string DataOutputPath =  "Assets/StageData/{0}.asset";
}
