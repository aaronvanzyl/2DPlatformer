using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public int width;
    public int height;
    enum BlockType { Any, Solid, Empty}
    BlockType[,] blocks;
    bool[,] onPath;
    PlayerController controller;
    float maxAirTime;
    float maxJumpDist;
    float maxHeight;

    void Start()
    {
        
        blocks = new BlockType[width,height];
    }

    void CalculateProperties() {
        float maxFlatHeight = controller.maxJumpTimeFlat * controller.flatJumpVelocity;
        float a = 0.5f * Physics2D.gravity.y;
        float b = controller.flatJumpVelocity;
        float c = maxFlatHeight;
        float m1 = (-b + Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);
        float m2 = (-b - Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);
        maxAirTime = Mathf.Max(m1,m2);
        maxJumpDist = controller.maxSpeed * maxAirTime;
    }

    // Update is called once per frame
    void GenerateLevel()
    {
        
    }

    void GenerateStep(int x, int y) {
        
    }

    void Connect(int x1, int y1, int x2, int y2) {
        
    }
}
