using System;
using System.Collections.Generic;
using HarmonyLib;
using PugMod;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming

namespace NameChests.UserInterface {
	public static class MenuAdder {
		// Called after MenuManager initializes
		public static event Action OnInit;

		private static readonly Dictionary<RadicalMenu.MenuType, RadicalMenu> MenusByType = new();

		public static RadicalMenu AddMenu(RadicalOptionsMenu parentMenu, int id, string term, int siblingOffset = 0) {
			// Copy UI settings
			var modMenu = Object.Instantiate(Manager.menu.uiOptionsMenuPrefab, API.Rendering.UICamera.transform).GetComponent<RadicalOptionsMenu>();
			
			// Update title text at top
			modMenu.gameObject.SetActive(value: false);
			modMenu.transform.Find("Title/Title bigtext").GetComponent<PugText>().Render(term);
			modMenu.transform.Find("Title/Title bigtext shadow").GetComponent<PugText>().Render(term);

			// Clear the existing menu options
			var modMenuScroll = modMenu.transform.Find("Options/Scroll");
			modMenu.menuOptions.Clear();
			for (var i = 0; i < modMenuScroll.childCount; i++)
				Object.Destroy(modMenuScroll.GetChild(i).gameObject);

			// Add option to open mod settings in main settings (above controls, below UI settings)
			var settingsMenuScroll = parentMenu.transform.Find("Options/Scroll");
			var goToUiSettings = Manager.menu.optionsMenu.transform.Find("Options/Scroll").Find("Go to UI settings").GetComponent<RadicalMenuOption>();
			var goToModSettings = Object.Instantiate(goToUiSettings.gameObject, settingsMenuScroll).GetComponent<RadicalOptionsMenuOption_PushMenu>();
			if (siblingOffset != 0)
				goToModSettings.transform.SetSiblingIndex(goToUiSettings.transform.GetSiblingIndex() + siblingOffset);
			goToModSettings.labelText.Render(term);
			goToModSettings.menuToPush = (RadicalMenu.MenuType) id;
			goToModSettings.SetParentMenu(parentMenu);
			// parentMenu.menuOptions.Insert(goToModSettings.transform.GetSiblingIndex(), goToModSettings);

			MenusByType[goToModSettings.menuToPush] = modMenu;
			
			return modMenu;
		}

		public static void AddOptionFromPath(this RadicalMenu menu, AssetBundle assetBundle, string prefabPath) {
			var scroll = menu.transform.Find("Options/Scroll");
			var prefab = assetBundle.LoadAsset<GameObject>(prefabPath);
			
			foreach (var prefabMenuOption in prefab.GetComponentsInChildren(typeof(RadicalMenuOption), true)) {
				var instance = Object.Instantiate(prefabMenuOption.gameObject, scroll);
				
				var menuOption = instance.GetComponent<RadicalMenuOption>();
				menuOption.SetParentMenu(menu);
				menu.menuOptions.Add(menuOption);
			}
		}

		[HarmonyPatch]
		public static class Patches {
			[HarmonyPatch(typeof(MenuManager), "Init")]
			[HarmonyPostfix]
			public static void MenuManager_Init(MenuManager __instance) {
				OnInit?.Invoke();
			}

			[HarmonyPatch(typeof(RadicalMenu), "TypeToMenu")]
			[HarmonyPrefix]
			public static bool RadicalMenu_TypeToMenu(RadicalMenu.MenuType type, ref RadicalMenu __result) {
				if (!MenusByType.TryGetValue(type, out var menu))
					return true;
				
				__result = menu;
				return false;
			}
		}
	}
}