using DevInterface;
using UnityEngine;

namespace Dawn.Components {
	class DawnTimeButton : DawnButton {
		readonly Time time;

		public DawnTimeButton(DevUI owner, string IDstring, Time time, DevUINode parentNode, Vector2 pos, float width, string text) : base(owner, IDstring, parentNode, pos, width, text) {
			this.time = time;
		}

		public override bool Active() {
			return DawnDevTools.currentTime == this.time;
		}
	}
}