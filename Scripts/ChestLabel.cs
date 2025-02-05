using UnityEngine;

namespace NameChests {
	public class ChestLabel : MonoBehaviour {
		public GameObject container;
		public PugText text;

		private EntityMonoBehaviour _assignedEntityMono;
		private byte[] _currentUtf8TextCache;
		private string _currentTextCache;
		private string _lastName;
		private bool _wasBeingHovered;
		private bool _wasBeingInteracted;

		private void Start() {
			text.SetTempColor(Options.Color);
			text.SetOutlineColor(Options.ColorOutline);
		}

		public void SetEntityMono(EntityMonoBehaviour entityMono) {
			_assignedEntityMono = entityMono;
		}

		private void Awake() {
			container.SetActive(false);
		}

		private void LateUpdate() {
			const int renderOrderNormal = 12001;
			const int renderOrderDim = 12000;

			var isBeingHovered = _assignedEntityMono != null && Options.ShowOnHover && Main.HoveredEntityMono == _assignedEntityMono;
			var isBeingInteracted = _assignedEntityMono != null && Main.InteractingEntityMono == _assignedEntityMono;
			var isVisible = Manager.main.player && !Manager.ui.isAnyInventoryShowing && !Manager.prefs.hideInGameUI && (Options.ShowAlways || isBeingHovered || isBeingInteracted);
			
			if (!isVisible) {
				container.SetActive(false);
				_lastName = null;
				return;
			}
			
			var description = Utils.GetDescription(_assignedEntityMono, ref _currentTextCache, ref _currentUtf8TextCache);

			container.SetActive(true);
			container.transform.position = GetLabelPosition(_assignedEntityMono);
			
			if (description != _lastName || isBeingInteracted != _wasBeingInteracted || isBeingHovered != _wasBeingHovered) {
				text.SetOrderInLayer(isBeingInteracted ? renderOrderNormal : renderOrderDim);
				text.Render(description);
				_lastName = description;
			}

			_wasBeingHovered = isBeingHovered;
			_wasBeingInteracted = isBeingInteracted;
		}

		private static Vector3 GetLabelPosition(EntityMonoBehaviour entityMono) {
			var top = entityMono.RenderPosition + Vector3.up;
			EntityUtility.GetPrefabSizeAndOffset(entityMono.entity, entityMono.objectInfo, out var size, out var offset);
			
			// calculate top of object
			var actualSize = size - offset;
			if (actualSize.x > 1)
				top.x += (actualSize.x - 1) / 2f;
			if (actualSize.y > 1)
				top.z += actualSize.y - 1;
			
			// per-object offsets
			var direction = EntityUtility.GetComponentData<DirectionCD>(entityMono.entity, entityMono.world);
			var verticalOffset = 0f;
			
			var isSingleWideChest = direction.direction.x != 0 && entityMono is Chest { showSortAndQuickStackButtons: true } && size.x == 1 && size.y == 1;
			var isPedestalContainingItem = entityMono is Pedestal pedestal && pedestal.inventoryHandler != null && !pedestal.inventoryHandler.IsEmpty();
			var isMannequin = entityMono is Mannequin;
			var isTerrariumOrAquarium = entityMono is Terrarium or Aquarium;
			
			if (isSingleWideChest)
				verticalOffset = 0.2f;
			if (isPedestalContainingItem || isMannequin)
				verticalOffset = 0.25f;
			if (isTerrariumOrAquarium)
				verticalOffset = 0.7f;

			top.y += verticalOffset;

			return top;
		}
	}
}