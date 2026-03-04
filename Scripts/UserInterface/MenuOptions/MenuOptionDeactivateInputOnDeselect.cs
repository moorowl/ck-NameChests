using System.Collections.Generic;

namespace NameChests.UserInterface.MenuOptions {
	public class MenuOptionDeactivateInputOnDeselect : MenuOptionCycling<bool> {
		protected override List<bool> AvailableOptions => new() {
			true,
			false
		};

		protected override bool CurrentOption {
			get => Options.Instance.DeactivateInputOnDeselect;
			set => Options.Instance.DeactivateInputOnDeselect = value;
		}
		
		protected override void UpdateText() {
			valueText.Render(CurrentOption ? "NameChests-Options/DeactivateInputOnDeselect_OnDeselect" : "NameChests-Options/DeactivateInputOnDeselect_OnEnter");
		}
	}
}