using System.Linq;
using PugMod;
using Interaction;
using Pug.UnityExtensions;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming

namespace NameChests {
	public class Main : IMod {
		public const string Version = "1.2";
		public const string InternalName = "NameChests";
		public const string DisplayName = "Chest Labels";

		private const float HoveredDetectionRadius = 0.01f;
		private static readonly float2 HoveredDetectionOffset = new(0f, -0.2f);
		
		internal static GameObject ChestNameInputPrefab { get; private set; }
		internal static GameObject ChestLabelPrefab { get; private set; }
		
		internal static AssetBundle AssetBundle { get; private set; }
		
		public static EntityMonoBehaviour InteractingEntityMono { get; private set; }
		public static EntityMonoBehaviour HoveredEntityMono { get; private set; }

		public void EarlyInit() {
			Debug.Log($"[{DisplayName}]: Mod version: {Version}");
			
			var modInfo = API.ModLoader.LoadedMods.FirstOrDefault(modInfo => modInfo.Handlers.Contains(this));
			AssetBundle = modInfo!.AssetBundles[0];
			
			ChestNameInputPrefab = AssetBundle.LoadAsset<GameObject>($"Assets/{InternalName}/Prefabs/ChestNameInput.prefab");
			ChestLabelPrefab = AssetBundle.LoadAsset<GameObject>($"Assets/{InternalName}/Prefabs/ChestLabel.prefab");
			
			Options.Instance.Init();
		}

		public void Init() { }
		
		public void ModObjectLoaded(Object obj) { }
		
		public void Shutdown() { }

		public void Update() {
			Options.Instance.Update();
			
			UpdateInteractingAndHoveredEntities();
		}

		private static void UpdateInteractingAndHoveredEntities() {
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
				var position = EntityMonoBehaviour.ToWorldFromRender(Manager.ui.mouse.GetMouseGameViewPosition()).ToFloat2() + HoveredDetectionOffset;
				var collisionWorld = PhysicsManager.GetCollisionWorld();

				var outHits = new NativeList<ColliderCastHit>(Allocator.Temp);
				collisionWorld.SphereCastAll(position.ToFloat3(), HoveredDetectionRadius, float3.zero, HoveredDetectionRadius, ref outHits, new CollisionFilter {
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