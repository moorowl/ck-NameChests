using System.Collections.Generic;

namespace NameChests.UserInterface.MenuOptions {
	public abstract class MenuOptionCycling<T> : RadicalMenuOption {
		protected abstract List<T> AvailableOptions { get; }
		protected abstract T CurrentOption { get; set; }

		protected abstract void UpdateText();

		private int _lastLevel;
		private int CurrentLevel {
			get => AvailableOptions.IndexOf(CurrentOption);
			set => CurrentOption = AvailableOptions[value];
		}
		
		public override void OnParentMenuActivation() {
			base.OnParentMenuActivation();
			SetLevel(CurrentLevel);
			UpdateText();
		}

		protected override void LateUpdate() {
			base.LateUpdate();

			if (_lastLevel != CurrentLevel) {
				UpdateText();
				_lastLevel = CurrentLevel;
			}
		}

		public override void OnActivated() {
			base.OnActivated();
			OnSkimRight();
		}

		public override bool OnSkimRight() {
			ChangeLevel(1);
			return true;
		}

		public override bool OnSkimLeft() {
			ChangeLevel(-1);
			return true;
		}

		private void ChangeLevel(int amount) {
			SetLevel((CurrentLevel + amount) % AvailableOptions.Count);
		}

		private void SetLevel(int level) {
			CurrentLevel = (level >= 0 && level <= AvailableOptions.Count) ? level : 0;
		}
	}
}