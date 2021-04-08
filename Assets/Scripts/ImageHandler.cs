using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

/// <summary>
/// Manages an image, creating a 3x5 matrix containing each a part of an image
/// </summary>
public class ImageHandler : MonoBehaviour {

	public struct Tile {

		public GameObject gameObject;
		public Texture2D texture;
		public bool IsLast { get { return !gameObject.activeSelf; } }

		public static Tile Empty { get { 
				Tile t = new Tile();
				t.gameObject = null;
				t.texture = null;
				return t;
			} }

		public static bool operator ==(Tile a, Tile b) {
			return a.gameObject == b.gameObject && a.texture == b.texture;
		}

		public static bool operator !=(Tile a, Tile b) {
			return !(a == b);
		}

		public override bool Equals(object other) {
			return this == (Tile)other;
		}

		public void SetTexture(Texture2D texture) {
			var renderer = gameObject.transform.GetChild(0).GetComponent<Renderer>();
			renderer.material.SetTexture("_MainTex", texture);
			this.texture = texture;
		}

	}

	#region Variables
	[SerializeField]
	private Texture2D[] images;
	private Texture2D originalImage;
	[SerializeField]
	private GameObject baseTile;
	[SerializeField]
	private Vector3 startingPos;
	[SerializeField]
	private float scaleDownFactor;
	[SerializeField]
	private float transformDownFactor;
	[SerializeField]
	private TextMeshProUGUI hintText;
	[SerializeField]
	private Steganography steganography;
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
		steganography = GetComponent<Steganography>();
		ChooseRandomImage();
		SpawnParts();
	}

	private void ChooseRandomImage() {
		int index = Random.Range(0, 14);
		originalImage = images[index];
		string hint = steganography.hints[index];
		//Debug.Log(hint);
		hintText.SetText(hint);
	}

	public void SpawnParts() {
		TileMatrix = GetTiles();
		SetGlobalTiles();

		Texture2D lastTileT = TileMatrix[4, 0].texture;
		Texture2D nonShuffled1 = TileMatrix[4, 1].texture;
		Texture2D nonShuffled2 = TileMatrix[3, 0].texture;
		
		Tile[,] shuffled = (Tile[,])TileMatrix.Clone();
		Utils.Randomize(shuffled);
		for (int i = 0; i < horizontal; i++) {
			for (int j = 0; j < vertical; j++) {
				TileMatrix[i, j].SetTexture(shuffled[i, j].texture);
			}
		}
		
		(int i, int j) lastTilePos = FindInMatrix(TileMatrix, lastTileT);
		
		SwapElements(ref TileMatrix[4, 0], ref TileMatrix[lastTilePos.i, lastTilePos.j]);
		(int i, int j) pos1 = FindInMatrix(TileMatrix, nonShuffled1);
		SwapElements(ref TileMatrix[4, 1], ref TileMatrix[pos1.i, pos1.j]);
		(int i, int j) pos2 = FindInMatrix(TileMatrix, nonShuffled2);
		SwapElements(ref TileMatrix[3, 0], ref TileMatrix[pos2.i, pos2.j]);
		//Debug.Log(pos1);

		TileMatrix[4, 0].gameObject.SetActive(false);
		lastTile = TileMatrix[4, 0];
		Assert.IsTrue(lastTile.IsLast);
	}


	private void SwapElements(ref Tile a, ref Tile b) {
		Texture2D temp = a.texture;
		a.SetTexture(b.texture);
		b.SetTexture(temp);
	}

	private void SetGlobalTiles() {
		int indexI = TileMatrix.GetLength(0) - 1;
		lastTile = TileMatrix[indexI, 0];
		//lastTile.gameObject.SetActive(false);
		originalTM = (Tile[,])TileMatrix.Clone();
	}

	private Tile[,] GetTiles() {
		float nWidth = originalImage.width / (float)horizontal;
		float nHeight = originalImage.height / (float)vertical;

		Tile[,] tiles = new Tile[horizontal, vertical];
		
		int roundedHeight = (int)nHeight;
		int roundedWidth = (int)nHeight;

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

	private (int i, int j) FindInMatrix(Tile[,] matrix, Texture2D element) {
		for (int i = 0; i < TileMatrix.GetLength(0); i++) {
			for (int j = 0; j < TileMatrix.GetLength(1); j++) {
				if (TileMatrix[i, j].texture == element)
					return (i, j);
			}
		}
		return (-1, -1);
	}

}
