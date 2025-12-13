using System;
using System.Globalization;
using System.Text.RegularExpressions;
using DevInterface;
using RWCustom;
using UnityEngine;

namespace Dawn {
	enum ColorPart {
		Red,
		Green,
		Blue
	}

	public class CustomEffectColours : PlacedObjectRepresentation {
		public CustomEffectColours(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name) {
			this.subNodes.Add(new CustomEffectColoursControlPanel(owner, "Colours_Control_Panel", this, new Vector2(10f, 10f), name));

			(this.subNodes[this.subNodes.Count - 1] as CustomEffectColoursControlPanel).pos = (pObj.data as CustomEffectColoursData).panelPos;
		}

		public override void Refresh() {
			base.Refresh();

			(this.pObj.data as CustomEffectColoursData).panelPos = (this.subNodes[0] as Panel).pos;
		}

		public class CustomEffectColoursControlPanel : Panel {
			public CustomEffectColoursControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string name) : base(owner, IDstring, parentNode, pos, new Vector2(250f, 105f + 15f), name) {
				this.subNodes.Add(new PaletteController(0, ColorPart.Red, owner, "PaletteAR", this, new Vector2(5f, 105f), "ColorA R: "));
				this.subNodes.Add(new PaletteController(0, ColorPart.Green, owner, "PaletteAG", this, new Vector2(5f, 085f), "ColorA G: "));
				this.subNodes.Add(new PaletteController(0, ColorPart.Blue, owner, "PaletteAB", this, new Vector2(5f, 065f), "ColorA B: "));
				this.subNodes.Add(new PaletteController(1, ColorPart.Red, owner, "PaletteBR", this, new Vector2(5f, 045f), "ColorB R: "));
				this.subNodes.Add(new PaletteController(1, ColorPart.Green, owner, "PaletteBG", this, new Vector2(5f, 025f), "ColorB G: "));
				this.subNodes.Add(new PaletteController(1, ColorPart.Blue, owner, "PaletteBB", this, new Vector2(5f, 005f), "ColorB B: "));
			}

			public override void Move(Vector2 newPos) {
				base.Move(newPos);
				this.parentNode.Refresh();
			}

			private class PaletteController : Slider {
				private readonly int id;
				private readonly ColorPart part;
				private readonly CustomEffectColoursData data;

				public PaletteController(int id, ColorPart part, DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 110f) {
					this.id = id;
					this.part = part;
					this.data = (this.parentNode.parentNode as CustomEffectColours).pObj.data as CustomEffectColoursData;
				}

				public override void NubDragged(float nubPos) {
					this.data.SetValue(this.id, this.part, nubPos);

					this.Refresh();
				}

				public override void Refresh() {
					base.Refresh();

					float value = this.data.GetValue(this.id, this.part);
					base.RefreshNubPos(value);
					base.NumberText = ((int) (value * 255.0f)).ToString();
				}
			}
		}
	}

	class CustomEffectColoursData : PlacedObject.Data {
		public CustomEffectColoursData(PlacedObject owner) : base(owner) {
			this.panelPos = Custom.DegToVec(30f) * 10f;
			this.colourA = Color.red;
			this.colourB = Color.green;
		}

		public void Apply(Room room) {
		}

		protected string BaseSaveString() {
			return String.Join("~", [
				1,
				this.panelPos.x,
				this.panelPos.y,
				this.colourA.r,
				this.colourA.g,
				this.colourA.b,
				this.colourB.r,
				this.colourB.g,
				this.colourB.b
			]);
		}

		public override string ToString() {
			string text = this.BaseSaveString();
			text = SaveState.SetCustomData(this, text);
			return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", this.unrecognizedAttributes);
		}

		public override void FromString(string s) {
			string[] array = Regex.Split(s, "~");
			int version = int.Parse(array[0]);

			switch (version) {
				case 0:
					if (array.Length < 3)
						return;

					this.panelPos.x = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					this.panelPos.y = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
					this.colourA = Color.red;
					this.colourB = Color.green;
					break;

				case 1:
					if (array.Length < 9)
						return;

					this.panelPos.x = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					this.panelPos.y = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
					this.colourA.r = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
					this.colourA.g = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
					this.colourA.b = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
					this.colourB.r = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
					this.colourB.g = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
					this.colourB.b = float.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture);

					break;

				default:
					throw new Exception("Unknown version of CustomEffectColoursData: " + version);
			}

			this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 4);
		}

		public float GetValue(int id, ColorPart part) {
			Color v = this.Get(id);

			if (part == ColorPart.Red)
				return v.r;
			if (part == ColorPart.Green)
				return v.g;
			if (part == ColorPart.Blue)
				return v.b;

			return -1.0f;
		}

		public void SetValue(int id, ColorPart part, float value) {
			Color v = this.Get(id);

			if (part == ColorPart.Red)
				v.r = value;
			if (part == ColorPart.Green)
				v.g = value;
			if (part == ColorPart.Blue)
				v.b = value;

			switch (id) {
				case 0:
					this.colourA = v;
					break;

				case 1:
					this.colourB = v;
					break;
			}
		}

		private Color Get(int id) {
			return id switch {
				0 => this.colourA,
				1 => this.colourB,
				_ => Color.black,
			};
		}

		public Vector2 panelPos;
		public Color colourA;
		public Color colourB;
	}
}