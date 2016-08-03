/**
* Copyright (c) 2015-2016 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
* EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
* and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
*/

using UnityEngine;
using System.Collections;
using EasyAR;

namespace EasyARSample
{
    public class TargetOnTheFly : MonoBehaviour
    {
        private const string title = "Please enter KEY first!";
        private const string boxtitle = "===PLEASE ENTER YOUR KEY HERE===";
        private const string keyMessage = ""
            + "Steps to create the key for this sample:\n"
            + "  1. login www.easyar.com\n"
            + "  2. create app with\n"
            + "      Name: TargetOnTheFly (Unity)\n"
            + "      Bundle ID: cn.easyar.samples.unity.targetonthefly\n"
            + "  3. find the created item in the list and show key\n"
            + "  4. replace all text in TextArea with your key";

        [HideInInspector]
        public bool StartShowMessage = false;
        private bool isShowing = false;
        private ImageTargetManager imageManager;
        private FilesManager imageCreater;

        public GUISkin skin;
        private void Awake()
        {
            if (FindObjectOfType<EasyARBehaviour>().Key.Contains(boxtitle))
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayDialog(title, keyMessage, "OK");
#endif
                Debug.LogError(title + " " + keyMessage);
            }
            imageManager = FindObjectOfType<ImageTargetManager>();
            imageCreater = FindObjectOfType<FilesManager>();
        }

        void OnGUI()
        {
            if (StartShowMessage)
            {
                if (!isShowing)
                    StartCoroutine(showMessage());
                StartShowMessage = false;
            }

            GUI.Box(new Rect(Screen.width / 2 - 250, 30, 500, 60), "The box area will be used as ImageTarget. Take photo!", skin.GetStyle("Box"));
            GUI.Box(new Rect(Screen.width / 4, Screen.height / 4, Screen.width / 2, Screen.height / 2), "", skin.GetStyle("Box"));

            if (isShowing)
                GUI.Box(new Rect(Screen.width / 2 - 65, Screen.height / 2, 130, 60), "Photo Saved", skin.GetStyle("Box"));


            if (GUI.Button(new Rect(Screen.width / 2 - 80, Screen.height - 85, 160, 80), "Take Photo", skin.GetStyle("Button")))
                imageCreater.StartTakePhoto();

            if (GUI.Button(new Rect(Screen.width - 160, Screen.height - 85, 150, 80), "Clear Targets", skin.GetStyle("Button")))
            {
                imageCreater.ClearTexture();
                imageManager.ClearAllTarget();
            }
        }

        IEnumerator showMessage()
        {
            isShowing = true;
            yield return new WaitForSeconds(2f);
            isShowing = false;
        }
    }
}
