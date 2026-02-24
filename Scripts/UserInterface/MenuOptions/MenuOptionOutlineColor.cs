using NameChests.Utilities;

namespace NameChests.UserInterface.MenuOptions {
	public class MenuOptionOutlineColor : MenuOptionColorSlider {
		protected override HsvColor CurrentColor {
			get => Options.Instance.OutlineColor;
			set => Options.Instance.OutlineColor = value;
		}
	}
}