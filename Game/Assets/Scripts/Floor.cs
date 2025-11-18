using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{
	[SerializeField] private bool isHighlightable = true;

	[Header("Highlight Settings")]
	[SerializeField] private Color defaultHighlightColor = Color.yellow;
	[SerializeField] private float emissionIntensity = 2f; // used if material supports emission

	private Renderer[] renderers;
	private Material[][] originalSharedMaterials; // original shared materials per renderer (for restoring)
	private List<Material> runtimeCreatedMaterials = new List<Material>();
	private bool isHighlighted = false;

	private void Awake() {
		renderers = GetComponentsInChildren<Renderer>();
		originalSharedMaterials = new Material[renderers.Length][];
		for (int i = 0; i < renderers.Length; i++) {
			originalSharedMaterials[i] = renderers[i].sharedMaterials;
		}
	}

	private void OnDestroy() {
		// Clean up any runtime-created materials to avoid leaks
		for (int i = 0; i < runtimeCreatedMaterials.Count; i++) {
			if (runtimeCreatedMaterials[i] != null) {
				Destroy(runtimeCreatedMaterials[i]);
			}
		}
		runtimeCreatedMaterials.Clear();
	}

	public bool IsHighlightable() {
		return isHighlightable;
	}

	public void Highlight(Color highlightColor) {
		if (!isHighlightable) return;
		if (renderers == null || renderers.Length == 0) return;

		// Avoid re-highlighting
		if (isHighlighted) return;

		isHighlighted = true;

		for (int i = 0; i < renderers.Length; i++) {
			var r = renderers[i];
			var shared = originalSharedMaterials[i];
			if (shared == null || shared.Length == 0) continue;

			Material[] mats = new Material[shared.Length];
			for (int m = 0; m < shared.Length; m++) {
				var baseMat = shared[m];
				if (baseMat == null) {
					mats[m] = null;
					continue;
				}

				// Create an instance of the material so we don't modify the shared asset
				Material inst = new Material(baseMat);
				runtimeCreatedMaterials.Add(inst);

				// Prefer setting _EmissionColor if available, otherwise try _Color
				if (inst.HasProperty("_EmissionColor")) {
					Color em = highlightColor * emissionIntensity;
					inst.SetColor("_EmissionColor", em);
					inst.EnableKeyword("_EMISSION");
				} else if (inst.HasProperty("_Color")) {
					inst.SetColor("_Color", highlightColor);
				}

				mats[m] = inst;
			}

			// Assign new material instances to renderer
			r.materials = mats;
		}
	}

	public void Highlight() {
		Highlight(defaultHighlightColor);
	}

	public void RemoveHighlight() {
		if (!isHighlighted) return;
		isHighlighted = false;

		if (renderers == null || renderers.Length == 0) return;

		for (int i = 0; i < renderers.Length; i++) {
			var r = renderers[i];
			var shared = originalSharedMaterials[i];
			if (shared == null) continue;

			// Before restoring, destroy runtime-created materials that were assigned
			if (r.materials != null) {
				foreach (var mat in r.materials) {
					if (mat == null) continue;
					// If this material is in our runtime list, destroy it
					if (runtimeCreatedMaterials.Contains(mat)) {
						runtimeCreatedMaterials.Remove(mat);
						Destroy(mat);
					}
				}
			}

			// Restore original shared materials
			r.sharedMaterials = shared;
		}

		// Destroy any remaining created materials
		for (int i = 0; i < runtimeCreatedMaterials.Count; i++) {
			if (runtimeCreatedMaterials[i] != null) Destroy(runtimeCreatedMaterials[i]);
		}
		runtimeCreatedMaterials.Clear();
	}

	public void Interact() {
		Debug.Log($"Zemin ile etkileþimde bulunuldu.");
	}
}
