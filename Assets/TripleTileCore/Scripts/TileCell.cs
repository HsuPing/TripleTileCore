using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections.Generic;
using DG.Tweening;

namespace TripleTileCore
{
    public class TileCell : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer tileSpriteRenderer;
        [SerializeField] private SpriteRenderer iconSpriteRenderer;
        [SerializeField] private BoxCollider2D boxCollider;
        [SerializeField] private Tile tile;
        [SerializeField] private ushort blockCellCount = 0;
        [SerializeField] private List<TileCell> coverCellList = new List<TileCell>();
        private Action<TileCell, bool> onClickCallback;
        public SpriteRenderer TileSpriteRenderer => tileSpriteRenderer;
        public ushort ID => tile.ID;
        public Tile Tile => tile;
        public bool InCollection { get; set;}
       
        public void SetTile(Tile _tile)
        {
            coverCellList.Clear();
            InCollection = false;
            blockCellCount = 0;
            tile = _tile;
        }

        public void SetID(ushort _id)
        {
            tile.ID = _id;
        }

        public void SetSprite(Sprite _iconSprite)
        {
            SetDefaultOrder();
            iconSpriteRenderer.sprite = _iconSprite;
            SetCover();
        }

        public void SetCover()
        {
            bool block = blockCellCount > 0;
            tileSpriteRenderer.color = block ? Color.gray: Color.white;
            iconSpriteRenderer.color = block ? Color.gray: Color.white;
            TriggerEnable(!block);
        }

        public void SetNewData(ushort _id, Sprite _iconSprite)
        {
            SetID(_id);
            iconSpriteRenderer.sprite = _iconSprite;
        }

        public void SetOnClickCallback(Action<TileCell, bool> _onClickCallback)
        {
            onClickCallback = null;
            onClickCallback = _onClickCallback;
        }

        public void OnPointerDown()
        {
            SetOrder(100);
            this.transform.DOScale(1.3f, 0.12f);
        }

        public void OnPointerUp()
        {
            SetDefaultOrder();
            this.transform.DOScale(1f, 0.12f);
        }

        public void OnPointerClick()
        {
            Collect();
        }

        public void Collect(bool _ignoreOutRange = false)
        {
            onClickCallback?.Invoke(this, _ignoreOutRange);
            for(short index = 0; index < coverCellList.Count; index++)
            {
                coverCellList[index].BlockCellRemove();
            }
        }

        public void AutoCollect()
        {
            TriggerEnable(false);
            tileSpriteRenderer.color = Color.white;
            iconSpriteRenderer.color = Color.white;
            Collect(true);
        }

        public void TriggerEnable(bool _enable)
        {
            boxCollider.enabled = _enable;
        }

        public void BlockCellRemove()
        {
            blockCellCount -= 1;
            if(blockCellCount <= 0)
            {
                TriggerEnable(true);
                tileSpriteRenderer.color = Color.white;
                iconSpriteRenderer.color = Color.white;
            }
        }

        public void OnMatch()
        {
            this.gameObject.SetActive(false);
        }

        public void AddBlockCount()
        {
            blockCellCount += 1;
        }

        public void AddCoverTileCell(TileCell _cell)
        {
            coverCellList.Add(_cell);
            _cell.AddBlockCount();
        }

        public void Return()
        {
            InCollection = false;
            
            for(short index = 0; index < coverCellList.Count; index++)
            {
                coverCellList[index].AddBlockCount();
                coverCellList[index].SetCover();
            }
        }

        public void SetDefaultOrder()
        {
            var order = tile.Layer * 10 + tile.RowY;
            SetOrder(order);
        }

        public void SetOrder(int _order)
        {
            tileSpriteRenderer.sortingOrder = _order;
            iconSpriteRenderer.sortingOrder = _order + 1;
        }
    }
}
