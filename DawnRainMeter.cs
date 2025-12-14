using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using HUD;

namespace Dawn;

public class DawnRainMeter : HudPart {
	private bool Show {
		get {
			return this.halfTimeBlink > 0 || this.hud.showKarmaFoodRain || this.hud.owner.RevealMap;
		}
	}

	private RainCycle RainCycle => (this.hud.owner as Player)?.room?.world?.rainCycle ?? (this.hud.owner as Player)?.abstractCreature?.world?.rainCycle;

	public Vector2 pos;
	public Vector2 lastPos;
	public int remainVisibleCounter;
	public float fade;
	public float lastFade;
	private float plop;
	public HUDCircle[] circles;
	private float fRain;
	public int halfTimeBlink;
	private bool halfTimeShown;
	public int tickCounter;
	public float tickPulse;

	public DawnRainMeter(HUD.HUD hud, FContainer fContainer) : base(hud) {
		this.lastPos = this.pos;
		int num = Math.Min((this.RainCycle as DawnRainCycle).nextCycleLength / 1200, 30);

		this.circles = new HUDCircle[num];
		for (int i = 0; i < this.circles.Length; i++) {
			this.circles[i] = new HUDCircle(hud, HUDCircle.SnapToGraphic.smallEmptyCircle, fContainer, 3) {
				forceColor = new Color(0.26f, 0.58f, 0.80f)
			};
		}

		if (ModManager.MSC && (hud.owner as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint && hud.map.RegionName != "HR") {
			this.halfTimeShown = true;
		}
	}

	public override void Update() {
		if (this.RainCycle == null)
			return;

		bool flag = (this.hud.owner as Player).room != null && (this.hud.owner as Player).room.game.setupValues.disableRain;

		// if (ModManager.MSC && (this.hud.owner as Player).room != null && (this.hud.owner as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint && this.hud.map.RegionName != "HR") {
		// 	this.halfTimeShown = true;
		// }
		// if (ModManager.MSC && (this.hud.owner as Player).inVoidSea) {
		// 	this.halfTimeShown = true;
		// }

		this.lastPos = this.pos;
		this.pos = this.hud.karmaMeter.pos;
		if (!this.halfTimeShown && !flag && (this.hud.owner as Player).room != null && this.RainCycle.AmountLeft < 0.5f && (this.hud.owner as Player).room.roomSettings.DangerType != RoomRain.DangerType.None && (!ModManager.MMF || !(this.hud.owner as Player).room.world.rainCycle.RegionHidesTimer)) {
			this.halfTimeBlink = 220;
			this.halfTimeShown = true;
		}
		this.lastFade = this.fade;
		if (this.remainVisibleCounter > 0) {
			this.remainVisibleCounter--;
		}
		if (this.halfTimeBlink > 0) {
			this.halfTimeBlink--;
			this.hud.karmaMeter.forceVisibleCounter = Math.Max(this.hud.karmaMeter.forceVisibleCounter, 10);
		}
		if (ModManager.MMF && MMF.cfgTickTock.Value) {
			this.tickPulse = Mathf.Lerp(this.tickPulse, 0f, 0.1f);
		}
		else {
			this.tickPulse = 0f;
		}
		if ((this.hud.karmaMeter.fade > 0f && this.Show) || this.remainVisibleCounter > 0) {
			this.fade = Mathf.Min(1f, this.fade + 0.033333335f);
			if (ModManager.MMF && MMF.cfgTickTock.Value && (this.hud.owner as Player).room != null && this.hud.owner.RevealMap && (this.RainCycle as DawnRainCycle).dayNightCounter > 0f && !(this.hud.owner as Player).room.world.rainCycle.RegionHidesTimer) {
				this.tickCounter++;
				if (this.tickCounter % 240 == 0) {
					(this.hud.owner as Player).room.PlaySound(SoundID.MENU_Checkbox_Check, 0f, 0.85f, 0.75f * UnityEngine.Random.Range(0.8f, 1.2f));
					this.tickPulse = 1f;
				}
				if (this.tickCounter % 240 == 120) {
					(this.hud.owner as Player).room.PlaySound(SoundID.MENU_Checkbox_Uncheck, 0f, 0.85f, 0.75f * UnityEngine.Random.Range(0.8f, 1.2f));
					this.tickPulse = 1f;
				}
			}
		}
		else {
			this.fade = Mathf.Max(0f, this.fade - 0.1f);
		}
		if (this.hud.HideGeneralHud) {
			this.fade = 0f;
		}

		if (this.fade >= 0.7f) {
			this.plop = Mathf.Min(1f, this.plop + 0.05f);
		}
		else {
			this.plop = 0f;
		}

		if (flag) {
			this.fRain = 0f;
		}
		else if ((this.hud.owner as Player).room != null) {
			this.fRain = 1.0f - (this.hud.owner as Player).room.world.rainCycle.dayNightCounter / (1320f * (3.92f + (this.RainCycle as DawnRainCycle).GetNightLengthRatio()));
		}

		bool flag2 = ModManager.MMF && MMF.cfgHideRainMeterNoThreat.Value && (this.hud.owner as Player).room != null && this.RainCycle.RegionHidesTimer;
		for (int i = 0; i < this.circles.Length; i++) {
			this.circles[i].Update();
			if (this.fade > 0f || this.lastFade > 0f) {
				float num = (float) i / (float) (this.circles.Length - 1);
				float value = Mathf.InverseLerp((float) i / (float) this.circles.Length, (float) (i + 1) / (float) this.circles.Length, this.fRain);
				float num2 = Mathf.InverseLerp(0.5f, 0.475f, Mathf.Abs(0.5f - Mathf.InverseLerp(0.033333335f, 1f, value)));
				if (flag2) {
					this.circles[i].rad = (3f * Mathf.Pow(this.fade, 2f) + Mathf.InverseLerp(0.075f, 0f, Mathf.Abs(1f - num - Mathf.Lerp((1f - this.fRain) * this.fade - 0.075f, 1.075f, Mathf.Pow(this.plop, 0.85f)))) * 2f * this.fade) * Mathf.InverseLerp(0f, 0.033333335f, 1f);
					this.circles[i].thickness = 1f;
					this.circles[i].snapGraphic = HUDCircle.SnapToGraphic.smallEmptyCircle;
					this.circles[i].snapRad = 3f;
					this.circles[i].snapThickness = 1f;
				}
				else {
					if (this.halfTimeBlink > 0) {
						num2 = Mathf.Max(num2, (this.halfTimeBlink % 15 < 7) ? 0f : 1f);
					}
					this.circles[i].rad = ((2f + num2) * Mathf.Pow(this.fade, 2f) + Mathf.InverseLerp(0.075f, 0f, Mathf.Abs(1f - num - Mathf.Lerp((1f - this.fRain) * this.fade - 0.075f, 1.075f, Mathf.Pow(this.plop, 0.85f)))) * 2f * this.fade) * Mathf.InverseLerp(0f, 0.033333335f, value);
					if (num2 == 0f) {
						this.circles[i].thickness = -1f;
						this.circles[i].snapGraphic = HUDCircle.SnapToGraphic.Circle4;
						this.circles[i].snapRad = 2f;
						this.circles[i].snapThickness = -1f;
					}
					else {
						this.circles[i].thickness = Mathf.Lerp(3.5f, 1f, num2);
						this.circles[i].snapGraphic = HUDCircle.SnapToGraphic.smallEmptyCircle;
						this.circles[i].snapRad = 3f;
						this.circles[i].snapThickness = 1f;
					}
				}
				this.circles[i].pos = this.pos + Custom.DegToVec((1f - (float) i / (float) this.circles.Length) * 360f * Custom.SCurve(Mathf.Pow(this.fade, 1.5f - num), 0.6f)) * (this.hud.karmaMeter.Radius + 8.5f + num2 + 4f * this.tickPulse);

			}
			else {
				this.circles[i].rad = 0f;
			}
		}

		if ((this.RainCycle as DawnRainCycle).dayNightCounter <= 0 || !(this.RainCycle as DawnRainCycle).inRoomWithDawn) {
			this.fade = 0f;
			for (int i = 0; i < this.circles.Length; i++) {
				this.circles[i].rad = 0f;
				this.circles[i].snapRad = 0f;
			}
		}
	}

	public Vector2 DrawPos(float timeStacker) {
		return Vector2.Lerp(this.lastPos, this.pos, timeStacker);
	}

	public override void Draw(float timeStacker) {
		if (ModManager.MSC && this.hud.owner.GetOwnerType() == HUD.HUD.OwnerType.Player && (this.hud.owner as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint && this.hud.map.RegionName != "HR")
			return;

		for (int i = 0; i < this.circles.Length; i++)
			this.circles[i].Draw(timeStacker);
	}

	public void ResetHalfTime() {
		this.halfTimeShown = false;
		this.halfTimeBlink = 0;
	}

	public void SuppressHalfTime() {
		this.halfTimeShown = true;
	}
}
