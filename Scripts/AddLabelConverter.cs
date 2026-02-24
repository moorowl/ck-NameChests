using System;
using System.Collections.Generic;
using NameChests.Utilities;
using Pug.Conversion;
using UnityEngine;

namespace NameChests {
	public class AddLabelConverter : Converter {
		private static readonly HashSet<Type> AlreadyModifiedGraphicalObjects = new();
		
		public override void Convert(GameObject authoring) {
			if (Manager.main.currentSceneHandler == null || !authoring.TryGetComponent<IEntityMonoBehaviourData>(out var entityMonoBehaviourData))
				return;
			
			var graphicalObject = entityMonoBehaviourData.ObjectInfo.prefabInfos[0].prefab;
			if (graphicalObject == null)
				return;

			if (!graphicalObject.TryGetComponent<EntityMonoBehaviour>(out var entityMono) || !LabelUtils.HasLabel(entityMono))
				return;

			var entityMonoType = entityMono.GetType();
			if (!AlreadyModifiedGraphicalObjects.Contains(entityMonoType)) {
				GraphicalObjectUtils.ModifyGraphicalObject(entityMonoType, (_, root) => {
					root.AddComponent<ChestLabelInitializer>();
				});
				AlreadyModifiedGraphicalObjects.Add(entityMonoType);	
			}
		}
	}
}