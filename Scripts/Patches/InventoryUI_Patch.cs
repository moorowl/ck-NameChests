using HarmonyLib;
using Unity.Mathematics;
using UnityEngine;

namespace NameChests.Patches {
	[HarmonyPatch]
	public static class InventoryUI_Patch {
		private static readonly Vector2 NameOffsetLeft = new(-10f / 16f, 1.1f);
		private static readonly Vector2 NameOffsetCenter = new(0f, 1.1f);
		
		[HarmonyPatch(typeof(InventoryUI), "Awake")]
		[HarmonyPostfix]
		public static void Awake(InventoryUI __instance) {
			// Add ChestNameInput to chest inventories
			if (__instance.containerType != ItemSlotsUIContainerType.ChestInventory)
				return;
			
			var chestNameInput = Object.Instantiate(Main.ChestNameInputPrefab, __instance.itemSlotsRoot.transform).GetComponent<ChestNameInput>();
			chestNameInput.gameObject.SetActive(false);
		}
		
		[HarmonyPatch(typeof(InventoryUI), "ShowContainerUI")]
		[HarmonyPostfix]
		public static void ShowContainerUI(InventoryUI __instance) {
			// Update positioning of ChestNameInput (or disable it) when a container is opened
			var chestNameInput = __instance.gameObject.GetComponentInChildren<ChestNameInput>(true);
			if (chestNameInput == null)
				return;
			
			if (Utils.SupportsNaming(__instance.GetInventoryHandler().entityMonoBehaviour) && __instance.itemSlots.Count >= 1) {
				var centerNameInput = __instance.visibleRows == 1;
				var slot = __instance.itemSlots[centerNameInput ? (int) math.floor(__instance.visibleColumns / 2f) : 0];
				var offset = centerNameInput ? NameOffsetCenter : NameOffsetLeft;
				
				chestNameInput.SetAlignment(centerNameInput ? PugTextStyle.HorizontalAlignment.center : PugTextStyle.HorizontalAlignment.left);
				
				chestNameInput.gameObject.SetActive(true);
				chestNameInput.gameObject.transform.localPosition = new Vector3(slot.transform.localPosition.x + offset.x, slot.transform.localPosition.y + offset.y, 0f);
			} else {
				chestNameInput.gameObject.SetActive(false);
			}
		}
	}
}