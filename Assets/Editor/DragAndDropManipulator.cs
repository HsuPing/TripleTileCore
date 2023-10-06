using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DragAndDropManipulator : PointerManipulator
{
    public DragAndDropManipulator(DragAndDropController _controller, VisualElement target, VisualElement root, int id, bool onDefautSlot = true)
    {
        this.controller = _controller;
        this.target = target;
        this.root = root;
        this.TileID = id;
        parent = this.target.parent;
        this.onDefautSlot = onDefautSlot;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
        target.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
        target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
        target.RegisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
        target.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
        target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
        target.UnregisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
    }
    
    public int TileID { get; }
    private const short editor_top_bar_height = 21;
    private const string slot_class_name = "slot";

    private Vector2 targetStartPosition { get; set; }
    private Vector3 pointerStartPosition { get; set; }
    private DragAndDropController controller { get;}

    private bool enabled { get; set; }
    private bool onDefautSlot { get; set; } = true;

    private VisualElement root { get; }
    private VisualElement parent { get; set;}

    /// <summary>
    /// 此物件的父物件改為root，並轉換其座標至對應位置。註冊PointerId，拖曳Trigger(enables)開啟。
    /// </summary>
    private void PointerDownHandler(PointerDownEvent evt)
    {
        var pos = root.WorldToLocal(target.LocalToWorld(target.transform.position));
        targetStartPosition = new Vector2(pos.x, pos.y);
        pointerStartPosition = evt.position;
        target.CapturePointer(evt.pointerId);
        root.Add(this.target);
        target.transform.position = targetStartPosition;
        enabled = true;
    }

    /// <summary>
    /// 拖曳
    /// </summary>
    private void PointerMoveHandler(PointerMoveEvent evt)
    {
        if (enabled && target.HasPointerCapture(evt.pointerId))
        {
            Vector3 pointerDelta = evt.position - pointerStartPosition;
            target.transform.position = new Vector2(
                Mathf.Clamp(targetStartPosition.x + pointerDelta.x ,0 , root.worldBound.width - target.layout.width),
                Mathf.Clamp(targetStartPosition.y + pointerDelta.y ,0 , root.worldBound.height - target.layout.height));
        }
    }

    /// <summary>
    /// 點擊釋放，Release PointerId
    /// </summary>
    private void PointerUpHandler(PointerUpEvent evt)
    {
        if (enabled && target.HasPointerCapture(evt.pointerId))
        {
            target.ReleasePointer(evt.pointerId);
        }
    }

    /// <summary>
    /// ReleasePointer後觸發。搜尋覆蓋且最鄰近的slot。
    /// </summary>
    private void PointerCaptureOutHandler(PointerCaptureOutEvent evt)
    {
        if (enabled)
        {
            UQueryBuilder<VisualElement> allSlots = root.Query<VisualElement>(className: slot_class_name);
           
            VisualElement closestOverlappingSlot = FindClosestSlot(allSlots);
            if(closestOverlappingSlot != null)
            {
                if(closestOverlappingSlot.childCount > 0)
                {
                    closestOverlappingSlot.RemoveAt(0);
                }

                // int putIndex = root.IndexOf(closestOverlappingSlot);
                // Debug.Log("Add Index: " + putIndex);
                closestOverlappingSlot.Add(this.target);
                this.parent = closestOverlappingSlot;
                //controller.DragInSlotCallback?.Invoke(putIndex, TileID);  //TODO
                if(onDefautSlot)
                {                   
                    onDefautSlot = false;
                }
            }
            else
            {
                if(onDefautSlot)
                    parent.Add(this.target);
                else
                {
                    //TODO
                    target.RemoveFromHierarchy();
                }
            }

            this.target.transform.position = Vector3.zero;
            enabled = false;
        }
    }

    private VisualElement FindClosestSlot(UQueryBuilder<VisualElement> slots)
    {
        List<VisualElement> slotsList = slots.ToList();

        float bestDistanceSq = float.MaxValue;
        VisualElement closest = null;
        Vector3 slotPos = Vector3.zero;
        Vector2 slotLocalToWorld = Vector2.zero;
        foreach (VisualElement slot in slotsList)
        {
            if(!OverlapsTarget(slot))
                continue;
            slotLocalToWorld = slot.LocalToWorld(slot.transform.position);
            slotPos = new Vector3(slotLocalToWorld.x, slotLocalToWorld.y - editor_top_bar_height, 0);
            Vector3 displacement = slotPos - target.transform.position;
            
            float distanceSq = displacement.sqrMagnitude;
            if (distanceSq < bestDistanceSq)
            {
                bestDistanceSq = distanceSq;
                closest = slot;
            }
        }
     
        return closest;
    }

    private bool OverlapsTarget(VisualElement slot)
    {
        return target.worldBound.Overlaps(slot.worldBound);
    }
}