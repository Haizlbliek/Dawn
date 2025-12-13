using System;
using System.Globalization;
using System.Text.RegularExpressions;
using DevInterface;
using RWCustom;
using UnityEngine;

namespace Dawn {
	class DawnObject : PlacedObject {
		public DawnObject(Type type, Data data) : base(type, data) {
		}
	}

	class DawnObjectRepresentation : PlacedObjectRepresentation {
		public DawnObjectRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name) {
			this.subNodes.Add(new DawnControlPanel(owner, "Dawn_Control_Panel", this, new Vector2(10f, 10f)));
			(this.subNodes[this.subNodes.Count - 1] as DawnControlPanel).pos = (pObj.data as DawnObjectData).panelPos;

			// (pObj.data as DawnObjectData).Apply(owner.room);
		}

		public override void Refresh() {
			base.Refresh();
			(this.pObj.data as DawnObjectData).panelPos = (this.subNodes[0] as Panel).pos;
		}

		public class DawnControlPanel : Panel, IDevUISignals {
			public DawnControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(200f, 20f), "Dawn Settings") {
				this.subNodes.Add(new PaletteController(owner, "Palette", this, new Vector2(5f, 5f), "Dawn Palette : "));
			}

			public override void Move(Vector2 newPos) {
				base.Move(newPos);
				this.parentNode.Refresh();
			}

			public void Signal(DevUISignalType type, DevUINode sender, string message) {
			}

			public class PaletteController : IntegerControl {
				public PaletteController(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title) {
				}

				public override void Increment(int change) {
					((this.parentNode.parentNode as DawnObjectRepresentation).pObj.data as DawnObjectData).dawnPalette += change;

					if (((this.parentNode.parentNode as DawnObjectRepresentation).pObj.data as DawnObjectData).dawnPalette < 0) {
						((this.parentNode.parentNode as DawnObjectRepresentation).pObj.data as DawnObjectData).dawnPalette = 0;
					}

					this.Refresh();
				}

				public override void Refresh() {
					base.NumberLabelText = ((this.parentNode.parentNode as DawnObjectRepresentation).pObj.data as DawnObjectData).dawnPalette.ToString();

					base.Refresh();
				}
			}
		}
	}

	class DawnObjectData : PlacedObject.Data {
		public DawnObjectData(PlacedObject owner) : base(owner) {
			this.panelPos = Custom.DegToVec(30f) * 10f;
			this.dawnPalette = 23;
		}

		public void Apply(Room room) {
		}

		protected string BaseSaveString() {
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}", [
				this.panelPos.x,
				this.panelPos.y,
				this.dawnPalette
			]);
		}

		public override string ToString() {
			string text = this.BaseSaveString();
			text = SaveState.SetCustomData(this, text);
			return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", this.unrecognizedAttributes);
		}

		public override void FromString(string s) {
			string[] array = Regex.Split(s, "~");
			try {
				this.panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				this.dawnPalette = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			}
			catch (Exception) {
			}
			this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 4);
		}

		public Vector2 panelPos;
		public int dawnPalette;
	}
}