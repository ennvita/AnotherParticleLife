using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class GameData {
    public float rmax;
    public float beta;
    public float forceFactor;
    public int particleCount;
    public float particleRadius;
    public int particleVertices;
    public int mapWidth;
    public int mapHeight;
    public float[] interactionMatrix;

    // initial values on a new game
    public GameData() {
        rmax = 15.0f;
        beta = 0.3f;
        forceFactor = 0.025f;
        particleCount = 30000;
        particleRadius = 0.25f;
        particleVertices = 30;
        mapWidth = 500;
        mapHeight = 500;
        interactionMatrix = new float[36] {
            -0.259193f, 0.140887f, -0.741252f, 0.856589f, 0.831565f, 0.701352f,
            0.770930f, -0.199705f, 0.845173f, 0.641216f, -0.136704f, -0.830708f,
            -0.523549f, -0.385346f, -0.228460f, 0.506021f, 0.388820f, 0.352685f,
            -0.496070f, -0.795348f, 0.764045f, 0.119144f, 0.378547f, -0.355109f,
            -0.711210f, 0.238660f, 0.462211f, 0.287729f, 0.687889f, 0.644950f,
            0.139160f, -0.411353f, 0.791219f, 0.477795f, -0.554215f, 0.445136f
        };
    }
}