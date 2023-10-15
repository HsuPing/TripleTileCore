using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor.SceneManagement;

namespace TripleTileGameDesginEditor
{
    public class TileLayerEditor
    {
        public List<Tile> Tiles = new List<Tile>();
        public ushort RowCountY;
        public ushort ColCountX;
    }

    public static class TripleTileGameDesginProcesser
    {
        public static int GetTileElementIndex(int _maxCol, int _row, int _col)  => (_row * _maxCol) + _col;
        public static int GetTileLayerIndex(int _layerCount, int _listViewIndex) => _layerCount - _listViewIndex;
    }

    public class TripleTileGameDesginEditorWindow : EditorWindow
    {
        private class TileLayers
        {
            public List<TileLayerEditor> Layers; 
            public TileLayers()
            {
                this.Layers = new List<TileLayerEditor>();
            }
        }

        private enum DataCheckResult { SUCCESS, COUNT_ERROR, DATA_NULL}

        static TripleTileGameDesginEditorWindow wnd;
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;
        private TileSpritesForEditorSO tileSpritesForEditorSO;
        private TripleTileGameDesginEditorMediatorSO tripleTileGameDesginEditorMediatorSO;
        private TileLayers tileLayers;
        private ObjectField tileLayersSOObjField;
        private LayerListViewController layerListViewController;
        private TileGroupController tileGroupController;
        private const string PLAYER_PREFS_KEY = "TripleTileGameDesginEditor";

        [MenuItem("三消遊戲編輯器/TripleTileGameDesginEditorWindow")]
        public static void ShowWindow()
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
            tileSpritesForEditorSO = AssetDatabase.LoadAssetAtPath<TileSpritesForEditorSO>("Assets/Editor/TileSpritesForEditorSO.asset");
            tripleTileGameDesginEditorMediatorSO = AssetDatabase.LoadAssetAtPath<TripleTileGameDesginEditorMediatorSO>("Assets/Editor/TripleTileGameDesginEditorMediator.asset");

            rootVisualElement.Q<Button>("ExportButton").RegisterCallback<ClickEvent>(onExportTemplateButton);
            rootVisualElement.Q<Button>("PlayDemoButton").RegisterCallback<ClickEvent>(onPlayDemoButton);

            layerListViewController = new LayerListViewController(new LayerListViewControllerModel()
            {
                RootElemnt = rootVisualElement.Q<VisualElement>("LayerListView"),
                ItemSource = tileLayers.Layers,
                GetLayerInfoCallback = getLayerInfo,
                ListItemClickCallback = setTilePanel,
                AddLayerCallback = addTileLayerData,
                RemoveLayerCallback = removeTileLayerDate
            });

            tileGroupController = new TileGroupController(new TileGroupControllerModel()
            {
                RootElement = rootVisualElement.Q<VisualElement>("TileGroup"),
                DragAndDropSprites = tileSpritesForEditorSO.TileSprites,
                OnAddCallback = onCreateTileButton,
                OnRemoveCallback = onDeleteTileButton,
                OnRowCountChange = (value) => onRowColValueChange(value, true),
                OnColCountChange = (value) => onRowColValueChange(value, false),
                SetTileId = setTileId
            });
        }

