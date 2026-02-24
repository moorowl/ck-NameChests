using System;
using System.Collections;
using System.Linq;
using PugMod;
using UnityEngine;

namespace NameChests.Utilities {
	public static class GraphicalObjectUtils {
		private static readonly MemberInfo MiGetPrefabPool = typeof(MemoryManager).GetMembersChecked().FirstOrDefault(x => x.GetNameChecked() == "GetPrefabPool");
		private static readonly MemberInfo MiPrefab = typeof(PoolSystem).GetMembersChecked().FirstOrDefault(x => x.GetNameChecked() == "_prefab");
		private static readonly MemberInfo MiAutoParent = typeof(PoolSystem).GetMembersChecked().FirstOrDefault(x => x.GetNameChecked() == "_autoParent");

		public delegate void ModifierDelegate(EntityMonoBehaviour entityMono, GameObject root);
		
		public static void ModifyGraphicalObject(Type type, ModifierDelegate modifier) {
			Manager.RunAfterInitComplete(ModifyGraphicalObjectCoroutine(type, modifier));
		}

		private static IEnumerator ModifyGraphicalObjectCoroutine(Type type, ModifierDelegate modifier) {
			var pool = (PoolSystem) MiGetPrefabPool.InvokeChecked(Manager.memory, type);

			var prefab = (GameObject) MiPrefab.GetValueChecked(pool);
			var autoParent = (Transform) MiAutoParent.GetValueChecked(pool);
			
			if (prefab != null && autoParent != null) {
				// Modify the main prefab
				if (prefab.TryGetComponent<EntityMonoBehaviour>(out var prefabEntityMono))
					modifier(prefabEntityMono, prefab);
			
				// Modify the objects that have already been allocated
				foreach (var allocatedEntityMono in autoParent.GetComponentsInChildren<EntityMonoBehaviour>(true))
					modifier(allocatedEntityMono, allocatedEntityMono.gameObject);	
			}

			yield return null;
		}
	}
}