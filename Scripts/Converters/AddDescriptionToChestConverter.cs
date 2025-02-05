using PugConversion;
using UnityEngine;

namespace NameChests.Converters {
	public class AddDescriptionToChestConverter : PugConverter {
		public override void Convert(GameObject authoring) {
			if (!authoring.TryGetComponent<IEntityMonoBehaviourData>(out var entityMonoBehaviourData))
				return;
			
			var graphicalPrefab = entityMonoBehaviourData.ObjectInfo.prefabInfos[0].prefab;
			if (graphicalPrefab == null)
				return;

			if (!graphicalPrefab.TryGetComponent<EntityMonoBehaviour>(out var entityMono))
				return;

			if (Utils.SupportsNaming(entityMono))
				EnsureHasBuffer<DescriptionBuffer>();
		}
	}
}