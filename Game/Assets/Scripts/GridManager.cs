using UnityEngine;

public class GridManager : MonoBehaviour
{
	[SerializeField] private int width, height;
	[SerializeField] private Tile grassTilePrefab;
	[SerializeField] private Tile kitchenTilePrefab;
	[SerializeField] private Tile restaurrantTilePrefab;
	[SerializeField] private Transform tileParent;

	private void Start() {
		GenerateGrid();
	}

	private void GenerateGrid() {
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				var spawnedTileType = grassTilePrefab;

				// Dýþ Kýsým Çim
				if (i == 0 || j == 0 || i == width-1 || j == height-1) {
					spawnedTileType = grassTilePrefab;
				}
				// Üstten 3 Satýr Mutfak
				else if (j > height-5) {
					spawnedTileType = kitchenTilePrefab;
				}
				// Geri Kalan Kýsým Restoran
				else {
					spawnedTileType = restaurrantTilePrefab;
				}

				// Oluþtur, Pozisyonu ayarla ve Ýsimlendir
				var spawnedTile = Instantiate(spawnedTileType, new Vector3(i, 0, j), Quaternion.identity, tileParent);
				spawnedTile.name = $"Tile {i} {j}";
			}
		}
	}
}
