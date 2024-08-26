using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TripleTileCore
{
    public class TripleTileProcessor
    {
        public const ushort MatchValue = 3;

        public static void SetTileID(List<TileCell> _tileCellList, Dictionary<ushort, short> _idCountPairs, ushort _totalIDAmount, int _maxIDLimit)
        {
             var unsetIDList = new List<ushort>();
      
            foreach(var pair in _idCountPairs)
            {
                if(pair.Key != 0)
                {
                    var remaindCount = MatchValue - (pair.Value % MatchValue);
                    if(remaindCount > 0)
                    {
                        for(short index = 0; index < remaindCount; index++)
                        {
                            unsetIDList.Add(pair.Key);
                        }
                    }
                }
            }

            var unsetCellCount = _tileCellList.Count - unsetIDList.Count;
            var neededPairIDCountArray = getNeededPairIDCountArray(unsetCellCount, _totalIDAmount);
            var allIDList = getAllIDList(_maxIDLimit);
            var slicesIndex = 0;   
            int addIdCount = 0;
            int setId = allIDList[Random.Range(0, allIDList.Count - 1)];

            if(unsetIDList.Count < unsetCellCount)
            {
                for(short index = 0; index < unsetCellCount; index++)
                {
                    unsetIDList.Add((ushort)setId);
                    addIdCount += 1;
                    
                    if(addIdCount == neededPairIDCountArray[slicesIndex])
                    {
                        slicesIndex += 1;
                        
                        if(allIDList.Any())
                        {
                            allIDList.Remove(setId);
                            setId = allIDList[Random.Range(0, allIDList.Count - 1)];
                            addIdCount = 0;
                        }
                    }
                }
            }

            shuffleIntList(unsetIDList);

            for(short index = 0; index < _tileCellList.Count; index++)
            {
                _tileCellList[index].SetID(unsetIDList[index]);
            }
        }

        public static void SetTileCellsCover(List<TileCell> _tileCellList, TileLayer[] _layers)
        {
            var tileCellPairs = getAllTileCellPairs(_tileCellList);
            var calculateDataPairs = getCoverCalculateDataPairs(_layers);

            for(ushort index = 0; index < _tileCellList.Count; index++)
            {
                var cell = _tileCellList[index];
                var tile = cell.Tile;

                if(tile.Layer == 1)
                    continue;
                
                ushort downLayerIndex = tile.Layer -= 1;
                var calculateData = calculateDataPairs[tile.Layer];
                ushort baseCoverColX = (ushort)(tile.ColX + calculateData.DiffX);
                ushort baseCoverRowY = (ushort)(tile.RowY + calculateData.DiffY);
                ushort addX = (ushort)(baseCoverColX + calculateData.AddBaseX);
                ushort addY = (ushort)(baseCoverRowY + calculateData.AddBaseY);

                if(tileCellPairs.ContainsKey((downLayerIndex, baseCoverColX, baseCoverRowY)))
                {
                    cell.AddCoverTileCell(tileCellPairs[(downLayerIndex, baseCoverColX, baseCoverRowY)]);
                }

                if(calculateData.IsCoverCrossX)
                {
                    if(tileCellPairs.ContainsKey((downLayerIndex, addX, baseCoverRowY)))
                    {
                        cell.AddCoverTileCell(tileCellPairs[(downLayerIndex, addX, baseCoverRowY)]);
                    }
                }

                if(calculateData.IsCoverCrossY)
                {
                    if(tileCellPairs.ContainsKey((downLayerIndex, baseCoverColX , addY)))
                    {
                        cell.AddCoverTileCell(tileCellPairs[(downLayerIndex, baseCoverColX , addY)]);
                    }
                }

                if(calculateData.IsCoverCrossX && calculateData.IsCoverCrossY)
                {
                    if(tileCellPairs.ContainsKey((downLayerIndex, addX, addY)))
                    {
                        cell.AddCoverTileCell(tileCellPairs[(downLayerIndex, addX, addY)]);
                    }
                }
            }
        }

        public static Vector3 GetTilePosition(float _layer ,float _colX, float _rowY, float _spriteWidth, float _spriteHeight, float _centerX, float _centerY ,float _tileYMargin, bool _moveUpY)
        {
            float x = (_colX - _centerX) * _spriteWidth * 2;
            float y = (_rowY - _centerY) * (_spriteHeight * -2 + _tileYMargin) + (_layer * _tileYMargin);
            //if(_moveUpY)
                //y = y + _tileYMargin;

            return new Vector3(x, y, -_layer);
        }

        public static bool RearrangeTiles(List<TileCell> _tileCellPoolList, Sprite[] _iconSprites)
        {
            List<int> activeIndexs = new List<int>();
            List<ushort> ids = new List<ushort>();

            for(short index = 0; index < _tileCellPoolList.Count; index++)
            {
                var tileCell = _tileCellPoolList[index];
                if(!tileCell.InCollection && tileCell.gameObject.activeInHierarchy)
                {
                    activeIndexs.Add(index);
                    ids.Add(tileCell.ID);
                }
            }

            if(activeIndexs.Count < MatchValue)
                return false;

            shuffleIntList(ids);

            for(short index = 0; index < activeIndexs.Count; index++)
            {
                var tileCell = _tileCellPoolList[activeIndexs[index]];
                var newId = ids[index];
                tileCell.SetNewData(newId, _iconSprites[newId - 1]);
            }

            return true;
        }

        public static bool AutoMatch(List<TileCell> _tileCellPoolList)
        {
            var tileCellIdPairs = new Dictionary<ushort, List<TileCell>>();
            var onCollectionTileCellIdPairs = new Dictionary<ushort, List<TileCell>>();
        
            for(short index = 0; index < _tileCellPoolList.Count; index++)
            {
                var tileCell = _tileCellPoolList[index];
                if(tileCell.gameObject.activeInHierarchy)
                {
                    if(!tileCell.InCollection)
                    {
                        if(!tileCellIdPairs.ContainsKey(tileCell.ID))
                            tileCellIdPairs.Add(tileCell.ID, new List<TileCell>(){ tileCell});
                        else
                            tileCellIdPairs[tileCell.ID].Add( tileCell);
                    }
                    else
                    {
                        if(!onCollectionTileCellIdPairs.ContainsKey(tileCell.ID))
                            onCollectionTileCellIdPairs.Add(tileCell.ID, new List<TileCell>(){ tileCell});
                        else
                            onCollectionTileCellIdPairs[tileCell.ID].Add(tileCell);
                    }
                }
            }

            if(tileCellIdPairs.Count <= 0)
                return false;

            List<ushort> hasThreeTilesIdList = new List<ushort>();
            List<ushort> hasTwoTilesIdList = new List<ushort>();
            List<ushort> hasOneTilesIdList = new List<ushort>();

            foreach(var pair in tileCellIdPairs)
            {
                var left = pair.Value.Count % MatchValue;
                switch(left)
                {
                    case 0:
                        hasThreeTilesIdList.Add(pair.Key);
                    break;
                    case 1:
                        hasTwoTilesIdList.Add(pair.Key);
                    break;
                    case 2:
                        hasOneTilesIdList.Add(pair.Key);
                    break;
                }
            }

            if(hasThreeTilesIdList.Count > 0)
            {
                var rIndex = Random.Range(0, hasThreeTilesIdList.Count);
                var id = hasThreeTilesIdList[rIndex];
                var tileCellList = tileCellIdPairs[id];

                for(short index = 0; index < 3; index++)
                {
                    rIndex = Random.Range(0, tileCellList.Count);
                    var tileCell = tileCellList[rIndex];
                    tileCell.AutoCollect();
                    tileCellList.Remove(tileCell);
                }
            }
            else if(hasOneTilesIdList.Count > 0)
            {
                var rIndex = Random.Range(0, hasOneTilesIdList.Count);
                var id = hasOneTilesIdList[rIndex];
                var tileCellList = tileCellIdPairs[id];

                for(short index = 0; index < 2; index++)
                {
                    rIndex = Random.Range(0, tileCellList.Count);
                    var tileCell = tileCellList[rIndex];
                    tileCell.AutoCollect();
                    tileCellList.Remove(tileCell);
                }
            }
            else if(hasTwoTilesIdList.Count > 0)
            {
                var rIndex = Random.Range(0, hasTwoTilesIdList.Count);
                var id = hasTwoTilesIdList[rIndex];
                var tileCellList = tileCellIdPairs[id];

                rIndex = Random.Range(0, tileCellList.Count);
                var tileCell = tileCellList[rIndex];
                tileCell.AutoCollect();
                tileCellList.Remove(tileCell);                
            }

            return true;
        }

        private static List<int> getAllIDList(int _count)
        {
            var allIDList = new List<int>();
            for(short index = 1; index <= _count; index++)
            {
                allIDList.Add(index);
            }
            return allIDList;
        }

        private static int[] getNeededPairIDCountArray(int _neededCount, int _totalIDAmount)
        {
            _totalIDAmount = Mathf.Max(_totalIDAmount, 1);
            int[] neededPairIDCountArray = new int[_totalIDAmount];

            int baseValue = _neededCount / _totalIDAmount / MatchValue;
            baseValue = baseValue * MatchValue;

            for(short index = 0; index < neededPairIDCountArray.Length; index++)
            {
                neededPairIDCountArray[index] = baseValue;
            }

            _neededCount -= baseValue * neededPairIDCountArray.Length;

            if(_neededCount > 0)
            {
                ushort sliceIndex = 0;
                for(short index = 0; index < _neededCount; index++)
                {
                    neededPairIDCountArray[sliceIndex] += 1;
                    if(neededPairIDCountArray[sliceIndex] % MatchValue == 0)
                    {
                        if(sliceIndex == neededPairIDCountArray.Length - 1)
                            sliceIndex = 0;
                        else
                            sliceIndex += 1;
                    }
                }
            }
            
            return neededPairIDCountArray;
        }

        public static void shuffleIntList(List<ushort> _list)
        {
            var rng = new System.Random();
            int n = _list.Count;

            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (_list[n], _list[k]) = (_list[k], _list[n]);
            }
        }

        private static Dictionary<(ushort, ushort, ushort), TileCell> getAllTileCellPairs(List<TileCell> _tileCells)
        {
            var dic = new Dictionary<(ushort, ushort, ushort), TileCell>();
            for(short index = 0; index < _tileCells.Count; index++)
            {
                var tileCell = _tileCells[index];
                var tile = tileCell.Tile;
                tile.ID = 0;
                dic.Add((tile.Layer, tile.ColX, tile.RowY), tileCell);
            }
            return dic;
        }

        private static Dictionary<int, TileCoverCalculateData> getCoverCalculateDataPairs(TileLayer[] _layers)
        {
            var dic = new Dictionary<int, TileCoverCalculateData>();

            for(ushort layerIndex = 1; layerIndex < _layers.Length; layerIndex ++)
            {
                var topLayer = _layers[layerIndex];
                var downLayer = _layers[layerIndex - 1];

                int diffX = downLayer.ColCountX - topLayer.ColCountX;
                int diffY = downLayer.RowCountY - topLayer.RowCountY;

                bool isCoverCrossX = Mathf.Abs(diffX) % 2 == 1;
                bool isCoverCrossY = Mathf.Abs(diffY) % 2 == 1;

                diffX = diffX / 2;
                diffY = diffY / 2;

                int addX = topLayer.ColCountX > downLayer.ColCountX ? -1: 1;
                int addY = topLayer.RowCountY > downLayer.RowCountY ? -1: 1;

                dic.Add(layerIndex, new TileCoverCalculateData()
                {
                    DiffX = diffX,
                    DiffY = diffY,
                    AddBaseX = addX,
                    AddBaseY = addY,
                    IsCoverCrossX = isCoverCrossX,
                    IsCoverCrossY = isCoverCrossY
                });
            }

            return dic;
        }
    }
}
