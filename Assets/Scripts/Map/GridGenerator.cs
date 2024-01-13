using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO.Compression;

public class GridGenerator : MonoBehaviour {

	public int width;
	public int height;

	public string seed;
	public bool useRandomSeed;

	[Range(0,100)]
	public int noiseDensity;
    public int iterations;

	int[,] noiseGrid;

	void Start() {
        useRandomSeed = true;
        width = 64;
        height = 64;
        iterations = 5;
        noiseDensity = 50;
		GenerateMap();
	}
	void Update() {
		if (Input.GetKeyDown(KeyCode.N)) {
			GenerateMap();
		}
	}
	void GenerateMap() {
		noiseGrid = new int[width,height];
		RandomFillMap();

		for (int i = 0; i < iterations; i ++) {
			SmoothMap();
		}
		MeshGenerator meshgen = GetComponent<MeshGenerator>();
		meshgen.GenerateMesh(noiseGrid, 1f);
	}

	List<List<Coord>> GetRegions(int tileType) {
		List<List<Coord>> regions = new List<List<Coord>>();
		int[,] mapFlags = new int[width, height];

		for (int x = 0; x <= width; x++) {
			for (int y = 0; y <= height; y++) {
				if (mapFlags[x, y] == 0 && noiseGrid[x, y] == tileType) {
					List<Coord> newRegion = GetRegionTiles(x, y);
					regions.Add(newRegion);
					foreach (Coord tile in newRegion) {
						mapFlags[tile.tileX, tile.tileY] = 1;
					}
				}
			}
		}
		return regions;
	}

	List<Coord> GetRegionTiles(int startX, int startY) {
		List<Coord> tiles = new List<Coord>();
		int[,] mapFlags = new int[width,height];
		int tileType = noiseGrid[startX,startY];

		Queue<Coord> queue = new Queue<Coord>();
		queue.Enqueue(new Coord(startX, startY));
		mapFlags[startX, startY] = 1;

		while (queue.Count > 0) {
			Coord tile =  queue.Dequeue();
			tiles.Add(tile);
			for (int x = tile.tileX; x <= tile.tileX; x++) {
				for (int y = tile.tileY; y <= tile.tileY; y++) {
					if (IsInMap(x, y) && (y == tile.tileY || x == tile.tileX)) {
						if (mapFlags[x, y] == 0 && noiseGrid[x, y] == tileType) {
							mapFlags[x, y] = 1;
							queue.Enqueue(new Coord(x, y));
						}
					}
				}
			}
		}
		return tiles;
	}
	
	bool IsInMap(int x, int y) {
		return x >= 0 && x < width && y >= 0 && y < height;
	}
	void RandomFillMap() {
        seed = Time.time.ToString();

		System.Random rand = new System.Random(seed.GetHashCode());

		for (int i = 0; i < width; i ++) {
			for (int j = 0; j < height; j ++) {
				if (i == 0 || i == width-1 || j == 0 || j == height -1) {
					noiseGrid[i,j] = 1;
				}
				else {
					noiseGrid[i,j] = (rand.Next(0,100) < noiseDensity)? 1: 0;
				}
			}
		}
	}
	void SmoothMap() {
		for (int i = 0; i < width; i ++) {
			for (int j = 0; j < height; j ++) {
				int neighbourWallTiles = GetSurroundingWallCount(i,j);

				if (neighbourWallTiles > 4)
					noiseGrid[i,j] = 1;
				else if (neighbourWallTiles < 4)
					noiseGrid[i,j] = 0;

			}
		}
	}
	struct Coord {
        public int tileX;
        public int tileY;

        public Coord(int x, int y) {
            tileX = x;
            tileY = y;
        }
    }
	int GetSurroundingWallCount(int gridX, int gridY) {
		int wallCount = 0;
		for (int neighborX = gridX - 1; neighborX <= gridX + 1; neighborX ++) {
			for (int neighborY = gridY - 1; neighborY <= gridY + 1; neighborY ++) {
				if (IsInMap(neighborX, neighborY)) {
					if (neighborX != gridX || neighborY != gridY) {
						wallCount += noiseGrid[neighborX,neighborY];
					}
				}
				else {
					wallCount ++;
				}
			}
		}

		return wallCount;
	}
	void OnDrawGizmos() {
		// if (noiseGrid != null) {
		// 	for (int i = 0; i < width; i ++) {
		// 		for (int j = 0; j < height; j ++) {
		// 			Gizmos.color = (noiseGrid[i,j] == 1)?Color.black:Color.white;
		// 			Vector3 pos = new Vector3(-width/2 + i + .5f, -height/2 + j+.5f);
		// 			Gizmos.DrawCube(pos,Vector3.one);
		// 		}
		// 	}
		// }
	}

}