using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace TripleTileCore
{
    public class TripleTileController : MonoBehaviour
    {
        private TripleTileStageDataSO stageDataSO;
        [Header("Tile產出位置")]
        [SerializeField] private Transform tilesGroupTrans;
        [Header("Tile Prefab")]
        [SerializeField] private TileCell tileCellPrefab;
        [Header("Tile Pos Y 偏移")]
        [SerializeField] private float tileYMargin;
        [Header("Tile Cells Pool")]
        [SerializeField] private List<TileCell> tileCellPoolList = new List<TileCell>();
        [Header("TileCollectionController")]
        [SerializeField] private TileCollectionController tileCollectionController;

        [SerializeField] private TileSpritesSO tileSprites;  
        [SerializeField] private Physics2DRaycaster raycaster;
        public Action<bool> GameResultEvent;

        public void Init()
        {
            tileCollectionController.Init(passStage, failStage);
            raycaster = Camera.main.GetComponent<Physics2DRaycaster>();
        }
        
        public void SetGameData(TripleTileStageDataSO _stageDataSO)
        {
            stageDataSO = _stageDataSO;
            buildTileCells(stageDataSO);
        }

        private void buildTileCells(TripleTileStageDataSO _stageDataSO)  
        {
            Vector3 spriteBounds = tileCellPrefab.TileSpriteRenderer.sprite.bounds.extents;
            List<TileCell> unsetIDTileCellList = new List<TileCell>();
            List<TileCell> setUpTileCellList = new List<TileCell>();
            Dictionary<ushort, short> idCountPairs = new Dictionary<ushort, short>();
            int cellCount = 0;

            for(short layerIndex = 0; layerIndex < _stageDataSO.TileLayers.Length; layerIndex++)
            {
                var layer = _stageDataSO.TileLayers[layerIndex];
                var tiles = layer.Tiles;
                var centerX = (float)(layer.ColCountX - 1) / 2;
                var centerY = (float)(layer.RowCountY - 1) / 2;
                bool moveUpY = layerIndex > 0 && _stageDataSO.TileLayers[layerIndex - 1].RowCountY % 2 == layer.RowCountY % 2;

                for(short tileIndex = 0; tileIndex < tiles.Length; tileIndex++)
                {
                    var tile = tiles[tileIndex];
                    var tileCell = GetTileCell(cellCount);
                    var id = tile.ID;
                    tileCell.transform.localPosition = TripleTileProcessor.GetTilePosition(layerIndex ,tile.ColX, tile.RowY, spriteBounds.x, spriteBounds.y, centerX, centerY, tileYMargin, moveUpY); 
                    tileCell.SetTile(tile);
                    tileCell.SetOnClickCallback(onTileCellClick);

                    if(id == 0)
                        unsetIDTileCellList.Add(tileCell);
                    if(!idCountPairs.ContainsKey(id))
                        idCountPairs.Add(id, 1);
                    else
                        idCountPairs[id] += 1;

                    //tileCellList.Add(tileCell);
                    setUpTileCellList.Add(tileCell);
                    cellCount += 1;
                }
            }

            if(unsetIDTileCellList.Count > 0)
                TripleTileProcessor.SetTileID(unsetIDTileCellList, idCountPairs, _stageDataSO.TotalIDAmount, tileSprites.Sprites.Length);

            if(stageDataSO.TileLayers.Length > 1)
                TripleTileProcessor.SetTileCellsCover(setUpTileCellList, stageDataSO.TileLayers);

            var setupCellCount = setUpTileCellList.Count;
            for(short index = 0; index < tileCellPoolList.Count; index++)
            {
                tileCellPoolList[index].gameObject.SetActive(setupCellCount > index);
            }

            setTileCellSprite(setUpTileCellList, tileSprites.Sprites);


            #if UNITY_EDITOR
                foreach(var cell in setUpTileCellList)
                {
                    cell.gameObject.name = string.Format("layer:{0},ID:{1},ColX:{2},RowY:{3}", cell.Tile.Layer, cell.Tile.ID, cell.Tile.ColX, cell.Tile.RowY);
                }
            #endif
            tileCollectionController.SetData(setupCellCount);
            raycaster.enabled = true;
        }

        public void Restart()
        {
           buildTileCells(stageDataSO);
        }

        public bool ReturnLastStep()
        {
            return tileCollectionController.ReturnLastTile();
        }

        public bool RearrangeTiles()
        {
            return TripleTileProcessor.RearrangeTiles(tileCellPoolList, tileSprites.Sprites );
        }

        public bool AutoMatch()
        {
            return TripleTileProcessor.AutoMatch(tileCellPoolList);
        }

        private TileCell GetTileCell(int _index)
        {
            if(tileCellPoolList.Count > _index)
                return tileCellPoolList[_index];
            var tileCell = Instantiate(tileCellPrefab, tilesGroupTrans);
            tileCellPoolList.Add(tileCell);
            return tileCell;
        }

        private void setTileCellSprite(List<TileCell> _tileCellList, Sprite[] _iconSprites)
        {
            foreach(var cell in _tileCellList)
            {
                cell.SetSprite(_iconSprites[cell.Tile.ID - 1]);
            }
        }

        void onTileCellClick(TileCell _cell, bool _ignoreOutRange)
        {
            tileCollectionController.AddTileCell(_cell, _ignoreOutRange);
        }

        void passStage()
        {
            GameResultEvent?.Invoke(true);
            raycaster.enabled = false;
        }

        void failStage()
        {
            GameResultEvent?.Invoke(false);
            raycaster.enabled = false;
        }
    }

    public struct TileCoverCalculateData
    {
        public int DiffX;
        public int DiffY;
        public int AddBaseX;
        public int AddBaseY;
        public bool IsCoverCrossX;
        public bool IsCoverCrossY;
    }
}
