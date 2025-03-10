using UnityEngine;

namespace NameChests {
	public class ChestNameInput : UIelement {
		private const float SetNameCooldown = 1f;
		
		public GameObject root;
		public TextInputField textInputField;
		public BoxCollider boxCollider;

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
		
		protected override void LateUpdate()  {
			base.LateUpdate();
			UpdateNameText();
			
			// Deactivate text input if we swap to a controller
			if (!Manager.input.singleplayerInputModule.PrefersKeyboardAndMouse() && textInputField.inputIsActive) {
				textInputField.Deactivate(false);
				Manager.ui.DeselectAnySelectedUIElement();
				Manager.ui.mouse.UpdateMouseUIInput(out _, out _);
			}
		}

		public void SetAlignment(PugTextStyle.HorizontalAlignment alignment) {
			var center = alignment == PugTextStyle.HorizontalAlignment.center ? 0 : 0.5f;
			boxCollider.center = new Vector3(boxCollider.size.x * center, boxCollider.center.y, boxCollider.center.z);
			
			textInputField.pugText.style.color = Options.Color;
			textInputField.pugText.style.outlineColor = Options.ColorOutline;
			textInputField.pugText.style.horizontalAlignment = alignment;
			textInputField.pugText.Render();
			textInputField.hintText.style.color = Options.ColorPlaceholder;
			textInputField.hintText.style.outlineColor = Options.ColorOutline;
			textInputField.hintText.style.horizontalAlignment = alignment;
			textInputField.hintText.Render();
			
			textInputField.characterMarkBlinker.sr.color = Options.Color.ColorWithNewAlpha(textInputField.characterMarkBlinker.sr.color.a);
		}
		
		public void SetName() {
			var player = Manager.main.player;
			if (player == null || ActiveInventoryHandler == null)
				return;

			var nameToSet = textInputField.pugText.displayedTextString;
			Manager.ui.HideAllInventoryAndCraftingUI();
			player.playerCommandSystem.SetDescription(ActiveInventoryHandler.inventoryEntity, nameToSet);
			
			_inputFieldWasSetTimer = SetNameCooldown;
		}
		
		public void UpdateNameText() {
			var activeInventoryHandler = ActiveInventoryHandler;
			if (activeInventoryHandler == null)
				return;

			var entityMono = activeInventoryHandler.entityMonoBehaviour;
			if (activeInventoryHandler != _lastInventoryHandler)
				_inputFieldWasSetTimer = 0f;

			if (_inputFieldWasSetTimer > 0f && activeInventoryHandler == _lastInventoryHandler) {
				_inputFieldWasSetTimer -= Time.deltaTime;
			} else {
				if (textInputField.inputIsActive)
					return;

				var text = Utils.GetDescription(entityMono, ref _currentTextCache, ref _currentUtf8TextCache);
				if (string.IsNullOrEmpty(text)) {
					if (!string.IsNullOrEmpty(textInputField.pugText.GetText())) {
						textInputField.SetInputText("");
					}
				} else if (!textInputField.pugText.GetText().Equals(text)) {
					textInputField.SetInputText(text);
				}
			}
			
			_lastInventoryHandler = activeInventoryHandler;
		}
	}
}