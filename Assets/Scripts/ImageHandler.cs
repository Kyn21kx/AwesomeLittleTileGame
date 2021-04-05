using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Manages an image, creating a 3x5 matrix containing each a part of an image
/// </summary>
public class ImageHandler : MonoBehaviour {

	public struct Tile {

		public GameObject gameObject;
		public Texture2D texture;
		public bool IsLast { get { return !gameObject.activeSelf; } }

		public static bool operator ==(Tile a, Tile b) {
			return a.gameObject == b.gameObject && a.texture == b.texture;
		}

		public static bool operator !=(Tile a, Tile b) {
			return !(a == b);
		}

		public void SetTexture(Texture2D texture) {
			var renderer = gameObject.transform.GetChild(0).GetComponent<Renderer>();
			renderer.material.SetTexture("_MainTex", texture);
			this.texture = texture;
		}

	}

	#region Variables
	[SerializeField]
	private Texture2D originalImage;
	[SerializeField]
	private GameObject baseTile;
	[SerializeField]
	private Vector3 startingPos;
	[SerializeField]
	private float scaleDownFactor;
	[SerializeField]
	private float transformDownFactor;
	public int horizontal;
	public int vertical;
	/// <summary>
	/// Once a movement is completed, we can check for the tile matrix + lastTile to be equal to the original matrix
	/// </summary>
	public Tile lastTile;
	private Tile[,] originalTM;
	public Tile[,] TileMatrix { get; private set; }
	#endregion

	private void Awake() {
		SpawnParts();
	}

	public void SpawnParts() {
		TileMatrix = GetTiles();
		Tile[,] shuffled = (Tile[,])TileMatrix.Clone();
		Utils.Randomize(shuffled);
		for (int i = 0; i < horizontal; i++) {
			for (int j = 0; j < vertical; j++) {
				TileMatrix[i, j].SetTexture(shuffled[i, j].texture);
			}
		}
		SetGlobalTiles();
	}

	private void SetGlobalTiles() {
		int indexI = TileMatrix.GetLength(0) - 1, indexJ = TileMatrix.GetLength(1) - 1;
		lastTile = TileMatrix[indexI, indexJ];
		TileMatrix[indexI, 0].gameObject.SetActive(false);
		originalTM = (Tile[,])TileMatrix.Clone();
	}

	private Tile[,] GetTiles() {
		float nWidth = originalImage.width / (float)horizontal;
		float nHeight = originalImage.height / (float)vertical;

		Tile[,] tiles = new Tile[horizontal, vertical];
		
		int roundedHeight = Mathf.FloorToInt(nHeight);
		int roundedWidth = Mathf.FloorToInt(nHeight);

		//Starting pos + nWidth gives you the next positions
		for (int i = 0; i < horizontal; i++) {
			for (int j = 0; j < vertical; j++) {
				Texture2D nTexture = new Texture2D(roundedWidth, roundedHeight, originalImage.format, false);
				(int x, int y) pos;
				pos.x = roundedWidth * i;
				pos.y = roundedHeight * j;
				Color[] pixelChunk = originalImage.GetPixels(pos.x, pos.y, roundedWidth, roundedHeight);
				nTexture.SetPixels(pixelChunk);
				byte[] bytes = nTexture.EncodeToPNG();
				//Create a new game object with the proper sprite attached
				var spawned = Instantiate(baseTile, this.transform);
				spawned.transform.GetChild(0).name = $"Tile {i},{j}";
				Vector3 position = new Vector3(startingPos.x + (i * nWidth), startingPos.y + (j * nHeight), 0f);
				//spawned.transform.parent = null;
				//Spawn them in a random position within the
				spawned.transform.position = position / transformDownFactor;
				spawned.transform.localScale = new Vector3(nWidth - 1, nHeight - 1, spawned.transform.localScale.z) / scaleDownFactor;
				//Think of moving these to hard drive to avoid using too much memory
				nTexture.LoadImage(bytes);
				Tile t = new Tile();
				t.gameObject = spawned;
				t.texture = nTexture;
				tiles[i, j] = t;
			}
		}
		return tiles;
	}

	public bool IsOriginalArrangement() {
		//Cycle through the array and compare their textures
		for (int i = 0; i < TileMatrix.GetLength(0); i++) {
			for (int j = 0; j < TileMatrix.GetLength(1); j++) {
				if (TileMatrix[i, j].texture != originalTM[i, j].texture)
					return false;
			}
		}
		return true;
	}
}
