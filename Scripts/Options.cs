using System;
using System.Text;
using NameChests.UserInterface;
using NameChests.Utilities;
using Newtonsoft.Json;
using PugMod;
using UnityEngine;

namespace NameChests {
	public class Options {
		public static Options Instance { get; private set; } = new();
		
		private const string FilePath = Main.InternalName + "/Options.json";
		private const int CurrentVersion = 0;
		private const float AutosaveInterval = 10f;

		public bool ShowOnHover {
			get => _data.ShowOnHover;
			set {
				if (_data.ShowOnHover == value)
					return;
				
				_data.ShowOnHover = value;
				_isDirty = true;
			}
		}
		
		public HsvColor Color {
			get => _data.Color;
			set {
				if (_data.Color.Equals(value))
					return;

				_data.Color = value;
				_isDirty = true;
			}
		}
		
		public HsvColor OutlineColor {
			get => _data.OutlineColor;
			set {
				if (_data.OutlineColor.Equals(value))
					return;

				_data.OutlineColor = value;
				_isDirty = true;
			}
		}

		private OptionsData _data;
		private bool _hasInit;
		private bool _isDirty;
		private float _lastSavedTime;
		
		private void SetData(OptionsData data) {
			_data = data;
			_isDirty = false;
		}
		
		public void Init() {
			MenuAdder.OnInit += () => {
				var menu = MenuAdder.AddMenu((RadicalOptionsMenu) Manager.menu.uiOptionsMenu, 19902, "NameChests-Options/Header");
				menu.AddOptionFromPath(Main.AssetBundle, $"Assets/{Main.InternalName}/Prefabs/MenuOptions.prefab");
			};
			
			Load();
			_hasInit = true;
		}

		public void Update() {
			if (!_hasInit)
				return;
			
			if (_isDirty && Time.unscaledTime > _lastSavedTime + AutosaveInterval)
				Save();
		}

		public void SetDefaults() {
			SetData(new OptionsData());
			Save();
		}
		
		public void Save() {
			if (!API.ConfigFilesystem.DirectoryExists(Main.InternalName))
				API.ConfigFilesystem.CreateDirectory(Main.InternalName);
			
			try {
				var serializedData = JsonConvert.SerializeObject(_data);
				API.ConfigFilesystem.Write(FilePath, Encoding.UTF8.GetBytes(serializedData));
			} catch (Exception ex) {
				Debug.Log($"[{Main.DisplayName}]: Error while saving options");
				Debug.LogException(ex);
			}
			
			_isDirty = false;
			_lastSavedTime = Time.unscaledTime;
		}
		
		private void Load() {
			if (!API.ConfigFilesystem.FileExists(FilePath)) {
				SetDefaults();
				return;
			}
			
			try {
				var deserializedData = JsonConvert.DeserializeObject<OptionsData>(Encoding.UTF8.GetString(API.ConfigFilesystem.Read(FilePath)));
				SetData(deserializedData);
			} catch (Exception ex) {
				Debug.Log($"[{Main.DisplayName}]: Error while loading options, using defaults");
				Debug.LogException(ex);
				SetDefaults();
			}
		}
		
		private class OptionsData {
			public int Version { get; set; } = CurrentVersion;
			public bool ShowOnHover { get; set; }
			public HsvColor Color { get; set; } = new(0f, 0f, 1f);
			public HsvColor OutlineColor { get; set; } = new(0f, 0f, 0f);
		}
	}
}