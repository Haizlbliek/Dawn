using DevInterface;
using UnityEngine;

namespace Dawn.Components {
	class DawnButton : Button {
		private static Color NIGHT_PRESSED_COLOR = new Color(0.0f, 0.1f, 0.5f);
		private static Color NIGHT_HOVER_COLOR = new Color(0.2f, 0.5f, 0.9f);
		private static Color NIGHT_TEXT_COLOR = new Color(0.1f, 0.5f, 1.0f);

		private static Color DAY_PRESSED_COLOR = new Color(0.5f, 0.1f, 0.0f);
		private static Color DAY_HOVER_COLOR = new Color(0.9f, 0.5f, 0.2f);
		private static Color DAY_TEXT_COLOR = new Color(1.0f, 0.5f, 0.1f);

		private static Color ERROR_COLOR = new Color(1.0f, 0.1f, 0.2f);

		public DawnButton(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, float width, string text) : base(owner, IDstring, parentNode, pos, width, text) {
		}

		private void ColorNight() {
			if (this.down) {
				this.colorA = NIGHT_TEXT_COLOR;
				this.colorB = NIGHT_PRESSED_COLOR;
			} else if (base.MouseOver) {
				this.colorA = NIGHT_TEXT_COLOR;
				this.colorB = NIGHT_HOVER_COLOR;
			} else {
				this.colorA = NIGHT_TEXT_COLOR;
				this.colorB = new Color(0.9f, 0.9f, 1.0f);
			}
		}
		
		private void ColorDay() {
			if (this.down) {
				this.colorA = DAY_TEXT_COLOR;
				this.colorB = DAY_PRESSED_COLOR;
			} else if (base.MouseOver) {
				this.colorA = DAY_TEXT_COLOR;
				this.colorB = DAY_HOVER_COLOR;
			} else {
				this.colorA = DAY_TEXT_COLOR;
				this.colorB = Color.yellow;
			}
		}
		
		public virtual bool Active() {
			return true;
		}

		public override void Update() {
			if (this.owner == null) {
				this.colorA = ERROR_COLOR * 3.0f;
				this.colorB = ERROR_COLOR;
				return;
			}

			if (Active()) {
				ColorDay();
			} else {
				ColorNight();
			}

			this.overrideTextColor = this.colorB;
			base.Update();
		}
	}
}