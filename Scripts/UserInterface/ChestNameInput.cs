using System;
using System.Collections.Generic;
using HarmonyLib;
using NameChests.Utilities;
using Pug.UnityExtensions;
using PugMod;
using Unity.Mathematics;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace NameChests {
	public class ChestNameInput : UIelement {
		private const float SetNameCooldown = 1f;

		public GameObject root;
		public TextInputField textInputField;
		public BoxCollider boxCollider;
		public Transform underline;
		public SpriteRenderer underlineSrTop;
		public SpriteRenderer underlineSrBottom;

		private byte[] _currentUtf8TextCache;
		private string _currentTextCache;
		private float _inputFieldWasSetTimer;
		private InventoryHandler _lastInventoryHandler;

		private static InventoryHandler ActiveInventoryHandler {
			get {
				var player = Manager.main.player;
				if (player == null || player.activeInventoryHandler == null)
					return null;

				return player.activeInventoryHandler;
			}
		}

		protected override void LateUpdate() {
			base.LateUpdate();
			UpdateNameText();

			// Deactivate text input if we swap to a controller
			if (!Manager.input.singleplayerInputModule.PrefersKeyboardAndMouse() && textInputField.inputIsActive) {
				textInputField.Deactivate(false);
				Manager.ui.DeselectAnySelectedUIElement();
				Manager.ui.mouse.UpdateMouseUIInput(out _, out _);
			}

			UpdateUnderline();
		}

		public void SetAlignment(PugTextStyle.HorizontalAlignment alignment) {
			var center = alignment == PugTextStyle.HorizontalAlignment.center ? 0 : 0.5f;
			boxCollider.center = new Vector3(boxCollider.size.x * center, boxCollider.center.y, boxCollider.center.z);
			
			textInputField.pugText.style.horizontalAlignment = alignment;
			textInputField.pugText.Render();
			textInputField.hintText.style.horizontalAlignment = alignment;
			textInputField.hintText.Render();
		}

		public void SetName() {
			var player = Manager.main.player;
			if (player == null || ActiveInventoryHandler == null)
				return;

			player.playerCommandSystem.SetDescription(ActiveInventoryHandler.inventoryEntity, textInputField.pugText.displayedTextString);

			_inputFieldWasSetTimer = SetNameCooldown;
		}

		public void UpdateNameText() {
			var activeInventoryHandler = ActiveInventoryHandler;
			if (activeInventoryHandler == null)
				return;
			
			textInputField.pugText.SetTempColor(Options.Instance.Color.Rgba);
			textInputField.pugText.SetOutlineColor(Options.Instance.OutlineColor.Rgba);
			textInputField.hintText.SetTempColor((Options.Instance.Color.Rgba * 0.7f).ColorWithNewAlpha(1f));
			textInputField.hintText.SetOutlineColor(Options.Instance.OutlineColor.Rgba);
			textInputField.characterMarkBlinker.sr.color = Options.Instance.Color.Rgba.ColorWithNewAlpha(textInputField.characterMarkBlinker.sr.color.a);

			var entityMono = activeInventoryHandler.entityMonoBehaviour;
			if (activeInventoryHandler != _lastInventoryHandler)
				_inputFieldWasSetTimer = 0f;

			if (_inputFieldWasSetTimer > 0f && activeInventoryHandler == _lastInventoryHandler) {
				_inputFieldWasSetTimer -= Time.deltaTime;
			} else {
				if (textInputField.inputIsActive)
					return;

				var label = LabelUtils.GetLabel(entityMono, ref _currentTextCache, ref _currentUtf8TextCache);
				if (string.IsNullOrEmpty(label)) {
					if (!string.IsNullOrEmpty(textInputField.pugText.GetText())) {
						textInputField.SetInputText("");
					}
				} else if (!textInputField.pugText.GetText().Equals(label)) {
					textInputField.SetInputText(label);
				}
			}

			_lastInventoryHandler = activeInventoryHandler;
		}

		private void UpdateUnderline() {
			//underline.gameObject.SetActive(Manager.ui.currentSelectedUIElement == textInputField || textInputField.inputIsActive);
			underline.gameObject.SetActive(false);
			if (!underline.gameObject.activeSelf)
				return;

			var visibleText = textInputField.hintText.displayedTextString != "" ? textInputField.hintText : textInputField.pugText;	
			underline.localPosition = RoundToPixelPerfectPosition.RoundPosition(new Vector3(
				visibleText.dimensions.position.x + visibleText.dimensions.width / 2f,
				visibleText.dimensions.position.y + (2f / 16f),
				0f
			));
			underline.localScale = RoundToPixelPerfectPosition.RoundPosition(new Vector3(visibleText.dimensions.width * 16f, 1f, 1f));
			underlineSrTop.color = textInputField.pugText.color;
			underlineSrBottom.color = textInputField.pugText.style.outlineColor;
		}
		
		[HarmonyPatch]
		public static class Patches {
			private static readonly Vector2 NameOffsetLeft = new(-10f / 16f, 1.1f);
			private static readonly Vector2 NameOffsetCenter = new(0f, 1.1f);

			private static readonly Dictionary<int, ChestNameInput> ChestNameInputLookup = new();

			[HarmonyPatch(typeof(UIManager), nameof(UIManager.OnChestInventoryOpen))]
			[HarmonyPostfix]
			public static void UIManager_OnChestInventoryOpen(UIManager __instance) {
				UpdateChestNameInput(__instance.chestInventoryUI);
			}

			[HarmonyPatch(typeof(UIManager), "Update")]
			[HarmonyPostfix]
			public static void UIManager_Update(UIManager __instance) {
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

				if (LabelUtils.HasLabel(inventoryUI.GetInventoryHandler().entityMonoBehaviour) && inventoryUI.itemSlots.Count >= 1) {
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

					var chestNameInput = Instantiate(Main.ChestNameInputPrefab, parent).GetComponent<ChestNameInput>();
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
}