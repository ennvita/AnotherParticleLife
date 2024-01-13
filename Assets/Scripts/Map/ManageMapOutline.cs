using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageMapOutline : MonoBehaviour
{
    LineRenderer mapBounds;
    private Vector3 bottomLeft;
    private Vector3 topLeft;
    private Vector3 topRight;
    private Vector3 bottomRight;

    void Awake()
    {
        mapBounds = gameObject.AddComponent<LineRenderer>();
    }

    public void UpdateMapOutline(int width, int height, float particleRadius) {
        mapBounds = GetComponent<LineRenderer>();
        mapBounds.material = new Material(Shader.Find("Sprites/Default"));
        mapBounds.startColor = Color.white;
        mapBounds.endColor = Color.white;
        mapBounds.widthMultiplier = 5f;

        bottomLeft = new Vector3(0 - 2.5f - particleRadius, 0 - 2.5f - particleRadius, 10);
        topLeft = new Vector3(0 - 2.5f - particleRadius, height + 2.5f + particleRadius, 10);
        topRight = new Vector3(width + 2.5f + particleRadius, height + 2.5f + particleRadius, 10);
        bottomRight = new Vector3(width + 2.5f + particleRadius, 0 - 2.5f - particleRadius, 10);
        Vector3 endLeft = new Vector3(0 - 2.5f - particleRadius, 0 - 2.5f - particleRadius, 10);

        Vector3[] mapCorners= new Vector3[5] {bottomLeft, topLeft, topRight, bottomRight, endLeft};
        mapBounds.positionCount = mapCorners.Length;
        mapBounds.SetPositions(mapCorners);
    }
}
