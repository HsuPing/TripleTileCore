using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DragAndDropManipulator : PointerManipulator
{
    public DragAndDropManipulator(VisualElement target, VisualElement root, Action moveSlotCallback)
    {
        this.target = target;
        this.root = root;
        parent = this.target.parent;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        // 註冊點擊拖曳事件
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

    private const short editor_top_bar_height = 21;
    private const string slot_class_name = "slot";

    private Vector2 targetStartPosition { get; set; }
    private Vector3 pointerStartPosition { get; set; }

    private bool enabled { get; set; }
    private bool onDefautSlot { get; set; } = true;

    private VisualElement root { get; }
    private VisualElement parent { get; set;}

    /// <summary>
    /// 此物件的父物件改為root，並轉換其座標至對應位置。註冊PointerId，拖曳Trigger(enables)開啟。
    /// </summary>
    private void PointerDownHandler(PointerDownEvent evt)
    {
        var localToWorldPos = target.LocalToWorld(target.transform.position);
        targetStartPosition = new Vector2(localToWorldPos.x, localToWorldPos.y - editor_top_bar_height);
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
                Mathf.Max(0, targetStartPosition.x + pointerDelta.x),
                Mathf.Max(0, targetStartPosition.y + pointerDelta.y));
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
                closestOverlappingSlot.Add(this.target);
                this.parent = closestOverlappingSlot;
                //建立新的物件在default_slot                                
                onDefautSlot = false;
            }
            else
            {
                if(onDefautSlot)
                    parent.Add(this.target);
                else
                {
                    target.parent.Remove(this.target);
                    UnregisterCallbacksFromTarget();
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