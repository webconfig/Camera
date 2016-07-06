//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.EventSystems;

//public class UI_Control_Table_Drag : MonoBehaviour
//{
//    public RectTransform DragObj;
//    public UI_Control_Table table;

//    public float old_y = 0;
//    public float event_start_y = 0;
//    public float current_y = 0;

//    public void OnBeginDrag(PointerEventData eventData)
//    {
//        old_y = DragObj.localPosition.y;
//        event_start_y = eventData.position.y;
//    }

//    public void OnDrag(PointerEventData eventData)
//    {
//        if (table.can_drag == 0)
//        {
//            current_y = old_y + (eventData.position.y - event_start_y);
//            DragObj.localPosition = new Vector3(0, current_y, 0);
//        }

//        if (table.can_drag == 2)
//        {
//            old_y = DragObj.localPosition.y;
//            event_start_y = eventData.position.y;
//            table.can_drag = 0;
//        }
//    }

//    public void OnEndDrag(PointerEventData eventData)
//    {
//    }
//}

