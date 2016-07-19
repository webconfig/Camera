using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Control_Table : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform RectTran;
    public RectTransform RowParents;
    public GameObject Row_Prefab;
    public float RowHeight = 100;
    public float TotalHeight = 0;
    public List<UI_Control_Table_Row> Rows=new List<UI_Control_Table_Row>();
    public UI_Control_Table_Row CurrentRow;
    public event TableRowDeleteEvnet RowDeleteEvent;
    public void Clear()
    {
        for (int i = 0; i < Rows.Count; i++)
        {
            Rows[i].Clear();
            Destroy(Rows[i].gameObject);
        }
        Rows.Clear();
        RectTransform rtf = RowParents.GetComponent<RectTransform>();
        rtf.sizeDelta = new Vector2(rtf.sizeDelta.x, 0);
    }

    public void AddRow(Dictionary<string,string> data,CallBack<string,int> Click)
    {
        TotalHeight = Rows.Count * RowHeight;

        GameObject row = Instantiate(Row_Prefab) as GameObject;
        RectTransform rt = row.GetComponent<RectTransform>();
        rt.SetParent(RowParents);
        rt.localPosition = new Vector3(0, TotalHeight * -1, 0);
        rt.localScale = new Vector3(1, 1, 1);
        row.SetActive(true);
        UI_Control_Table_Row table_row = row.GetComponent<UI_Control_Table_Row>();
        table_row.Table = this;
        table_row.SetColor(Rows.Count % 2 == 0 ? UI_Manager.Instance.NormalColor1 : UI_Manager.Instance.NormalColor2);
        table_row.index = Rows.Count;
        table_row.DataBind(data, Click);
        Rows.Add(table_row);
        RectTransform rtf = RowParents.GetComponent<RectTransform>();
        rtf.sizeDelta = new Vector2(rtf.sizeDelta.x, TotalHeight + RowHeight);
    }


    public void RowDelete(UI_Control_Table_Row row)
    {
        if(RowDeleteEvent!=null)
        {
            RowDeleteEvent(row);
        }
    }

    /// <summary>
    /// 删除一行
    /// </summary>
    /// <param name="row"></param>
    public void RemoveRow(UI_Control_Table_Row row)
    {
        Rows.Remove(row);
        Destroy(row.gameObject);


        RectTransform rtf = RowParents.GetComponent<RectTransform>();
        rtf.sizeDelta = new Vector2(rtf.sizeDelta.x, Rows.Count * RowHeight);
        for (int i = 0; i < Rows.Count; i++)
        {
            RectTransform rt = Rows[i].gameObject.GetComponent<RectTransform>();
            rt.anchoredPosition3D = new Vector3(0, i * RowHeight * -1, 0);
            Rows[i].SetColor(i % 2 == 0 ? UI_Manager.Instance.NormalColor1 : UI_Manager.Instance.NormalColor2);
        }
    }

    public void ToEnd()
    {
        if (Rows.Count * RowHeight > RectTran.rect.height)
        {
            RowParents.localPosition = new Vector3(0, RowParents.rect.height - Height, 0);
        }
    }

    private float _height = -1;
    public float Height
    {
        get
        {
            if (_height < 0)
            {
                _height = GetComponent<RectTransform>().rect.height;
            }
            return _height;
        }
    }

    public void DeleteTop(int num)
    {
        int k = 0;
        for (int i = 0; i < Rows.Count; i++)
        {
            if(k<num)
            {
                k++;
                Destroy(Rows[i].gameObject);
                Rows.RemoveAt(i);
                i--;
            }
        }
        RectTransform rtf = RowParents.GetComponent<RectTransform>();
        rtf.sizeDelta = new Vector2(rtf.sizeDelta.x, Rows.Count * RowHeight);
        for (int i = 0; i < Rows.Count; i++)
        {
            RectTransform rt = Rows[i].gameObject.GetComponent<RectTransform>();
            rt.anchoredPosition3D = new Vector3(0, i * RowHeight * -1, 0);
            //Rows[i].SetColor(i % 2 == 0 ? UI_Manager.Instance.NormalColor1 : UI_Manager.Instance.NormalColor2);
        }
        RowParents.localPosition = new Vector3(0, RowParents.localPosition.y - num * RowHeight, 0);
    }

    public void DeleteBottom(int num)
    {
        for (int i = 0; i < num; i++)
        {
            if(Rows.Count>0)
            {
                Destroy(Rows[Rows.Count-1].gameObject);
                Rows.RemoveAt(Rows.Count - 1);
            }
        }

        RectTransform rtf = RowParents.GetComponent<RectTransform>();
        rtf.sizeDelta = new Vector2(rtf.sizeDelta.x, Rows.Count * RowHeight);
    }

    public void AddRowToTop(List<Dictionary<string,string>> datas)
    {
        for (int i = 0; i < datas.Count; i++)
        {
            GameObject row = Instantiate(Row_Prefab) as GameObject;
            RectTransform rt = row.GetComponent<RectTransform>();
            rt.SetParent(RowParents);
            rt.localScale = new Vector3(1, 1, 1);
            row.SetActive(true);
            UI_Control_Table_Row table_row = row.GetComponent<UI_Control_Table_Row>();
            table_row.Table = this;
            table_row.index = i;
            table_row.DataBind(datas[i], null);
            Rows.Insert(i,table_row);
        }

        RectTransform rtf = RowParents.GetComponent<RectTransform>();
        rtf.sizeDelta = new Vector2(rtf.sizeDelta.x, Rows.Count * RowHeight);
        for (int i = 0; i < Rows.Count; i++)
        {
            RectTransform rt = Rows[i].gameObject.GetComponent<RectTransform>();
            rt.anchoredPosition3D = new Vector3(0, i * RowHeight * -1, 0);
            Rows[i].gameObject.GetComponent<RectTransform>().GetComponent<UI_Control_Table_Row>().index = i;
            Rows[i].SetColor(i % 2 == 0 ? UI_Manager.Instance.NormalColor1 : UI_Manager.Instance.NormalColor2);
        }
        RowParents.localPosition = new Vector3(0, RowParents.localPosition.y + datas.Count * RowHeight, 0);

    }

    #region 拖动
    public int can_drag = 0;
    public float old_y = 0;
    public float event_start_y = 0;
    public float current_y = 0;
    public bool GoToEnd = false;
    public bool GoToTop = false;

    public float MoveTime = 0.5f;
    public float StartMoveTime = 0;
    public float MoveStartPosition,MoveEndPosition;
    public float MT = 0;
    public float DragSpeed = 1.5f;
    public void OnBeginDrag(PointerEventData eventData)
    {
        old_y = RowParents.localPosition.y;
        event_start_y = eventData.position.y;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (can_drag == 0)
        {
            current_y = old_y + (eventData.position.y - event_start_y) * DragSpeed;
            RowParents.localPosition = new Vector3(0, current_y, 0);
        }

        if (can_drag == 2)
        {
            old_y = RowParents.localPosition.y;
            event_start_y = eventData.position.y;
            can_drag = 0;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (Rows.Count * RowHeight <= RectTran.rect.height || RowParents.localPosition.y < 0)
        {
            if (can_drag == 0)
            {
                //Debug.Log("ToTop");
                MoveEndPosition = 0;
                MoveStartPosition = RowParents.localPosition.y;
                GoToTop = true;
                StartMoveTime = Time.time;
            }
        }
        else if (RowParents.localPosition.y + Height > RowParents.rect.height)
        {
            if (can_drag == 0)
            {
                //Debug.Log("ToEnd");
                MoveEndPosition = RowParents.rect.height - Height;
                MoveStartPosition = RowParents.localPosition.y;
                GoToEnd = true;
                StartMoveTime = Time.time;
            }

        }
    }
    #endregion
    void Update()
    {
        if (can_drag == 0)
        {
            if (GoToEnd)
            {
                MT = (Time.time - StartMoveTime) / MoveTime;
                if (MT >= 1) { MT = 1; GoToEnd = false;  }
                RowParents.localPosition = new Vector3(0, Mathf.Lerp(MoveStartPosition, MoveEndPosition, MT), 0);
            }
            else if (GoToTop)
            {
                MT = (Time.time - StartMoveTime) / MoveTime;
                if (MT >= 1) { MT = 1; GoToTop = false; }
                RowParents.localPosition = new Vector3(0, Mathf.Lerp(MoveStartPosition, MoveEndPosition, MT), 0);
            }
        }
    }
}

public delegate void TableRowDeleteEvnet(UI_Control_Table_Row row);

