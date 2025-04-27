using DevInterface;
using UnityEngine;

namespace Dawn.Components {
	class DawnEnableButton : DawnButton {
		public DawnEnableButton(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, float width, string text) : base(owner, IDstring, parentNode, pos, width, text) {
		}

		public override void Clicked() {
			base.Clicked();
			
			if (DawnDevTools.dawnDevToolsActive) {
				this.Text = "Dawn - Enabled";
			} else {
				this.Text = "Dawn - Disabled";
			}
		}
	}
}