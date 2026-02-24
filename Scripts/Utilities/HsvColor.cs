using Newtonsoft.Json;
using Pug.UnityExtensions;
using UnityEngine;

namespace NameChests.Utilities {
	public record HsvColor {
		public float Hue;
		public float Saturation;
		public float Value;
		public float Alpha;
		
		[JsonIgnore]
		public Color Rgba => Color.HSVToRGB(Hue, Saturation, Value).ColorWithNewAlpha(Alpha);

		public HsvColor(float hue, float saturation, float value, float alpha = 1f) {
			Hue = hue;
			Saturation = saturation;
			Value = value;
			Alpha = alpha;
		}
	}
}