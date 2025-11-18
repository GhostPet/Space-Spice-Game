using UnityEngine;

public class GridManager : MonoBehaviour
{
	[SerializeField] private int width, height;
	[SerializeField] private Transform tileParent;
	[SerializeField] private Floor TileFloor;

	private void Awake() {
		GenerateGrid();
	}

	private void GenerateGrid() {
		for (int i =0; i < width; i++) {
			for (int j =0; j < height; j++) {
				// Create parent GameObject for the tile (do not use Instantiate for a new empty GameObject)
				var spawnedParent = new GameObject($"Tile {i} {j}");
				spawnedParent.transform.SetParent(tileParent);
				// Place parent at tile position so child can be local at zero
				spawnedParent.transform.position = new Vector3(i,0, j);
				spawnedParent.AddComponent<Tile>();

				// Instantiate the floor prefab once as a child of the parent
				if (TileFloor != null) {
					var spawnedTile = Instantiate(TileFloor, spawnedParent.transform.position, Quaternion.identity, spawnedParent.transform);
					spawnedTile.name = "TileFloor";
				}
			}
		}
	}
}
