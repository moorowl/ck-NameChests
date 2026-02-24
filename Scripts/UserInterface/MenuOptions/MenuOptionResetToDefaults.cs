using System.Collections.Generic;
using UnityEngine;

namespace NameChests.UserInterface.MenuOptions {
	public class MenuOptionResetToDefaults : RadicalMenuOption {
		public override void OnActivated() {
			base.OnActivated();
			
			Manager.menu.centerPopUpText.StartNewDisplaySequence(
				"NameChests-Options/ResetToDefaultsDesc",
				null,
				menuInputCooldown: true,
				fadeTime: 0f,
				staticTime: 1.5f,
				useUnscaledTime: true,
				yPosition: 0f,
				textBackgroundAlpha: 1f,
				localize: true,
				TextManager.FontFace.boldMedium,
				response => {
					if (response.IsCancel)
						return;

					Options.Instance.SetDefaults();

					MenuOptionColorSlider.PreventInteractionUntil = Time.unscaledTime + 1f;
				},
				options: new List<string> { "cancelDialogue", "yes" },
				minWidth: 10f,
				backgroundAlpha: 0.9f,
				pauseGame: false
			);
		}
	}
}