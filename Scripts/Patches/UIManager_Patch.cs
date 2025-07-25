using System;
using System.Collections.Generic;
using HarmonyLib;
using PugMod;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming

namespace NameChests.Patches {
	[HarmonyPatch]
	public static class UIManager_Patch {
		private static readonly Vector2 NameOffsetLeft = new(-10f / 16f, 1.1f);
		private static readonly Vector2 NameOffsetCenter = new(0f, 1.1f);

		private static readonly Dictionary<int, ChestNameInput> ChestNameInputLookup = new();
		
		[HarmonyPatch(typeof(UIManager), nameof(UIManager.OnChestInventoryOpen))]
		[HarmonyPostfix]
		public static void OnChestInventoryOpen(UIManager __instance) {
			UpdateChestNameInput(__instance.chestInventoryUI);
		}
		
		[HarmonyPatch(typeof(UIManager), "Update")]
		[HarmonyPostfix]
		public static void Update(UIManager __instance) {
			if (__instance.chestInventoryUI == null || !__instance.chestInventoryUI.isShowing || !__instance.chestInventoryUI.autoPositionSlots)
				return;
			
			UpdateChestNameInput(__instance.chestInventoryUI);
		}

		private static void UpdateChestNameInput(ItemSlotsUIContainer inventoryUI) {
			if (inventoryUI.containerType != ItemSlotsUIContainerType.ChestInventory)
				return;
			
			var chestNameInput = GetChestNameInput(inventoryUI);
			if (chestNameInput == null)
				return;
			
			if (Utils.SupportsNaming(inventoryUI.GetInventoryHandler().entityMonoBehaviour) && inventoryUI.itemSlots.Count >= 1) {
				var alignment = inventoryUI.visibleColumns <= 3 ? Alignment.Center : Alignment.Left;
				var slot = alignment switch {
					Alignment.Left => inventoryUI.itemSlots[0],
					Alignment.Center => inventoryUI.itemSlots[(int) math.floor(inventoryUI.visibleColumns / 2f)],
					_ => throw new ArgumentOutOfRangeException()
				};
				var textAlignment = alignment switch {
					Alignment.Left => PugTextStyle.HorizontalAlignment.left,
					Alignment.Center => PugTextStyle.HorizontalAlignment.center,
					_ => throw new ArgumentOutOfRangeException()
				};
				var offset = alignment switch {
					Alignment.Left => NameOffsetLeft,
					Alignment.Center => NameOffsetCenter,
					_ => throw new ArgumentOutOfRangeException()
				};

				chestNameInput.SetAlignment(textAlignment);
				
				chestNameInput.gameObject.SetActive(true);
				chestNameInput.gameObject.transform.localPosition = new Vector3(slot.transform.localPosition.x + offset.x, slot.transform.localPosition.y + offset.y, 0f);
			} else {
				chestNameInput.gameObject.SetActive(false);
			}
		}

		private static ChestNameInput GetChestNameInput(ItemSlotsUIContainer inventoryUI) {
			var instanceId = inventoryUI.GetInstanceID();
			if (!ChestNameInputLookup.ContainsKey(instanceId)) {
				var isExpandedChestUi = inventoryUI.GetType().GetNameChecked() == "ExpandedInventoryUI";
				var parent = isExpandedChestUi ? inventoryUI.itemSlotsRoot.transform.parent.parent : inventoryUI.itemSlotsRoot.transform;
					
				var chestNameInput = Object.Instantiate(Main.ChestNameInputPrefab, parent).GetComponent<ChestNameInput>();
				chestNameInput.gameObject.SetActive(false);
				
				ChestNameInputLookup[instanceId] = chestNameInput;
			}

			return ChestNameInputLookup[instanceId];
		}

		private enum Alignment {
			Left,
			Center
		}
	}
}