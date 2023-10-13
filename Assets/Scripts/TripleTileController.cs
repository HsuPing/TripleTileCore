using UnityEngine;
using System.Collections.Generic;

public class TripleTileController : MonoBehaviour
{
    [SerializeField] private TileLayersSO tileLayersSO;
    [SerializeField] private TileCell tileCellPrefab;
    [SerializeField] private Transform tileInstantiateTrans;
    [SerializeField] private float tileYMargin;

    [Header("Tile Collector")]
    [SerializeField] private SpriteRenderer tileCollectorSpriteRenderer;
    [SerializeField] private TileCell[] collectedTiles = new TileCell[7];
    [SerializeField] private Vector3[] collectVects = new Vector3[7];
    
    private void Awake()
    {
        setTileCollector();
    }

    private void Start() 
    {
        #if UNITY_EDITOR
        var mediator = UnityEditor.AssetDatabase.LoadAssetAtPath<TripleTileGameDesginEditorMediatorSO>("Assets/Editor/TripleTileGameDesginEditorMediator.asset");
        if(mediator.PlayDemo)
        {
            tileLayersSO = UnityEditor.AssetDatabase.LoadAssetAtPath<TileLayersSO>(mediator.DemoDataPath);
            mediator.PlayDemo = false;
        }
        #endif
        if(tileLayersSO != null && tileLayersSO.TileLayers != null)
        {
            instantiateTiles();
        }    
    }

    private void instantiateTiles()  //TODO: 物件池?
    {
        var tileLayers = tileLayersSO.TileLayers;
        var tileSpriteRenderer = tileCellPrefab.SpriteRenderer;
        var tileSpacingX = tileSpriteRenderer.sprite.bounds.extents.x;
        var tileSpacingY = tileSpriteRenderer.sprite.bounds.extents.y;
        float posX;
        float posY;
        Vector3 tilePos = Vector3.zero;
        var layerCount = tileLayers.Length;
        var tileCellPairs = new Dictionary<(int, int, int), TileCell>();
        
        for(short layerIndex = 0; layerIndex < layerCount; layerIndex++)  //layer process
        {
            var tileLayer = tileLayers[layerIndex];
            var tiles = tileLayer.Tiles;
            var cX = (float)(tileLayer.RowCountX - 1) / 2;
            var cY = (float)(tileLayer.ColCountY - 1) / 2;
            var baseOrder = (layerIndex + 1) * 10;

            for(ushort tileIndex = 0; tileIndex < tiles.Length; tileIndex++)  //tile process
            {
                var tile = tiles[tileIndex];
                posX = (tile.RowX - cX) * tileSpacingX * 2;
                posY = (tile.ColY - cY) * (tileSpacingY * -2 + tileYMargin); 
                tilePos.x = posX;
                tilePos.y = posY;
                var newTileCell = Instantiate(tileCellPrefab, tileInstantiateTrans);
                newTileCell.transform.localPosition = tilePos;
                newTileCell.SetData(tile, baseOrder + tile.ColY, onTileClick);
            #if UNITY_EDITOR
                var stringBuilder = new System.Text.StringBuilder();
                stringBuilder.Append(layerIndex);
                stringBuilder.Append(tile.RowX);
                stringBuilder.Append(tile.ColY);
                stringBuilder.Append(baseOrder + tile.ColY);
                newTileCell.gameObject.name = stringBuilder.ToString();
            #endif    

                tileCellPairs.Add((layerIndex, tile.RowX, tile.ColY), newTileCell);
            }
        }

        if(tileCellPairs.Count > 0)
        {
            setTilesCellData(tileCellPairs);
        }
    }

    private void setTilesCellData(Dictionary<(int, int, int), TileCell> _tileCellPairs)
    {
        var tileLayers = tileLayersSO.TileLayers;
        for(short layerIndex = 1; layerIndex < tileLayers.Length; layerIndex++)  //最底層不用判斷
        {
            var tilesLayerUp = tileLayers[layerIndex];

            var downLayerIndex = layerIndex - 1;
            var tilesLayerDown = tileLayers[downLayerIndex];

            int normalizeDiffX = tilesLayerDown.RowCountX - tilesLayerUp.RowCountX;
            bool coverTwoTileX = normalizeDiffX % 2 > 0;
            int normalizeDiffY = tilesLayerDown.ColCountY - tilesLayerUp.ColCountY;
            bool coverTwoTileY = normalizeDiffY % 2 > 0;

            normalizeDiffX = normalizeDiffX / 2;
            normalizeDiffY = normalizeDiffY / 2;

            var tiles = tilesLayerUp.Tiles;

            for(short tileIndex = 0; tileIndex < tiles.Length; tileIndex++)
            {
                var tile = tiles[tileIndex];
                int coverRowX = tile.RowX + normalizeDiffX;
                int coverColY = tile.ColY + normalizeDiffY;

                if(coverRowX > tilesLayerDown.RowCountX || coverColY > tilesLayerDown.ColCountY)
                    continue;

                TileCell upCell = _tileCellPairs[(layerIndex, tile.RowX, tile.ColY)];

                if(_tileCellPairs.ContainsKey((downLayerIndex, coverRowX, coverColY)))
                    registerEvent(upCell, _tileCellPairs[(downLayerIndex, coverRowX, coverColY)]);

                if(coverTwoTileX && coverTwoTileY)
                {
                    if(_tileCellPairs.ContainsKey((downLayerIndex, coverRowX + 1, coverColY)))
                        registerEvent(upCell, _tileCellPairs[(downLayerIndex, coverRowX + 1, coverColY)]);
                    if(_tileCellPairs.ContainsKey((downLayerIndex, coverRowX, coverColY + 1)))
                        registerEvent(upCell, _tileCellPairs[(downLayerIndex, coverRowX, coverColY + 1)]);
                    if(_tileCellPairs.ContainsKey((downLayerIndex, coverRowX + 1, coverColY + 1)))
                        registerEvent(upCell, _tileCellPairs[(downLayerIndex, coverRowX + 1, coverColY + 1)]);
                }
                else if(coverTwoTileX && _tileCellPairs.ContainsKey((downLayerIndex, coverRowX + 1, coverColY)))
                    registerEvent(upCell, _tileCellPairs[(downLayerIndex, coverRowX + 1, coverColY)]);
                else if(coverTwoTileY && _tileCellPairs.ContainsKey((downLayerIndex, coverRowX, coverColY + 1)))
                    registerEvent(upCell, _tileCellPairs[(downLayerIndex, coverRowX, coverColY + 1)]);
            }
        }

        foreach(var dic in _tileCellPairs)
        {
            dic.Value.SetBlockState();
        }
    }

