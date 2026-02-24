using UnityEngine;

namespace NameChests {
	public class ChestLabelInitializer : MonoBehaviour {
		private void Start() {
			Instantiate(Main.ChestLabelPrefab, transform);
			Destroy(this);
		}
	}
}