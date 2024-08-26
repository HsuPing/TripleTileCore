#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace TripleTileCoreEditor
{
    [CreateAssetMenu(fileName = "EditorToGameMediatorSO", menuName = "ScriptableObject/Create EditorToGameMediatorSO")]
    public class EditorToGameMediatorSO : ScriptableObject
    {
        public bool PlayDemo = false;
        public SceneAsset DemoScene;
        public TripleTileStageDataSO PlayDemoStageData;
        public string DataOutputPath =  "Assets/StageData/{0}.asset";
    }
}
#endif