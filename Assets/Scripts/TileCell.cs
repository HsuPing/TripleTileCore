using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class TileCell : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private EventTrigger eventTrigger; 
    [SerializeField] private Tile tile;
    [SerializeField] private int blockCellCount = 0;
    private Action<TileCell> onClickCallback;
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public ushort Id => tile.Id;
    public UnityEvent RemoveEvent;

    public void SetData(Tile _tile, int _order, Action<TileCell> _callback)
    {
        tile = _tile;
        spriteRenderer.sortingOrder = _order;
        onClickCallback = _callback;
    }

    public void SetBlockState()
    {
        bool block = blockCellCount > 0;
        spriteRenderer.color = block ? Color.gray: Color.white;
        TriggerEnable(!block);
    }

    public void OnPointerDown()
    {
        Debug.Log("ON Pointer Down " + this.gameObject.name);
        //TODO 放大動畫
    }

    public void OnPointerUp()
    {
        Debug.Log("ON Pointer Up " + this.gameObject.name);
        //TODO 縮小
    }

    public void OnPointerClick()
    {
        Debug.Log("ON Pointer Click " + this.gameObject.name);
        //TODO 移至插槽
        onClickCallback?.Invoke(this);
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
            spriteRenderer.color = Color.white;
        }
    }

    public void OnMatch()
    {
        this.gameObject.SetActive(false);
        RemoveEvent.RemoveAllListeners();
    }

    public void AddBlockCount()
    {
        blockCellCount += 1;
    }

    private void OnDestroy() 
    {
        RemoveEvent.RemoveAllListeners();
    }

    void OnDrawGizmos() 
    {
        if(spriteRenderer.color == Color.white)
        {
            GUI.color = Color.red;
            UnityEditor.Handles.Label(this.transform.position, Id.ToString());
        }
    }
}
