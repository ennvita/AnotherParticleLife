using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class showDebuggingHUD : MonoBehaviour
{
    public TMP_Text showFPS;
    public TMP_Text showDT;
    public float fpsDT;
    public float timeCounter;

    void Awake () {
        timeCounter = 0;
    }

    // Update is called once per frame
    void Update()
    {
    fpsDT += (Time.deltaTime -fpsDT) * 0.1f;
        float fps = 1.0f /fpsDT;
        showFPS.SetText(Mathf.Ceil(fps).ToString());
        timeCounter = Time.time;
        showDT.SetText((int)timeCounter + "s Elapsed");
    }
}