    private void registerEvent(TileCell _upCell, TileCell _downCell)
    {
        _downCell.AddBlockCount();
        _upCell.RemoveEvent.AddListener(_downCell.BlockCellRemove);
    }

    void setTileCollector()
    {
        var tileSpacingX = tileCollectorSpriteRenderer.sprite.bounds.extents.x * 2;
        var collectorVect = tileCollectorSpriteRenderer.transform.localPosition;
        int middleNum = 3;

        for(ushort index = 0; index < collectVects.Length; index++)
        {
            float x = collectorVect.x + (index - middleNum) * tileSpacingX;
            collectVects[index] = new Vector3(x ,collectorVect.y, collectorVect.z);
        }
    }

    void onTileClick(TileCell _cell)
    {
        _cell.TriggerEnable(false);
        putIntoCollect(_cell);
    }

    void putIntoCollect(TileCell _putInCell)
    {
        var putInCellId = _putInCell.Id;
        short nullIndex = -1;
        short matchIndex = -1;
        bool lose = true;

        for(short index = 0; index < collectedTiles.Length; index++)
        {
            var cell = collectedTiles[index];
            if(cell != null)
            {
                if(cell.Id == putInCellId)
                {
                    if(matchIndex < 0)
                        matchIndex = index;
                    else
                    {
                        //Match
                        _putInCell.RemoveEvent?.Invoke();
                        _putInCell.OnMatch();
                        collectedTiles[matchIndex].OnMatch();
                        collectedTiles[matchIndex] = null;
                        cell.OnMatch();
                        collectedTiles[index] = null;
                        return;
                    }
                }
            }
            else
            {
                if(nullIndex < 0)
                    nullIndex = index;
                else
                    lose = false;
            }
        }

        if(nullIndex == -1)
            return;

        _putInCell.RemoveEvent?.Invoke();
        _putInCell.transform.localPosition = collectVects[nullIndex];
        collectedTiles[nullIndex] = _putInCell;
    }

    private List<(int, int)> getTileBlockIndex((ushort, ushort) buildTileRowCol, (ushort, ushort) upLayerRowCol, (ushort, ushort) downLayerRowCol)
    {
        //TODO 這部分運算可以抽出來
        int normalizeDiffX = downLayerRowCol.Item1 - upLayerRowCol.Item1;
        bool coverTwoTileX = normalizeDiffX % 2 > 0;
        int normalizeDiffY = downLayerRowCol.Item2 - upLayerRowCol.Item2;
        bool coverTwoTileY = normalizeDiffY % 2 > 0;

        normalizeDiffX = normalizeDiffX / 2;
        normalizeDiffY = normalizeDiffY / 2;
        //

        int coverRowX = buildTileRowCol.Item1 + normalizeDiffX;
        int coverColY = buildTileRowCol.Item2 + normalizeDiffY;
    #if UNITY_EDITOR
        Debug.LogFormat("checkObjectBlock target({0}, {1}), downLayer({2}, {3}), upLayer({4}, {5}), cover({6}, {7}), coverTwoTileX: {8}, coverTwoTileY: {9}."
            , buildTileRowCol.Item1, buildTileRowCol.Item2, downLayerRowCol.Item1, downLayerRowCol.Item2
            , upLayerRowCol.Item1, upLayerRowCol.Item2, coverRowX, coverColY, coverTwoTileX, coverTwoTileY);
    #endif

        List<(int, int)> blockIndexList = null;
        if(coverRowX > downLayerRowCol.Item1 || coverColY > downLayerRowCol.Item2)
            return blockIndexList;

        blockIndexList = new List<(int, int)>() {(coverRowX, coverColY)};

        if(coverTwoTileX && coverTwoTileY)
        {
            blockIndexList.Add((coverRowX + 1, coverColY));
            blockIndexList.Add((coverRowX, coverColY + 1));
            blockIndexList.Add((coverRowX + 1, coverColY + 1));
        }
        else if(coverTwoTileX)
            blockIndexList.Add((coverRowX + 1, coverColY));
        else if(coverTwoTileY)
            blockIndexList.Add((coverRowX, coverColY + 1));

        return blockIndexList;
    }
}
