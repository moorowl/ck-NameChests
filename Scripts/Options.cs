using System;
using PugMod;
using UnityEngine;
// ReSharper disable PossibleInvalidOperationException

namespace NameChests {
	public static class Options {
		public static bool ShowAlways { get; private set; }
		public static bool ShowOnHover { get; private set; }
		
		public static Color Color { get; private set; }
		public static Color ColorPlaceholder { get; private set; }
		public static Color ColorOutline { get; private set; }

		private const string DefaultColor = "#ffffff";
		private const string DefaultColorPlaceholder = "#b2b2b2";
		private const string DefaultColorOutline = "#000000";
		
		public static void Init() {
			ShowAlways = RegisterAndGet("General", nameof(ShowAlways), "Always shows in-world labels.", false);
			ShowOnHover = RegisterAndGet("General", nameof(ShowOnHover), "Shows in-world labels when the object is hovered over.", false);

			Color = RegisterAndGet("Colors", nameof(Color), "Color of labels.", DefaultColor, value => {
				return Utils.ParseHexColor(value) ?? Utils.ParseHexColor(DefaultColor).Value;
			});
			ColorPlaceholder = RegisterAndGet("Colors", nameof(ColorPlaceholder), "Color of the 'Name...' placeholder.", DefaultColorPlaceholder, value => {
				return Utils.ParseHexColor(value) ?? Utils.ParseHexColor(DefaultColorPlaceholder).Value;
			});
			ColorOutline = RegisterAndGet("Colors", nameof(ColorOutline), "Outline color of labels.", DefaultColorOutline, value => {
				return Utils.ParseHexColor(value) ?? Utils.ParseHexColor(DefaultColorOutline).Value;
			});
		}
		
		private static T RegisterAndGet<T>(string section, string key, string description, T defaultValue) {
			return API.Config.Register(Main.Name, section, description, key, defaultValue).Value;
		}
        
		private static TValue RegisterAndGet<T, TValue>(string section, string key, string description, T defaultValue, Func<T, TValue> parser) {
			return parser(RegisterAndGet(section, key, description, defaultValue));
		}
	}
}