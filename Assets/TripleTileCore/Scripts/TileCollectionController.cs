using System.Collections.Generic;
using TripleTileCore;
using UnityEngine;
using DG.Tweening;
using System;

public class TileCollectionController : MonoBehaviour
{
    public class TileCollection
    {
        public TileCell TileCell { get; set;}
        public int Order { get; set;}
        public Vector3 OriginalPos { get; set;}
    }

    [SerializeField] private Transform blockBaseTransform;
    [SerializeField] private float blockSpriteScale = 1.24f;
    [SerializeField] private short blockCount = 7;
    [SerializeField] private List<TileCollection> tileCollectionList = new List<TileCollection>();

    [SerializeField] private int tilesCount = 0;  
    [SerializeField] private int collectCount = 0;  

    [Header("動畫")]
    [SerializeField] private float tileMoveDuration = 0.25f;

    private Action passStageCallback;
    private Action failStageCallback;

    public void Init(Action _passStageCallback = null, Action _failStageCallback = null)
    {
        passStageCallback = _passStageCallback;
        failStageCallback = _failStageCallback;
    }

    public void SetData(int _tileCount)
    {
        tilesCount = _tileCount;
        collectCount = 0;
        tileCollectionList.Clear();
    }

    public void AddTileCell(TileCell _cell, bool _ignoreOutRange = false)
    {
        Sequence sequence = DOTween.Sequence();

        collectCount += 1;
        TileCollection tileCollection = new TileCollection() { TileCell = _cell, Order = collectCount, OriginalPos = _cell.transform.position};

        int addIndex = -1;

        TileCollection tileCollectionCompare1 = null;
        TileCollection tileCollectionCompare2 = null;

        _cell.SetOrder(100);
        _cell.TriggerEnable(false);
        _cell.InCollection = true;

        for(int index = tileCollectionList.Count - 1; index >= 0; index--)
        {
            var cell = tileCollectionList[index].TileCell;
            if(_cell.Tile.ID == cell.Tile.ID)
            {
                if(addIndex >= 0)
                {
                    tileCollectionCompare2 = tileCollectionList[index];
                    break;
                }
                else
                {
                    addIndex = index;
                    tileCollectionCompare1 = tileCollectionList[index];
                }
            }
        }

        var tCell = _cell.transform;

        if(tileCollectionCompare1 != null)
        {
            addIndex += 1;
            tileCollectionList.Insert(addIndex, tileCollection);
            sequence.Insert(0, tCell.DOMove(getCellPosition(addIndex), tileMoveDuration));
            for(int index = addIndex; index < tileCollectionList.Count; index++)
            {
                sequence.Insert(0, tileCollectionList[index].TileCell.transform.DOMove(getCellPosition(index), tileMoveDuration));
            }

            if(tileCollectionCompare2 != null)  
            {
                Handheld.Vibrate();  //手機震動
                tileCollectionList.Remove(tileCollection);
                tileCollectionList.Remove(tileCollectionCompare1);
                tileCollectionList.Remove(tileCollectionCompare2);

                tilesCount = tilesCount - TripleTileProcessor.MatchValue;
                if(tilesCount <= 0)
                    passStageCallback?.Invoke();

                sequence.OnComplete(() =>
                {
                    var t1 = tileCollectionCompare1.TileCell.transform;
                    var t2 = tileCollectionCompare2.TileCell.transform;
                    t1.DOMove(getRemovePosition(t1.position), tileMoveDuration).OnComplete(() => tileCollectionCompare1.TileCell.OnMatch());
                    t2.DOMove(getRemovePosition(t2.position), tileMoveDuration).OnComplete(() => tileCollectionCompare2.TileCell.OnMatch());
                    tCell.DOMove(getRemovePosition(tCell.position), tileMoveDuration).OnComplete(() => _cell.OnMatch());

                    collectionMove();
                });
            }
        }
        else
        {
            tileCollectionList.Add(tileCollection);
            sequence.Insert(0, tCell.DOMove(getCellPosition(tileCollectionList.Count - 1), tileMoveDuration));
        }

        if(tileCollectionList.Count >= blockCount && !_ignoreOutRange)
        {
            failStageCallback?.Invoke();
        }
    }

    public bool ReturnLastTile()
    {
        if(tileCollectionList.Count == 0)
            return false;
        TileCollection tileCollection = tileCollectionList[0];
        for(short index = 1; index < tileCollectionList.Count; index++)
        {
            var tCollection = tileCollectionList[index];
            if(tileCollection.Order < tCollection.Order)
                tileCollection = tCollection;
        }

        var t = tileCollection.TileCell.transform;
        tileCollectionList.Remove(tileCollection);
        tileCollection.TileCell.Return();
        t.DOMove(tileCollection.OriginalPos, tileMoveDuration)
            .OnComplete(()=>
            {
                tileCollection.TileCell.TriggerEnable(true);
                tileCollection.TileCell.SetDefaultOrder();
            });

        collectionMove();
        return true;
    }

    private void collectionMove()
    {
        for(int index = 0; index < tileCollectionList.Count; index++)
        {
            var t = tileCollectionList[index].TileCell.transform;
            var moveTo = getCellPosition(index);
            if(t.position != moveTo)
                t.DOMove(moveTo, tileMoveDuration);
        }
    }

    private Vector3 getCellPosition(int _index)
    {
        return new Vector3(blockBaseTransform.position.x + _index * blockSpriteScale, blockBaseTransform.position.y, 0);
    }

    private Vector2 getRemovePosition(Vector2 _pos)
    {
        return new Vector2(_pos.x, blockBaseTransform.position.y + 1.8f);
    }
}
