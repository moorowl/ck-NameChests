using NameChests.Utilities;

namespace NameChests.UserInterface.MenuOptions {
	public class MenuOptionColor : MenuOptionColorSlider {
		protected override HsvColor CurrentColor {
			get => Options.Instance.Color;
			set => Options.Instance.Color = value;
		}
	}
}