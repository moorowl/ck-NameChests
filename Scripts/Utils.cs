﻿using System.Globalization;
using System.Text;
using Unity.Entities;
using UnityEngine;

namespace NameChests {
	public static class Utils {
		public static bool SupportsNaming(EntityMonoBehaviour entityMono) {
			return entityMono is Chest { showSortAndQuickStackButtons: true } or (Pedestal and not AncientGiant) or Mannequin or Aquarium or Terrarium;
		}

		public static string GetDescription(EntityMonoBehaviour entityMono, ref string currentTextCache, ref byte[] currentUtf8TextCache) {
			if (entityMono.entityExist && EntityUtility.TryGetBuffer<DescriptionBuffer>(entityMono.entity, entityMono.world, out var buffer))
				return GetDescriptionFromBuffer(buffer, ref currentTextCache, ref currentUtf8TextCache);

			return null;
		}
		
		public static string GetDescriptionFromBuffer(DynamicBuffer<DescriptionBuffer> buffer, ref string currentTextCache, ref byte[] currentUtf8TextCache) {
			if (AreEqual(currentUtf8TextCache, buffer))
				return currentTextCache;
			
			currentUtf8TextCache = new byte[buffer.Length];
			for (var i = 0; i < currentUtf8TextCache.Length; i++)
				currentUtf8TextCache[i] = buffer[i].Value;

			currentTextCache = Encoding.UTF8.GetString(currentUtf8TextCache);
			return currentTextCache;
		}
		
		private static bool AreEqual(byte[] currentUtf8Text, DynamicBuffer<DescriptionBuffer> newUtf8Text) {
			if (currentUtf8Text == null || currentUtf8Text.Length != newUtf8Text.Length)
				return false;

			for (var i = 0; i < currentUtf8Text.Length; i++) {
				if (currentUtf8Text[i] != newUtf8Text[i].Value) {
					return false;
				}
			}

			return true;
		}
		
		public static Color? ParseHexColor(string hexColor) {
			if (string.IsNullOrEmpty(hexColor) || hexColor.Length < 7)
				return null;

			hexColor = hexColor.Replace("#", "");

			return new Color(
				byte.Parse(hexColor.Substring(0, 2), NumberStyles.HexNumber) / 255f,
				byte.Parse(hexColor.Substring(2, 2), NumberStyles.HexNumber) / 255f,
				byte.Parse(hexColor.Substring(4, 2), NumberStyles.HexNumber) / 255f,
				1f
			);
		}
	}
}