using PugMod;
using Interaction;
using Pug.UnityExtensions;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

namespace NameChests {
	public class Main : IMod {
		public const string Version = "1.1.3";
		public const string Name = "NameChests";
		public const string FriendlyName = "Chest Labels";
		
		internal static GameObject ChestNameInputPrefab;
		internal static GameObject ChestLabelPrefab;
		
		public static EntityMonoBehaviour InteractingEntityMono { get; private set; }
		public static EntityMonoBehaviour HoveredEntityMono { get; private set; }

		public void EarlyInit() {
			Debug.Log($"[{FriendlyName}]: Mod version: {Version}");
			
			API.Client.OnObjectSpawnedOnClient += TryAssignChestLabel;
		}

		public void Init() {
			Options.Init();
		}

		public void ModObjectLoaded(Object obj) {
			if (obj is not GameObject gameObject)
				return;
			
			switch (gameObject.name) {
				case "ChestLabel":
					ChestLabelPrefab = gameObject;
					break;
				case "ChestNameInput":
					ChestNameInputPrefab = gameObject;
					break;
			}
		}

		public void Shutdown() { }

		public void Update() {
			UpdateInteractingAndHoveredEntities();
		}

		private static void TryAssignChestLabel(Entity entity, EntityManager entityManager, GameObject graphicalObject) {
			var entityMono = graphicalObject.GetComponent<EntityMonoBehaviour>();
			if (entityMono == null)
				return;

			var chestLabel = graphicalObject.GetComponentInChildren<ChestLabel>(true);
			if (Utils.SupportsNaming(entityMono) && chestLabel == null)
				chestLabel = Object.Instantiate(ChestLabelPrefab, graphicalObject.transform).GetComponent<ChestLabel>();

			if (chestLabel != null)
				chestLabel.SetEntityMono(entityMono);
		}

		private static void UpdateInteractingAndHoveredEntities() {
			const float hoveredDetectionRadius = 0.01f;
			
			InteractingEntityMono = null;
			HoveredEntityMono = null;
			
			if (Manager.main.player == null || Manager.ui.isAnyInventoryShowing || Manager.prefs.hideInGameUI)
				return;
			
			// Find interacting entity
			var player = Manager.main.player;
			if (player != null
			    && EntityUtility.TryGetComponentData<InteractorCD>(player.entity, player.world, out var interactor)
			    && interactor.currentClosestInteractable != Entity.Null
			    && Manager.memory.TryGetEntityMono(interactor.currentClosestInteractable, out var entityMono)
			    && EntityUtility.HasComponentData<DescriptionBuffer>(entityMono.entity, entityMono.world)) {
				InteractingEntityMono = entityMono;
			}
			
			// Find hovered entity
			if (player.inputModule.PrefersKeyboardAndMouse()) {
				var position = EntityMonoBehaviour.ToWorldFromRender(Manager.ui.mouse.GetMouseGameViewPosition()).ToFloat2();
				var collisionWorld = PhysicsManager.GetCollisionWorld();

				var outHits = new NativeList<ColliderCastHit>(Allocator.Temp);
				collisionWorld.SphereCastAll(position.ToFloat3(), hoveredDetectionRadius, float3.zero, hoveredDetectionRadius, ref outHits, new CollisionFilter {
					BelongsTo = PhysicsLayerID.Everything,
					CollidesWith = PhysicsLayerID.DefaultCollider
				});

				foreach (var hit in outHits) {
					if (EntityUtility.HasComponentData<ObjectDataCD>(hit.Entity, player.world) && Manager.memory.TryGetEntityMono(hit.Entity, out var hoveredEntityMono)) {
						HoveredEntityMono = hoveredEntityMono;
						break;
					}
				}

				outHits.Dispose();
			}
		}
	}
}