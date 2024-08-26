using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TripleTileCore;

namespace TripleTileCoreEditor
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
            public int IDAmount;
            public TileLayers()
            {
                Layers = new List<TileLayerEditor>();
            }
        }

        static TripleTileGameDesginEditorWindow wnd;
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;
        private TileSpritesSO tileSpritesSO;
        private EditorToGameMediatorSO tripleTileGameDesginEditorMediatorSO;
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
            tileLayersSOObjField.objectType = typeof(TripleTileStageDataSO); 
            rootVisualElement.Q<Button>("ImportButton").RegisterCallback<ClickEvent>(onImportTempleteButton);
            tileSpritesSO = AssetDatabase.LoadAssetAtPath<TileSpritesSO>("Assets/TripleTileCore/ScriptableObjects/TileSpritesSO.asset");
            tripleTileGameDesginEditorMediatorSO = AssetDatabase.LoadAssetAtPath<EditorToGameMediatorSO>("Assets/TripleTileCore/Editor/ScriptableObjects/EditorToGameMediatorSO.asset");

            rootVisualElement.Q<Button>("ExportButton").RegisterCallback<ClickEvent>(onExportTemplateButton);
            rootVisualElement.Q<Button>("PlayDemoButton").RegisterCallback<ClickEvent>(onPlayDemoButton);
            rootVisualElement.Q<Button>("RemoveAllIdButton").RegisterCallback<ClickEvent>(onRemoveAllIdPreWarm);
            rootVisualElement.Q<Button>("OpenAllTilesButton").RegisterCallback<ClickEvent>(onOpenAllTilesButton);

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
                DragAndDropSprites = tileSpritesSO.Sprites,
                OnAddCallback = onCreateTileButton,
                OnRemoveCallback = onDeleteTileButton,
                OnRowCountChange = (value) => onRowColValueChange(value, true),
                OnColCountChange = (value) => onRowColValueChange(value, false),
                SetTileId = setTileId,
                InputIDMaxAmount = tileSpritesSO.Sprites.Length
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
                case PlayModeStateChange.ExitingEditMode:
                    if(tileLayersSOObjField.value != null)
                        PlayerPrefs.SetString(PLAYER_PREFS_KEY, AssetDatabase.GetAssetPath(tileLayersSOObjField.value));
                break;
                   
                case PlayModeStateChange.ExitingPlayMode:
                    string path = PlayerPrefs.GetString(PLAYER_PREFS_KEY);
                    if(!string.IsNullOrEmpty(path))
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<TripleTileStageDataSO>(path);
                        if(asset != null)
                            tileLayersSOObjField.value = asset;
                        else
                            PlayerPrefs.DeleteKey(PLAYER_PREFS_KEY);
                    }

                    onImportTempleteButton(null);
                    break;
            }
        }

        private void onImportTempleteButton(ClickEvent _clickEvent)
        {
            if(tileLayersSOObjField.value == null)
                return;

            TripleTileStageDataSO tileLayersSO = tileLayersSOObjField.value as TripleTileStageDataSO;
            tileLayers.IDAmount = tileLayersSO.TotalIDAmount;
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
            
            tileGroupController.RemoveTilePanel();
            layerListViewController.Rebuild(0);
            refreshAllAmount();
        }

        private void onExportTemplateButton(ClickEvent _clickEvent)
        {
            string outputPath = string.Format(tripleTileGameDesginEditorMediatorSO.DataOutputPath, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
            if(tileLayersSOObjField.value == null || tileLayersSOObjField.value == tripleTileGameDesginEditorMediatorSO.PlayDemoStageData)
                exportStageData(tileLayers, outputPath);
            else
            {
                if(EditorUtility.DisplayDialog("是否覆蓋匯入檔案?", "確定要覆蓋檔案嗎？", "覆蓋", "新建"))
                    exportStageData(tileLayers);
                else
                    exportStageData(tileLayers, outputPath);
            }
        }

        private void onPlayDemoButton(ClickEvent _clickEvent)
        {
            if(tileLayers.Layers.Count == 0)
                return;

            tileLayersSOObjField.value = tripleTileGameDesginEditorMediatorSO.PlayDemoStageData;
            exportStageData(tileLayers);
            tripleTileGameDesginEditorMediatorSO.PlayDemo = true;

            if (EditorApplication.isPlaying)
            {
                EditorSceneManager.LoadScene(tripleTileGameDesginEditorMediatorSO.DemoScene.name);
            }
            else
            {
                if(SceneManager.GetActiveScene().name != tripleTileGameDesginEditorMediatorSO.DemoScene.name)
                {
                    var path = AssetDatabase.GetAssetPath(tripleTileGameDesginEditorMediatorSO.DemoScene);
                    EditorSceneManager.OpenScene(path);
                }
                EditorApplication.isPlaying = true;
            }
        }

        private void exportStageData(TileLayers _stageData, string _outputPath = null)
        {
            //資料檢查
            var stageData = dataRefact(_stageData);
            if(!dataCheck(stageData))
                return;

            bool coverData = string.IsNullOrEmpty(_outputPath);
            TripleTileStageDataSO stageDataSO = transToTripleTileStageDataSO(stageData);

            if(!coverData)
            {
                AssetDatabase.CreateAsset(stageDataSO, _outputPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
                tileLayersSOObjField.value = stageDataSO;
            }
            else
            {
                TripleTileStageDataSO tileLayersSO = tileLayersSOObjField.value as TripleTileStageDataSO;
                tileLayersSO.TileLayers = stageDataSO.TileLayers;
                tileLayersSO.TotalIDAmount = stageDataSO.TotalIDAmount;
            }

            Selection.activeObject = tileLayersSOObjField.value;

            tileGroupController.RemoveTilePanel();
            layerListViewController.Rebuild(0);
        }

        private bool dataCheck(TileLayers _stageData)
        {
            if(tileLayers.Layers.Count <= 0)
                return false;

            int tilesCount = 0;
            var tileIDPairs = new Dictionary<ushort, int>();
            foreach(var layer in tileLayers.Layers)
            {
                tilesCount += layer.Tiles.Count;
                foreach(var tile in layer.Tiles)
                {
                    if(tileIDPairs.ContainsKey(tile.ID))
                        tileIDPairs[tile.ID] += 1;
                    else
                        tileIDPairs.Add(tile.ID, 1);
                }
            }

            if(tilesCount % TripleTileProcessor.MatchValue != 0)
            {
                ShowNotification(new GUIContent("總數不為3的倍數"));
                return false;
            }

            //未設定ID tile數量
            int unsetIDCount = 0;

            if(tileIDPairs.ContainsKey(0))
                unsetIDCount = tileIDPairs[0];

            foreach(var pair in tileIDPairs)
            {
                if(pair.Key != 0)
                {
                    int leftCount = pair.Value % TripleTileProcessor.MatchValue;
                    if(leftCount > 0)
                        leftCount = TripleTileProcessor.MatchValue - leftCount;
                  
                    unsetIDCount = unsetIDCount - leftCount;
                    if(unsetIDCount < 0)
                    {
                        ShowNotification(new GUIContent("手動設置ID數量錯誤"));
                        return false;
                    }
                }
            }

            //檢查ID amount
            var idAmount = tileGroupController.GetIDAmount;  
            var maxIdCount = unsetIDCount / TripleTileProcessor.MatchValue;
            if(idAmount > maxIdCount || idAmount == 0)
            {
                idAmount = (ushort)Mathf.Max(unsetIDCount / 6, 0);

                if(idAmount > maxIdCount)
                    ShowNotification(new GUIContent("不同卡牌ID數量太多\n程式自動調整數量為" + idAmount));
            }

            _stageData.IDAmount = idAmount;

            return true;
        }

        private TileLayers dataRefact(TileLayers _stageData)
        {
            //移除沒有tile資料的layer
            _stageData.Layers.RemoveAll(layer => layer.Tiles == null || layer.Tiles.Count == 0);
            //移除不在範圍內的tile
            for(short index = 0; index < tileLayers.Layers.Count; index++)
            {
                var tiles = tileLayers.Layers[index];
                tiles.Tiles.RemoveAll((t) => t.RowY >= tiles.RowCountY || t.ColX >= tiles.ColCountX);
            }

            return _stageData;
        }

        private TripleTileStageDataSO transToTripleTileStageDataSO(TileLayers _stageData)
        {
            TripleTileStageDataSO stageDataSO = CreateInstance<TripleTileStageDataSO>();
            stageDataSO.TileLayers = new TileLayer[_stageData.Layers.Count];
            int layerSOIndex = 0;
            int totalTilesCount = 0;

            for(int index = _stageData.Layers.Count - 1; index >= 0; index--)
            {
                var layerData = _stageData.Layers[index];

                stageDataSO.TileLayers[layerSOIndex] = new TileLayer
                {
                    RowCountY = layerData.RowCountY,
                    ColCountX = layerData.ColCountX
                };

                Tile[] tiles = new Tile[layerData.Tiles.Count];

                for(int index2 = 0; index2 < tiles.Length; index2++)
                {
                    tiles[index2].ID = layerData.Tiles[index2].ID;
                    tiles[index2].ColX = layerData.Tiles[index2].ColX;
                    tiles[index2].RowY = layerData.Tiles[index2].RowY;
                    tiles[index2].Layer = (ushort)(layerSOIndex + 1);
                }

                stageDataSO.TileLayers[layerSOIndex].Tiles = tiles;
                layerSOIndex += 1;
                totalTilesCount += layerData.Tiles.Count;
            }
            
            stageDataSO.TotalIDAmount = (ushort)_stageData.IDAmount;

            return stageDataSO;
        }

        private void onRemoveAllIdPreWarm(ClickEvent _clickEvent)
        {
            if (tileLayers.Layers.Count == 0)
                return;

            if (EditorUtility.DisplayDialog("移除警告", "是否移除所有ID", "移除", "取消"))
            {
                removeAllId();
            }
        }

        private void onOpenAllTilesButton(ClickEvent _clickEvent)
        {
            var layerIndex = layerListViewController.GetLayerIndex;
            if(tileLayers != null && tileLayers.Layers != null && tileLayers.Layers.Any() && layerIndex >= 0)
            {
                var layer = tileLayers.Layers[layerListViewController.GetLayerIndex];
                var amout = layer.RowCountY * layer.ColCountX;
                var tiles = layer.Tiles;
                
                for(short tileIndex = 0; tileIndex < amout; tileIndex++)
                {
                    var dataSet = getLayerRowCol(layerIndex, tileIndex);
                    if(tiles.Any(tile => tile.ColX == dataSet.Item3 && tile.RowY == dataSet.Item2))
                        continue;
                    Tile tile = new Tile
                    {
                        RowY = (ushort)dataSet.Item2,
                        ColX = (ushort)dataSet.Item3
                    };
                    tiles.Add(tile);
                }

                tileGroupController.RemoveTilePanel();
                layerListViewController.Rebuild(layerListViewController.GetLayerIndex);
                tileGroupController.SetTileCountAmount(0, getActiveTilesAmoutn());
            }
        }

        private void removeAllId()
        {
            for(int index = 0; index < tileLayers.Layers.Count; index++)
            {
                for(short tileIndex = 0; tileIndex < tileLayers.Layers[index].Tiles.Count; tileIndex++)
                {
                    var tile = tileLayers.Layers[index].Tiles[tileIndex];
                    tileLayers.Layers[index].Tiles[tileIndex] = new Tile() { ID = 0, ColX = tile.ColX, RowY = tile.RowY};
                }
            }

            var onViewLayerIndex = layerListViewController.GetLayerIndex;
            if (onViewLayerIndex >= 0)
            {
                setTilePanel(onViewLayerIndex);

                for(int index = 1; index <= tileSpritesSO.Sprites.Length; index++)
                    tileGroupController.SetTileCountAmount(index, 0);
            }
        }

    #region LayerListViewController Callback
        private string getLayerInfo(int _index)
        {
            var data = tileLayers.Layers[_index];
            return string.Format("第 {0} 層\n列: {2}, 行: {1}"
            , TripleTileGameDesginProcesser.GetTileLayerIndex(tileLayers.Layers.Count, _index)
            , data.RowCountY, data.ColCountX);
        }

        private void setTilePanel(int _index)
        {
            var data = tileLayers.Layers[_index];
            tileGroupController.SetGrid(data.RowCountY, data.ColCountX, tileLayers.IDAmount);
            tileGroupController.SetData(data.Tiles, data.RowCountY, data.ColCountX);
        }
        private void addTileLayerData()
        {
            tileLayers.Layers.Insert( 0 ,new TileLayerEditor() { RowCountY = 3, ColCountX = 4});
        }

        private void removeTileLayerDate(int _index)
        {
            tileGroupController.RemoveTilePanel();
            tileLayers.Layers.RemoveAt(_index);
            refreshAllAmount();
        }
    #endregion
    #region TileGroupController Callback
        private void onCreateTileButton(int _index)
        {
            var dataSet = getLayerRowCol(layerListViewController.GetLayerIndex, _index);
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
            var dataSet = getLayerRowCol(layerListViewController.GetLayerIndex , _index);

            foreach(var tile in dataSet.Item1.Tiles)
            {
                if(tile.RowY == dataSet.Item2 && tile.ColX == dataSet.Item3)
                {
                    dataSet.Item1.Tiles.Remove(tile);
                    tileGroupController.SetTileCountAmount(tile.ID, getIdAmount(tile.ID));
                    break;
                }
            }

            tileGroupController.SetTileCountAmount(0, getActiveTilesAmoutn());
        }

        private void setTileId(int _index, int _id, bool _isRemove)
        {
            var dataSet = getLayerRowCol(layerListViewController.GetLayerIndex ,_index);

            for(int index = 0; index < dataSet.Item1.Tiles.Count; index++)
            {
                var tile = dataSet.Item1.Tiles[index];
                if(tile.RowY == dataSet.Item2 && tile.ColX == dataSet.Item3)
                {
                    dataSet.Item1.Tiles[index] = new Tile() {ID = _isRemove ? (ushort)0 :(ushort)_id, ColX = tile.ColX, RowY = tile.RowY};
                }
            }
            
            refreshAllAmount();
            // tileGroupController.SetTileCountAmount(_id, getIdAmount(_id));
            // tileGroupController.SetTileCountAmount(0, getActiveTilesAmoutn());
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
            int[] tilesNums = new int[tileSpritesSO.Sprites.Length + 1];

            foreach(var layer in tileLayers.Layers)
            {
                foreach(var tile in layer.Tiles)
                {
                    if(tile.RowY >= layer.RowCountY || tile.ColX >= layer.ColCountX)
                        continue;
                    tilesNums[tile.ID] += 1;
                }
            }

            for(short index = 0; index <= tileSpritesSO.Sprites.Length; index++)
            {
                tileGroupController.SetTileCountAmount(index, tilesNums[index]);
            }
        }

        private int getIdAmount(int _id)
        {
            var idAmount = 0;

            foreach(var layer in tileLayers.Layers)
            {
                foreach(var tile in layer.Tiles)
                {
                    if(tile.ID == _id)
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
    #endregion

        private (TileLayerEditor ,int, int) getLayerRowCol(int _layer,int _index)
        {
            var layer = tileLayers.Layers[_layer];
            int row = _index/layer.ColCountX;
            int col = _index%layer.ColCountX;
            return (layer, row, col);
        }
    }
}
