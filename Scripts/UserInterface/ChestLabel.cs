using NameChests.Utilities;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace NameChests {
	public class ChestLabel : MonoBehaviour {
		public GameObject container;
		public PugText text;

		private EntityMonoBehaviour _entityMono;
		private byte[] _currentUtf8TextCache;
		private string _currentTextCache;
		private string _lastLabel;
		private bool _wasBeingHovered;
		private bool _wasBeingInteracted;
		private bool _hasRenderedText;
		
		private void Awake() {
			_entityMono = GetComponentInParent<EntityMonoBehaviour>();
			container.SetActive(false);
		}

		private void LateUpdate() {
			UpdateVisuals();
		}

		private void UpdateVisuals() {
			const int renderOrderNormal = 12001;
			const int renderOrderDim = 12000;

			var isBeingHovered = Options.Instance.ShowOnHover && Main.HoveredEntityMono == _entityMono;
			var isBeingInteracted = Main.InteractingEntityMono == _entityMono;
			var isVisible = Manager.main.player != null
			                && _entityMono != null
			                && _entityMono.entityExist
			                && !Manager.ui.isAnyInventoryShowing
			                && !Manager.prefs.hideInGameUI
			                && (isBeingHovered || isBeingInteracted);
			
			if (!isVisible) {
				container.SetActive(false);
				_lastLabel = null;
				if (text.displayedTextString.Length > 0)
					text.Clear();
				
				return;
			}
			
			var label = LabelUtils.GetLabel(_entityMono, ref _currentTextCache, ref _currentUtf8TextCache);

			container.SetActive(true);
			container.transform.position = CalculatePosition();
			
			if (!_hasRenderedText || label != _lastLabel || isBeingInteracted != _wasBeingInteracted || isBeingHovered != _wasBeingHovered) {
				text.SetOrderInLayer(isBeingInteracted ? renderOrderNormal : renderOrderDim);
				text.Render(label);
				text.SetOutlines(true, true, true, true);

				_lastLabel = label;
				_wasBeingHovered = isBeingHovered;
				_wasBeingInteracted = isBeingInteracted;
				_hasRenderedText = true;
			}
			
			text.SetTempColor(Options.Instance.Color.Rgba);
			text.SetOutlineColor(Options.Instance.OutlineColor.Rgba);
		}

		private Vector3 CalculatePosition() {
			const float singleWideChestOffset = 0.2f;
			const float mannequinOffset = 0.25f;
			const float terrariumOffset = 0.7f;
			
			var top = _entityMono.RenderPosition + Vector3.up;
			EntityUtility.GetPrefabSizeAndOffset(_entityMono.entity, _entityMono.objectInfo, out var size, out var offset);
			
			// calculate top of object
			var actualSize = size - offset;
			if (actualSize.x > 1)
				top.x += (actualSize.x - 1) / 2f;
			if (actualSize.y > 1)
				top.z += actualSize.y - 1;
			
			// per-object offsets
			var direction = EntityUtility.GetComponentData<DirectionCD>(_entityMono.entity, _entityMono.world);
			var verticalOffset = 0f;
			
			var isSingleWideChest = direction.direction.x != 0 && _entityMono is Chest { showSortAndQuickStackButtons: true } && size.x == 1 && size.y == 1;
			var isPedestalContainingItem = _entityMono is Pedestal pedestal && pedestal.inventoryHandler != null && !pedestal.inventoryHandler.IsEmpty();
			var isMannequin = _entityMono is Mannequin;
			var isTerrariumOrAquarium = _entityMono is Terrarium or Aquarium;
			
			if (isSingleWideChest)
				verticalOffset = singleWideChestOffset;
			if (isPedestalContainingItem || isMannequin)
				verticalOffset = mannequinOffset;
			if (isTerrariumOrAquarium)
				verticalOffset = terrariumOffset;

			top.y += verticalOffset;

			return top;
		}
	}
}