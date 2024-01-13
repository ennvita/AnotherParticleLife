using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class TakeScreenshot : MonoBehaviour
{
    public Camera screenshotCamera;
    string dtString;

    void Update() {
        if (Input.GetKeyDown(KeyCode.X)) {
            SaveView(screenshotCamera);
        }
    }

    void SaveView(Camera cam) {
        RenderTexture screenTexture = new RenderTexture(Screen.width, Screen.height, 16);
        cam.targetTexture = screenTexture;
        RenderTexture.active = screenTexture;
        cam.Render();

        Texture2D renderedTexture = new Texture2D(Screen.width, Screen.height);
        renderedTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        RenderTexture.active = null;

        byte[] byteArray = renderedTexture.EncodeToPNG();
        
        DtToString(DateTime.Now);
        System.IO.File.WriteAllBytes(Application.dataPath + "/Screenshots/cameracapture" + dtString + ".png", byteArray);
    }

    void DtToString(DateTime dt) {
        dtString = dt.ToString("MMddyyyyhhmm");
        Regex.Replace(dtString, "[^0-9]", "");
    }
}
