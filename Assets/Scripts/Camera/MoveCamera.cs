using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MoveCamera : MonoBehaviour
{
    private Vector3 dragOrigin;
    private Vector3 posDelta;
    private float zoomLevel;
    private float speed = 1000;
    private float minZoom = 25;
    private float maxZoom = 2500;
    private float newZoom;
    private Camera primaryCam;
    public Camera secondaryCam;
    public List<Camera> cams;
    private bool IsDragging;
    private int mapWidth;
    private int mapHeight;
    GameObject engine;

    private void Start() {
        engine = GameObject.Find("Engine");
        ChangeOptions changeOptions = engine.gameObject.GetComponent<ChangeOptions>();

        mapHeight = changeOptions._mapHeight; // temporary while I figure out the interaction matrix
        mapWidth = changeOptions._mapWidth;

        primaryCam = Camera.main;
        cams.Add(primaryCam);
        cams.Add(secondaryCam);

        for (int i = 0; i < cams.Count; i++)
        {
            cams[i].transform.position = new Vector3(mapWidth / 2, mapHeight / 2, -10);
            cams[i].orthographicSize = 80;
        }
    }

    public void GetCurrentMapDimensions() {
        engine = GameObject.Find("Engine");
        ChangeOptions changeOptions = engine.gameObject.GetComponent<ChangeOptions>();

        mapHeight = changeOptions._mapHeight;
        mapWidth = changeOptions._mapWidth;
    }

    public void OnDrag(InputAction.CallbackContext ctx) {
        if (ctx.started) dragOrigin = GetMousePosition;
        IsDragging = ctx.started || ctx.performed;
    }

    void Update() {
        float mapArea = mapWidth * mapHeight;
        float sensitivity = Mathf.Log(mapArea) * 25; // for the still impatient brother...

        for (int i = 0; i < cams.Count; i++) {
            if (Input.mouseScrollDelta.y != 0) {
                zoomLevel += -(Input.mouseScrollDelta.y * sensitivity);
                zoomLevel = Mathf.Clamp(zoomLevel, minZoom, maxZoom);

                newZoom = Mathf.MoveTowards(cams[i].orthographicSize, zoomLevel, speed * Time.deltaTime);
                cams[i].orthographicSize = newZoom;

                zoomLevel = newZoom;
            }
        }
    }

    private void LateUpdate() {
        if (IsDragging) {
            posDelta = GetMousePosition - transform.position;
            transform.position = dragOrigin - posDelta;
        }
    }

    public void ResetCameraPosition() {
        GetCurrentMapDimensions();

        for (int i = 0; i < cams.Count; i++)
        {
            cams[i].transform.position = new Vector3(mapWidth / 2, mapHeight / 2, -10);
        }
    }

    private Vector3 GetMousePosition => primaryCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
}