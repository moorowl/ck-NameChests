using NameChests.Utilities;
using Pug.Conversion;
using UnityEngine;

namespace NameChests {
	public class AddDescriptionConverter : Converter {
		public override void Convert(GameObject authoring) {
			if (!authoring.TryGetComponent<IEntityMonoBehaviourData>(out var entityMonoBehaviourData))
				return;
			
			var graphicalObject = entityMonoBehaviourData.ObjectInfo.prefabInfos[0].prefab;
			if (graphicalObject == null)
				return;

			if (!graphicalObject.TryGetComponent<EntityMonoBehaviour>(out var entityMono) || !LabelUtils.HasLabel(entityMono))
				return;
			
			EnsureHasBuffer<DescriptionBuffer>();
		}
	}
}