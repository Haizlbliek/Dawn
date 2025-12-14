using System.Security.Permissions;
using BepInEx;
using DevInterface;
using MoreSlugcats;
using UnityEngine;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace Dawn;

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
	public const string PLUGIN_GUID = "goofybox.dawn";
	public const string PLUGIN_NAME = "Dawn";
	public const string PLUGIN_VERSION = "1.0.1";

	public static string MOD_ID = "dawn";

	public Time timeLerpA = Time.NONE;
	public Time timeLerpB = Time.NONE;
	public float lerpAmount = 0.0f;
	public Room currentRoom = null;
	public int currentCameraPosition = 0;

	public static Plugin instance = null;

	public static void Log(object data) {
		instance.SelfLog(data);
	}

	public void SelfLog(object data) {
		this.Logger.LogInfo(data);
		Debug.Log(data);
	}

	public void OnEnable() {
		instance = this;

		DawnEnums.Initialize();
		DawnDevTools.Initialize();
		TerrainController.Initialize();

		On.RoomCamera.UpdateDayNightPalette += this.On_RoomCamera_UpdateDayNightPalette;
		On.RainCycle.Update += this.On_RainCycle_Update;
		On.DevInterface.RoomSettingsPage.DevEffectGetCategoryFromEffectType += this.On_DevInterface_RoomSettingsPage_DevEffectGetCategoryFromEffectType;
		On.RainTracker.Utility += this.On_RainTracker_Utility;
		On.AbstractCreature.InDenUpdate += this.On_AbstractCreature_InDenUpdate;
		On.HUD.HUD.InitSinglePlayerHud += this.On_HUD_InitSinglePlayerHud;
		On.World.ctor += this.On_World_ctor;
		On.RoomCamera.ApplyFade += this.On_RoomCamera_ApplyFade;
		On.RoomCamera.ApplyPalette += this.On_RoomCamera_ApplyPalette;
		On.RoomCamera.ModifyEffectColorA += this.On_RoomCamera_ModifyEffectColorA;
		On.RoomCamera.ModifyEffectColorB += this.On_RoomCamera_ModifyEffectColorB;
		On.Player.ProcessDebugInputs += this.On_Player_ProcessDebugInputs;
		On.Room.ctor += this.On_Room_ctor;
	}

	private void On_RoomCamera_ApplyFade(On.RoomCamera.orig_ApplyFade orig, RoomCamera self) {
		float effect_dawn = self.room?.roomSettings?.GetEffectAmount(DawnEnums.Dawn) ?? 0f;
		if (effect_dawn > 0f && self.room.world.rainCycle.timer >= self.room.world.rainCycle.cycleLength) {
			return;
		}

		orig(self);
	}

	public void OnDisable() {
		DawnEnums.Cleanup();
		DawnDevTools.Cleanup();
		TerrainController.Cleanup();
	}

	private void On_Room_ctor(On.Room.orig_ctor orig, Room self, RainWorldGame game, World world, AbstractRoom abstractRoom, bool devUI) {
		orig(self, game, world, abstractRoom, devUI);

		self.roomSettings = new DawnRoomSettings(self.roomSettings);
	}

	private void On_Player_ProcessDebugInputs(On.Player.orig_ProcessDebugInputs orig, Player self) {
		if (self.room == null || !self.room.game.devToolsActive)
			return;

		orig(self);

		if (Input.GetKeyDown("d")) {
			DawnRainCycle rainCycle = self.room.world.rainCycle as DawnRainCycle;

			if (rainCycle == null)
				return;

			rainCycle.dayNightCounter += 4000;
		}
	}

	// private Color LerpColor(Color orig, Color halfDusk, Color dusk, Color dark, Color dawn, Color halfDawn) {
	// 	return Color.Lerp(Color.Lerp(Color.Lerp(Color.Lerp(Color.Lerp(
	// 		orig,
	// 		halfDusk, this.fadeBlendHalfDusk),
	// 		dusk, this.fadeBlendDusk),
	// 		dark, this.fadeBlendNight),
	// 		dawn, this.fadeBlendDawn),
	// 		halfDawn, this.fadeBlendHalfDawn
	// 	);
	// }

	// private Color ModifyColor(Color orig, int index) {
	// 	if (index == 0) {
	// 		return this.LerpColor(orig, this.effectColorHalfDuskA, this.effectColorDuskA, this.effectColorDarkA, this.effectColorDawnA, this.effectColorHalfDawnA);
	// 	}
	// 	else if (index == 1) {
	// 		return this.LerpColor(orig, this.effectColorHalfDuskB, this.effectColorDuskB, this.effectColorDarkB, this.effectColorDawnB, this.effectColorHalfDawnB);
	// 	}

	// 	return orig;
	// }

	private Color[] On_RoomCamera_ModifyEffectColorA(On.RoomCamera.orig_ModifyEffectColorA orig, RoomCamera self, Color[] colors) {
		Color[] newColors = orig(self, colors);

		// TODO:
		// for (int i = 0; i < newColors.Length; i++) {
		// 	newColors[i] = this.ModifyColor(newColors[i], 0);
		// }

		return newColors;
	}

	private Color[] On_RoomCamera_ModifyEffectColorB(On.RoomCamera.orig_ModifyEffectColorB orig, RoomCamera self, Color[] colors) {
		Color[] newColors = orig(self, colors);

		// TODO:
		// for (int i = 0; i < newColors.Length; i++) {
		// 	newColors[i] = this.ModifyColor(newColors[i], 1);
		// }

		return newColors;
	}

	private void On_World_ctor(On.World.orig_ctor orig, World self, RainWorldGame game, Region region, string name, bool singleRoomWorld) {
		orig(self, null, region, name, singleRoomWorld);
		self.game = game;

		if (self.game != null) {
			if (self.game.IsStorySession) {
				float minutes;
				if (game.GetStorySession.characterStats.name == SlugcatStats.Name.Yellow || (ModManager.MSC && (game.GetStorySession.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Rivulet || game.GetStorySession.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Gourmand || game.GetStorySession.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Saint))) {
					minutes = Mathf.Lerp(game.rainWorld.setup.cycleTimeMin, game.rainWorld.setup.cycleTimeMax, 0.35f + 0.65f * Mathf.Pow(Random.value, 1.2f)) / 60f;
				}
				else {
					minutes = Mathf.Lerp(game.rainWorld.setup.cycleTimeMin, game.rainWorld.setup.cycleTimeMax, Random.value) / 60f;
				}

				if (ModManager.MMF && MMF.cfgNoRandomCycles.Value)
					minutes = game.rainWorld.setup.cycleTimeMax / 60f;

				self.rainCycle = new DawnRainCycle(self, minutes);

				if (ModManager.TimelineModule && name == "SB")
					self.rainCycle.filtrationPowerBehavior = new FiltrationPowerController(self);
			}
			else {
				self.rainCycle = new RainCycle(self, game.GetArenaGameSession.rainCycleTimeInMinutes);
			}
		}
	}

	private void On_HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam) {
		orig(self, cam);

		self.AddPart(new DawnRainMeter(self, self.fContainers[1]));
	}

	private void On_AbstractCreature_InDenUpdate(On.AbstractCreature.orig_InDenUpdate orig, AbstractCreature self, int time) {
		if ((self.world.rainCycle as DawnRainCycle) == null || !(self.world.rainCycle as DawnRainCycle).inRoomWithDawn) {
			orig(self, time);
			return;
		}

		if (self.stuckObjects.Count > 0) {
			orig(self, time);
			return;
		}

		if (self.remainInDenCounter == -1 && self.InDen) {
			if (!self.ignoreCycle) {
				if (self.nightCreature) {
					if (self.world.rainCycle.dayNightCounter > 0 && self.world.rainCycle.dayNightCounter < 1320f * (2.92f + (self.world.rainCycle as DawnRainCycle).GetNightLengthRatio())) {
						self.remainInDenCounter = Random.Range(100, 400);
						// Plugin.Log("Set NIGHT to go out");
						self.Room.MoveEntityOutOfDen(self);
					}
				}
				else {
					if (self.world.rainCycle.TimeUntilRain > (self.world.game.IsStorySession ? 60 : 15) * 40) {
						self.remainInDenCounter = Random.Range(100, 400);
						// Plugin.Log("Set DAY to go out");
						self.Room.MoveEntityOutOfDen(self);
					}
				}
			}
		}

		bool flag = !self.preCycle || (self.preCycle && (!ModManager.MSC || self.world.rainCycle.maxPreTimer > 0));
		if (self.remainInDenCounter > -1 && (!self.nightCreature || (self.nightCreature && self.world.rainCycle.dayNightCounter > 600f && self.world.rainCycle.dayNightCounter < 1320f * (3.92f + (self.world.rainCycle as DawnRainCycle).GetNightLengthRatio())) || self.ignoreCycle) && flag) {
			if (self.WantToStayInDenUntilEndOfCycle()) {
				self.remainInDenCounter = -1;
				return;
			}

			if (!self.Room.world.game.IsArenaSession || self.Room.world.game.GetArenaGameSession.IsCreatureAllowedToEmergeFromDen(self)) {
				if (ModManager.MSC) {
					if (!self.Room.isBattleArena)
						self.remainInDenCounter -= time;

					if (self.DrainWorldDenFlooded() && self.remainInDenCounter < 0)
						self.remainInDenCounter = UnityEngine.Random.Range(100, 400);

					if (self.Room.battleArenaTriggeredTime > 0) {
						self.remainInDenCounter = -1; // Force in den
					}
				}
				else {
					self.remainInDenCounter -= time;
				}

				if (self.remainInDenCounter < 0) {
					self.remainInDenCounter = -1;
					self.Room.MoveEntityOutOfDen(self);
				}
			}
		}
	}

	private float On_RainTracker_Utility(On.RainTracker.orig_Utility orig, RainTracker self) {
		if (self.rainCycle is not DawnRainCycle cycle || !cycle.inRoomWithDawn)
			return orig(self);

		if (self.AI.creature != null && self.AI.creature.nightCreature) {
			if (cycle.dayNightCounter <= 0 || cycle.dayNightCounter >= 1320f * (2.92f + cycle.GetNightLengthRatio()))
				return 1.0f;

			return 0.0f;
		}

		return orig(self);
	}

	private void On_RainCycle_Update(On.RainCycle.orig_Update orig, RainCycle self) {
		orig(self);

		if ((self as DawnRainCycle) != null && (self as DawnRainCycle).inRoomWithDawn) {
			if (self.dayNightCounter >= 1320f * (3.92f + (self as DawnRainCycle).GetNightLengthRatio())) {
				(self as DawnRainCycle).NextCycle();
			}
		}
	}

	private RoomSettingsPage.DevEffectsCategories On_DevInterface_RoomSettingsPage_DevEffectGetCategoryFromEffectType(On.DevInterface.RoomSettingsPage.orig_DevEffectGetCategoryFromEffectType orig, RoomSettingsPage self, RoomSettings.RoomEffect.Type type) {
		if (type == DawnEnums.Dawn) {
			return RoomSettingsPage.DevEffectsCategories.Gameplay;
		}

		return orig(self, type);
	}

	private static Texture2D fadeTexC;

	private void On_RoomCamera_ApplyPalette(On.RoomCamera.orig_ApplyPalette orig, RoomCamera self) {
		this.currentRoom = self.room;
		this.currentCameraPosition = self.currentCameraPosition;

		orig(self);
	}

	private void BlendPalettesFrom(Texture2D texA, Texture2D texB, Texture2D texC, Vector4 fadeCoord) {
		for (int i = 0; i < 32; i++) {
			for (int j = 8; j < 16; j++) {
				texC.SetPixel(i, j - 8, Color.Lerp(
					Color.Lerp(texA.GetPixel(i, j), texA.GetPixel(i, j - 8), fadeCoord.y),
					Color.Lerp(texB.GetPixel(i, j), texB.GetPixel(i, j - 8), fadeCoord.y),
					fadeCoord.x
				));
			}
		}
	}

	private void BlendPalettes(Texture2D texA, Texture2D texB, Texture2D texC, float blend) {
		for (int i = 0; i < 32; i++) {
			for (int j = 0; j < 8; j++) {
				texC.SetPixel(i, j, Color.Lerp(
					texA.GetPixel(i, j),
					texB.GetPixel(i, j),
					blend
				));
			}
		}
	}

	private void ApplyFade(RoomCamera self, Time timeA, Time timeB, float tA, float tB) {
		fadeTexC ??= new Texture2D(32, 8, TextureFormat.ARGB32, false) {
			anisoLevel = 0,
			filterMode = FilterMode.Point,
			wrapMode = TextureWrapMode.Clamp
		};

		float t = Mathf.InverseLerp(tA, tB, self.room.world.rainCycle.dayNightCounter);

		this.timeLerpA = timeA;
		this.timeLerpB = timeB;
		this.lerpAmount = t;

		DawnRoomSettings roomSettings = self.room.roomSettings as DawnRoomSettings;

		RoomSettings settingsA = roomSettings.GetTimeSetting(timeA);
		RoomSettings settingsB = roomSettings.GetTimeSetting(timeB);

		self.LoadPalette(settingsA.Palette, ref self.fadeTexA);
		self.LoadPalette(settingsA.fadePalette?.palette ?? settingsA.Palette, ref self.fadeTexB);
		this.BlendPalettesFrom(self.fadeTexA, self.fadeTexB, self.paletteTexture, self.fadeCoord with { x = settingsA.fadePalette?.fades[self.currentCameraPosition] ?? 0f });

		self.LoadPalette(settingsB.Palette, ref self.fadeTexA);
		self.LoadPalette(settingsB.fadePalette?.palette ?? settingsB.Palette, ref self.fadeTexB);
		this.BlendPalettesFrom(self.fadeTexA, self.fadeTexB, fadeTexC, self.fadeCoord with { x = settingsB.fadePalette?.fades[self.currentCameraPosition] ?? 0f });

		this.BlendPalettes(self.paletteTexture, fadeTexC, self.paletteTexture, t);
		self.paletteTexture.Apply(updateMipmaps: false);
		self.ApplyPalette();
	}

	public void On_RoomCamera_UpdateDayNightPalette(On.RoomCamera.orig_UpdateDayNightPalette orig, RoomCamera self) {
		float effect_dawn = self.room.roomSettings.GetEffectAmount(DawnEnums.Dawn);
		DawnRainCycle dawnRainCycle = self.room.world.rainCycle as DawnRainCycle;

		dawnRainCycle?.inRoomWithDawn = effect_dawn > 0.0f;

		if (effect_dawn > 0f && self.room.world.rainCycle.timer >= self.room.world.rainCycle.cycleLength) {
			// TODO: Effect colors
			// TODO: Check terrain palettes
			float num = 1320f;
			float num2 = 1.47f;
			float num3 = 1.92f;
			float num4 = 2.00f + dawnRainCycle.GetNightLengthRatio();
			float num5 = 2.45f + dawnRainCycle.GetNightLengthRatio();
			float num6 = 2.92f + dawnRainCycle.GetNightLengthRatio();
			float num7 = 3.92f + dawnRainCycle.GetNightLengthRatio();

			if (self.room.world.rainCycle.dayNightCounter <= num) {             // $ MILESTONE 1 - Normal Palette -> Fade Palette
				this.ApplyFade(self, Time.Day, Time.HalfDusk, 0f, num);
			}
			else if (self.room.world.rainCycle.dayNightCounter <= num * num2) { // $ MILESTONE 2 - Fade Palette -> Dusk Palette
				this.ApplyFade(self, Time.HalfDusk, Time.Dusk, num, num * num2);
			}
			else if (self.room.world.rainCycle.dayNightCounter <= num * num3) { // $ MILESTONE 3 - Dusk Palette -> Night Palette
				this.ApplyFade(self, Time.Dusk, Time.Night, num * num2, num * num3);
			}
			else if (self.room.world.rainCycle.dayNightCounter <= num * num4) { // $ MILESTONE 4 - Night Palette
				this.ApplyFade(self, Time.Night, Time.Night, num * num3, num * num4);
			}
			else if (self.room.world.rainCycle.dayNightCounter <= num * num5) { // $ MILESTONE 5 - Night Palette -> Dawn Palette
				this.ApplyFade(self, Time.Night, Time.Dawn, num * num4, num * num5);
			}
			else if (self.room.world.rainCycle.dayNightCounter <= num * num6) { // $ MILESTONE 6 - Dawn Palette -> Fade Palette
				this.ApplyFade(self, Time.Dawn, Time.HalfDawn, num * num5, num * num6);
			}
			else if (self.room.world.rainCycle.dayNightCounter <= num * num7) { // $ MILESTONE 7 - Fade Palette -> Normal Palette
				this.ApplyFade(self, Time.HalfDawn, Time.Day, num * num6, num * num7);
			}
		}
		else {
			orig(self);
		}

		self.dayNightNeedsRefresh = false;

		self.ApplyEffectColorsToAllPaletteTextures(self.room.roomSettings.EffectColorA, self.room.roomSettings.EffectColorB);
		TimeController.ApplyRoomSettings(self.room);
	}
}