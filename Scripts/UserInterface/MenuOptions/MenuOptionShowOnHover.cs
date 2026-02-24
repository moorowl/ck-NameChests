using System.Collections.Generic;

namespace NameChests.UserInterface.MenuOptions {
	public class MenuOptionShowOnHover : MenuOptionCycling<bool> {
		protected override List<bool> AvailableOptions => new() {
			true,
			false
		};

		protected override bool CurrentOption {
			get => Options.Instance.ShowOnHover;
			set => Options.Instance.ShowOnHover = value;
		}
		
		protected override void UpdateText() {
			valueText.Render(CurrentOption ? "on" : "off");
		}
	}
}