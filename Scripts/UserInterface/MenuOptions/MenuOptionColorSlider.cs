using System;
using System.Linq;
using NameChests.Utilities;
using Pug.UnityExtensions;
using PugMod;
using UnityEngine;

namespace NameChests.UserInterface.MenuOptions {
	public abstract class MenuOptionColorSlider : RadicalMenuOption {
		public static float PreventInteractionUntil;
		private static bool IsInteractionAllowed => Time.unscaledTime >= PreventInteractionUntil;
		
		public enum ColorComponent {
			Hue,
			Saturation,
			Value,
			Alpha
		}
		
		private static readonly MemberInfo MiMenuSelectionInputCooldownTimer = typeof(InputManager).GetMembersChecked().FirstOrDefault(x => x.GetNameChecked() == "menuSelectionInputCooldownTimer");
		private static readonly MemberInfo MiSfxCooldownTimer = typeof(MenuManager).GetMembersChecked().FirstOrDefault(x => x.GetNameChecked() == "sfxCooldownTimer");
		
		private const char StepChar = '|';
		
		public ColorComponent component;
		public int numberOfSteps = 90;
		public BoxCollider valueCollider;
		public SpriteRenderer border;
		public Transform pointer;

		private bool _isActive;
		private int _currentStep;
		
		protected abstract HsvColor CurrentColor { get; set; }

		public float CurrentComponent {
			get {
				return component switch {
					ColorComponent.Hue => CurrentColor.Hue,
					ColorComponent.Saturation => CurrentColor.Saturation,
					ColorComponent.Value => CurrentColor.Value,
					ColorComponent.Alpha => CurrentColor.Alpha,
					_ => throw new ArgumentOutOfRangeException()
				};
			}
			set {
				var clampedValue = Mathf.Clamp01(value);
				if (component == ColorComponent.Hue)
					CurrentColor.Hue = clampedValue;
				else if (component == ColorComponent.Saturation)
					CurrentColor.Saturation = clampedValue;
				else if (component == ColorComponent.Value)
					CurrentColor.Value = clampedValue;
				else if (component == ColorComponent.Alpha)
					CurrentColor.Alpha = clampedValue;
			}
		}

		private void Start() {
			UpdateVisuals(false);
		}
		
		public override void OnParentMenuActivation() {
			SyncStepWithOptions();
			base.OnParentMenuActivation();
		}

		public void SyncStepWithOptions() {
			SetStep(Mathf.RoundToInt(CurrentComponent * numberOfSteps));
		}
		
		public override bool OnSkimRight() {
			return OnSkimDelta(1);
		}

		public override bool OnSkimLeft() {
			return OnSkimDelta(-1);
		}
		
		private bool OnSkimDelta(int delta) {
			if (leftClickIsHeldDown)
				return false;
			
			var inputTimer = (TimerSimple) API.Reflection.GetValue(MiMenuSelectionInputCooldownTimer, Manager.input);
			inputTimer.FastForward(inputTimer.remainingTime / 2f);
			API.Reflection.SetValue(MiMenuSelectionInputCooldownTimer, Manager.input, inputTimer);
			
			var sfxTimer = (TimerSimple) API.Reflection.GetValue(MiSfxCooldownTimer, Manager.menu);
			sfxTimer.Start();
			API.Reflection.SetValue(MiSfxCooldownTimer, Manager.menu, sfxTimer);
			
			var previousStep = _currentStep;
			SetStep(_currentStep + delta);
			
			if (_currentStep != previousStep)
				AudioManager.SfxUI(SfxID.FIXME_menu_select, 0.975f + _currentStep / (float) numberOfSteps * 0.05f, true, 1f, 0f, true);
			
			return true;
		}
		
		public void SetStep(int step) {
			var num = Mathf.Clamp(step, 0, numberOfSteps - 1);
			if (_currentStep != num) {
				_currentStep = num;

				UpdateVisuals(IsSelected());
				CurrentComponent = _currentStep / (float) numberOfSteps;
			}
		}
		
		public override void OnSelected() {
			base.OnSelected();
			
			UpdateVisuals(true);
		}

