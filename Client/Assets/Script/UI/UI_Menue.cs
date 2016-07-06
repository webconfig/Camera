using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 登录
/// </summary>
public class UI_Menue : UI_Base
{
    public Button Btn_JS, Btn_JSJS;
    public override void UI_Start()
    {
        Btn_JS.onClick.AddListener(Btn_JS_Click);
        Btn_JSJS.onClick.AddListener(Btn_JSJS_Click);
    }

    public override void UI_End()
    {
        Btn_JS.onClick.RemoveAllListeners();
        Btn_JSJS.onClick.RemoveAllListeners();
    }


    private void Btn_JS_Click()
    {
        UI_Manager.Instance.Show("UI_Camera");
    }
    private void Btn_JSJS_Click()
    {

    }
}
