using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class TileShuffler : MonoBehaviour {

	public enum Directions {
		UP = 2,
		DOWN = -2,
		LEFT = -1,
		RIGHT = 1,
		NONE = 3
	}

	#region Variables
	private const int TILE_MASK = 1 << 8;
	private Camera cam;
	[SerializeField]
	private ImageHandler handlerRef;
	private Transform selectedTileT;
	private bool selected;
	private Directions direction;
	public bool Won { get; private set; }
	#endregion

	//Do mouse picking (Lifting the tile slighlty, and moving it to 
	//a normalized vector multiplied by a fixed scalar factor

	private void Start() {
		cam = Camera.main;
		Won = false;
		direction = Directions.NONE;
	}

	private void ManageInput() {
		if (Input.GetMouseButtonDown(0) && selectedTileT == null) {
			SelectTile();
		}
		else if (Input.GetMouseButtonUp(0) && selectedTileT != null) {
			//Debug.Log("Tile " + selectedTileT.name + " has been deselected!");
			selected = false;
			MoveTile();
			selectedTileT = null;
		}
	}

	private void Update() {
		ManageInput();
		GrabTile();
	}

	private void MoveTile() {
		//Debug.Log(direction);
		//Get the tile's indeces
		StringBuilder sb = new StringBuilder(selectedTileT.name.Replace("Tile ", ""));
		string[] strIndeces = sb.ToString().Split(',');
		(int i, int j) indeces = (System.Convert.ToInt32(strIndeces[0]), System.Convert.ToInt32(strIndeces[1]));
		(int i, int j) prevIndeces = indeces;
		//Debug.Log(indeces);
		//Switch
		switch (direction) {
			case Directions.UP:
			case Directions.DOWN:
				int value = Mathf.Abs((int)direction);
				indeces.j += (int)Mathf.Sign((int) direction) * (value - 1);
				break;
			case Directions.LEFT:
			case Directions.RIGHT:
				indeces.i += (int)direction;
				break;
			case Directions.NONE:
				return;
		}
		//Check for bounds
		if (!IsValidMove(indeces)) {
			//Debug.Log("Invalid move!");
		}
		else {
			//Set active the empty tile object, and
			//Swap their textures
			ref ImageHandler.Tile prevIndexed = ref handlerRef.TileMatrix[prevIndeces.i, prevIndeces.j];
			ref ImageHandler.Tile futureIndexed = ref handlerRef.TileMatrix[indeces.i, indeces.j];
			
			Texture2D tempTex = futureIndexed.texture;
			futureIndexed.SetTexture(prevIndexed.texture);
			prevIndexed.SetTexture(tempTex);
			futureIndexed.gameObject.SetActive(true);
			prevIndexed.gameObject.SetActive(false);
			
			if(handlerRef.IsOriginalArrangement())
				Won = true;
		}
	}

	private bool IsValidMove((int i, int j) indeces) {
		int xLength = handlerRef.TileMatrix.GetLength(0);
		int yLength = handlerRef.TileMatrix.GetLength(1);
		return !(indeces.i < 0 || indeces.i >= xLength || indeces.j >= yLength || indeces.j < 0)
			&& handlerRef.TileMatrix[indeces.i, indeces.j].IsLast;

	}

	private unsafe void GrabTile() {
		if (selected) {
			//Move around and calculate dir
			//Get direction with mouse position
			Vector3 mousePos = Input.mousePosition;
			mousePos.z = 10f;
			Vector2 dir = (cam.ScreenToWorldPoint(mousePos) - selectedTileT.position);
			dir.Normalize();
			//Debug.DrawRay(selectedTileT.position, dir);
			//Now, get the int representation of the direction you want the tile to move
			float* p = Utils.GetMaxAbsPointer(&dir.x, &dir.y);
			//Give me 2 * sign if X, if Y 1 * sign
			direction = p == &dir.x ? (Directions) (1 * Mathf.Sign(*p)) : (Directions) (2 * Mathf.Sign(*p));
		}
	}

	private void SelectTile() {
		//Clone the tile, and hide the other one to simulate movement
		//Setup layermask to make sure that you cannot grab between collisions
		Ray ray = cam.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 2000f, TILE_MASK)) {
			selectedTileT = hit.transform;
			selected = true;
			//Debug.Log("Selected: " + selectedTileT.name);
		}
	}

}