        private void OnEnable() 
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable() 
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode:
                    Debug.Log("Entered Edit Mode");
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    Debug.Log("Exiting Edit Mode");
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    Debug.Log("Entered Play Mode");
                    //TODO 儲存當前資料
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    Debug.Log("Exiting Play Mode");
                    tileLayersSOObjField.value = AssetDatabase.LoadAssetAtPath<TileLayersSO>(tripleTileGameDesginEditorMediatorSO.DemoDataPath);
                    onImportTempleteButton(null);
                    break;
            }
        }

        private void onImportTempleteButton(ClickEvent _clickEvent)
        {
            if(tileLayersSOObjField.value == null)
                return;

            TileLayersSO tileLayersSO = tileLayersSOObjField.value as TileLayersSO;
            tileLayers.Layers.Clear();
            var layerCount = tileLayersSO.TileLayers.Length;
            for(int index = layerCount - 1; index >= 0; index--)
            {
                var data = tileLayersSO.TileLayers[index];
                tileLayers.Layers.Add(new TileLayerEditor
                {
                    RowCountY = data.RowCountY,
                    ColCountX = data.ColCountX,
                    Tiles = data.Tiles.ToList(),
                });
            }

            layerListViewController.Rebuild();
            tileGroupController.RemoveTilePanel();
            refreshAllAmount();
        }

        private void onExportTemplateButton(ClickEvent _clickEvent)
        {
            //TODO 檢查資料格式
            //TODO 詢問視窗
            var dataCheckResult = dataCheck();

            if(dataCheckResult == DataCheckResult.SUCCESS)
            {
                string outputPath = String.Format(tripleTileGameDesginEditorMediatorSO.DataOutputPath, DateTime.Now.ToString("yyyymmddhhss"));
                if(tileLayersSOObjField.value != null)
                {
                    if(EditorUtility.DisplayDialog("是否覆蓋匯入檔案", "????????", "覆蓋", "新建"))
                    {
                        tileLayersSOObjField.value = exportSO(outputPath);
                    }
                    else
                    {
                        exportSO(outputPath);
                    }
                }

                exportSO(outputPath);
            }
        }

        private TileLayersSO exportSO(string _outputPath)
        {
            TileLayersSO tileLayersSO = CreateInstance<TileLayersSO>();
            tileLayersSO.TileLayers = new TileLayer[tileLayers.Layers.Count];
            int layerSOIndex = 0;
            int totalTilesCount = 0;

            for(int index = tileLayers.Layers.Count - 1; index >= 0; index--)
            {
                var layerData = tileLayers.Layers[index];
                tileLayersSO.TileLayers[layerSOIndex] = new TileLayer
                {
                    Tiles = layerData.Tiles.ToArray(),
                    RowCountY = layerData.RowCountY,
                    ColCountX = layerData.ColCountX
                };
                layerSOIndex += 1;
                totalTilesCount += layerData.Tiles.Count;
            }
            string path = _outputPath;
            tileLayersSO.DifferentIdCount = (ushort)(totalTilesCount / 6);

            AssetDatabase.CreateAsset(tileLayersSO, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = tileLayersSO;
            return tileLayersSO;
        }

        private void dataRefact() 
        {
            tileLayers.Layers.RemoveAll(layer => layer.Tiles == null || layer.Tiles.Count == 0);
            
            for(short index = 0; index < tileLayers.Layers.Count; index++)
            {
                var tiles = tileLayers.Layers[index];
                tiles.Tiles.RemoveAll((t) => t.RowY >= tiles.RowCountY || t.ColX >= tiles.ColCountX);
            }

            layerListViewController.Rebuild(-1);
            tileGroupController.RemoveTilePanel();
        }

        private DataCheckResult dataCheck()
        {
             if(tileLayers.Layers.Count <= 0)
                return DataCheckResult.DATA_NULL;

            dataRefact();

            if(tileLayers.Layers.Count <= 0)
                return DataCheckResult.DATA_NULL;

            int tilesCount = 0;
            foreach(var tiles in tileLayers.Layers)
            {
                tilesCount += tiles.Tiles.Count;
            }

            if(tilesCount % 3 != 0)
                return DataCheckResult.COUNT_ERROR;
            return DataCheckResult.SUCCESS;
        }

        private void onPlayDemoButton(ClickEvent _clickEvent)
        {
            var dataCheckResult = dataCheck();

            if(dataCheckResult != DataCheckResult.SUCCESS)
                return;
            
            exportSO(tripleTileGameDesginEditorMediatorSO.DemoDataPath);
            tripleTileGameDesginEditorMediatorSO.PlayDemo = true;

            if (EditorApplication.isPlaying)
            {
                EditorSceneManager.LoadScene(tripleTileGameDesginEditorMediatorSO.DemoScenePath);
            }
            else
            {
                EditorSceneManager.OpenScene(tripleTileGameDesginEditorMediatorSO.DemoScenePath);
                EditorApplication.isPlaying = true;
            }
        }

        //ListView Callback
        private string getLayerInfo(int _index)
        {
            var data = tileLayers.Layers[_index];
            return string.Format("第 {0} 層\n行: {1}, 列: {2}"
            , TripleTileGameDesginProcesser.GetTileLayerIndex(tileLayers.Layers.Count, _index)
            , data.RowCountY, data.ColCountX);
        }

        private void setTilePanel(int _index)
        {
            var data = tileLayers.Layers[_index];
            tileGroupController.SetGrid(data.RowCountY, data.ColCountX);
            tileGroupController.SetData(data.Tiles, data.RowCountY, data.ColCountX);
        }
        private void addTileLayerData()
        {
            tileLayers.Layers.Insert( 0 ,new TileLayerEditor() { RowCountY = 4, ColCountX = 5});
        }

        private void removeTileLayerDate(int _index)
        {
            tileGroupController.RemoveTilePanel();
            tileLayers.Layers.RemoveAt(_index);
            refreshAllAmount();
        }

        //TileController Callback
        private void onCreateTileButton(int _index)
        {
            var dataSet = getLayerRowCol(_index);
            Tile tile = new Tile
            {
                RowY = (ushort)dataSet.Item2,
                ColX = (ushort)dataSet.Item3
            };
            dataSet.Item1.Tiles.Add(tile);
            tileGroupController.SetTileCountAmount(0, getActiveTilesAmoutn());
        }

        private void onDeleteTileButton(int _index)
        {
            var dataSet = getLayerRowCol(_index);

            foreach(var tile in dataSet.Item1.Tiles)
            {
                if(tile.RowY == dataSet.Item2 && tile.ColX == dataSet.Item3)
                {
                    dataSet.Item1.Tiles.Remove(tile);
                    tileGroupController.SetTileCountAmount(tile.Id, getIdAmount(tile.Id));
                    break;
                }
            }

            tileGroupController.SetTileCountAmount(0, getActiveTilesAmoutn());
        }

        private void setTileId(int _index, int _id)
        {
            var dataSet = getLayerRowCol(_index);
            
            foreach(var tile in dataSet.Item1.Tiles)
            {
                if(tile.RowY == dataSet.Item2 && tile.ColX == dataSet.Item3)
                {
                    tile.Id = (ushort)_id;
                    break;
                }
            }

            tileGroupController.SetTileCountAmount(_id, getIdAmount(_id));
            tileGroupController.SetTileCountAmount(0, getActiveTilesAmoutn());
        }

        private void onRowColValueChange(int _value, bool _isRow)
        {
            var layer = tileLayers.Layers[layerListViewController.GetLayerIndex];
            bool refresh = false;
            if(_isRow)
            {
                if(layer.RowCountY != _value)
                {
                    layer.RowCountY = (ushort)_value;
                    refresh = true;
                }
            }
            else
            {
                if(layer.ColCountX != _value)
                {
                    layer.ColCountX = (ushort)_value;
                    refresh = true;
                }
            }

            if(refresh)
            {
                layerListViewController.Rebuild();
                setTilePanel(layerListViewController.GetLayerIndex);
                refreshAllAmount();
            }
        }

        private void refreshAllAmount()
        {
            Dictionary<int, int> amountDic = new Dictionary<int, int>();

            foreach(var layer in tileLayers.Layers)
            {
                foreach(var tile in layer.Tiles)
                {
                    if(tile.RowY >= layer.RowCountY || tile.ColX >= layer.ColCountX)
                        continue;
                    if(!amountDic.ContainsKey(tile.Id))
                        amountDic.Add(tile.Id, 0);
                    amountDic[tile.Id] += 1;
                }
            }

            foreach(var dic in amountDic)
            {
                tileGroupController.SetTileCountAmount(dic.Key, dic.Value);
            }
        }

        private int getIdAmount(int _id)
        {
            var idAmount = 0;

            foreach(var layer in tileLayers.Layers)
            {
                foreach(var tile in layer.Tiles)
                {
                    if(tile.Id == _id)
                        idAmount += 1;
                }
            }

            return idAmount;
        }

        private int getActiveTilesAmoutn()
        {
            var amount = 0;
            foreach(var layer in tileLayers.Layers)
            {
                foreach(var tile in layer.Tiles)
                {
                    if(tile.RowY >= layer.RowCountY || tile.ColX >= layer.ColCountX)
                        continue;
                    amount += 1;
                }
            }

            return amount;
        }

        private (TileLayerEditor ,int, int) getLayerRowCol(int _index)
        {
            var layer = tileLayers.Layers[layerListViewController.GetLayerIndex];
            int row = _index/layer.ColCountX;
            int col = _index%layer.ColCountX;
            return (layer, row, col);
        }
    }
}