		public override void OnDeselected(bool playEffect = true) {
			base.OnDeselected(playEffect);
			Cleanup();
		}

		public override void OnActivated() {
			_isActive = !_isActive;
			base.OnActivated();
		}

		protected override void UpdateClickCollider() {
			base.UpdateClickCollider();
			clickCollider.center = new Vector3(0f, clickCollider.center.y, clickCollider.center.z);
			clickCollider.size = new Vector3(22f, clickCollider.size.y, clickCollider.size.z);
		}

		protected override void LateUpdate() {
			base.LateUpdate();
			
			var expectedStep = Mathf.RoundToInt(CurrentComponent * numberOfSteps);
			var isSelected = IsSelected() && IsInteractionAllowed;
			UpdateVisuals(isSelected);

			var inputModule = Manager.input.singleplayerInputModule;

			if (isSelected && inputModule.PrefersKeyboardAndMouse() && leftClickIsHeldDown) {
				Manager.physics.RaycastNonAlloc(Manager.ui.mouse.pointer.transform.position + Vector3.back * 5f, Vector3.forward, 10f, includeTriggers: true, ObjectLayerID.UILayerMask, out var hits);

				foreach (var hit in hits) {
					if (hit.collider != valueCollider)
						continue;
					
					var hitPoint = hit.point;
					var value = RoundToPixelPerfectPosition.RoundPosition(hitPoint - valueCollider.transform.position).x / (valueCollider.size.x - 1f / 16f);
					expectedStep = Mathf.RoundToInt(value * numberOfSteps) - 1;

					break;
				}	
			}

			if (expectedStep != _currentStep)
				SetStep(expectedStep);
		}

		public void UpdateVisuals(bool isSelected) {
			const float selectedOpacity = 1f;
			const float unselectedOpacity = 0.65f;
			
			pointer.localPosition = RoundToPixelPerfectPosition.RoundPosition(new Vector3(((_currentStep) / 16f) + (2f / 16f), pointer.localPosition.y, pointer.localPosition.z));
			
			valueText.Render();
			
			var opacity = isSelected ? selectedOpacity : unselectedOpacity;
			for (var i = 0; i < valueText.glyphs.Count; i++) {
				var baseSegmentColor = component switch {
					ColorComponent.Hue => CurrentColor with {
						Hue = i / (float) numberOfSteps
					},
					ColorComponent.Saturation => CurrentColor with {
						Saturation = i / (float) numberOfSteps
					},
					ColorComponent.Value => CurrentColor with {
						Value = i / (float) numberOfSteps
					},
					ColorComponent.Alpha => CurrentColor with {
						Alpha = i / (float) numberOfSteps
					},
					_ => throw new ArgumentOutOfRangeException()
				};
				var segmentColor = baseSegmentColor.Rgba;
				segmentColor.a *= opacity;
				
				valueText.glyphs[i].color = segmentColor;
			}
			
			if (valueCollider != null) {
				var width = Mathf.Abs(valueText.dimensions.width);
				var height = Mathf.Abs(valueText.dimensions.height);
				valueCollider.size = new Vector3(width - (1f / 16f), (height / 2f) + (3f / 16f), valueCollider.size.z);
				valueCollider.center = new Vector3((valueCollider.size.x / 2f) + (1f / 16f), valueCollider.center.y, valueCollider.size.z);
			}
			
			if (border != null) {
				border.size = new Vector2(valueCollider.size.x + (4f / 16f), valueCollider.size.y + (4f / 16f));
				border.transform.localPosition = new Vector3(valueCollider.center.x, border.transform.localPosition.y, border.transform.localPosition.z);
			}

			var srColor = new Color(opacity, opacity, opacity, 1f);
			border.color = srColor;
			pointer.GetComponent<SpriteRenderer>().color = srColor;
		}
		
		private void Cleanup() {
			UpdateVisuals(false);
			_isActive = false;
		}

		public void OnValidate() {
			if (valueText != null) {
				valueText.SetText(new string(StepChar, numberOfSteps));
				valueText.style.extraCharSpacing = -2;
			}
		}
	}
}