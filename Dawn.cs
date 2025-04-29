using BepInEx;
using UnityEngine;
using RWCustom;
using DevInterface;
using System.Security.Permissions;
using MoreSlugcats;
using Microsoft.Win32;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace Dawn {

	[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
	public class Dawn : BaseUnityPlugin {
		public const string PLUGIN_GUID = "goofybox.dawn";
		public const string PLUGIN_NAME = "Dawn";
		public const string PLUGIN_VERSION = "1.0.1";

		public static string MOD_ID = "dawn";

		private int timeSinceLog = 0;
		private const bool DO_LOGS = false;

		public float fadeBlendDay = 0f;
		public float fadeBlendHalfDusk = 0f;
		public float fadeBlendDusk = 0f;
		public float fadeBlendNight = 0f;
		public float fadeBlendDawn = 0f;
		public float fadeBlendHalfDawn = 0f;
		
		private Color effectColorHalfDuskA = Color.black;
		private Color effectColorDuskA = Color.black;
		private Color effectColorDarkA = Color.black;
		private Color effectColorDawnA = Color.black;
		private Color effectColorHalfDawnA = Color.black;

		private Color effectColorHalfDuskB = Color.black;
		private Color effectColorDuskB = Color.black;
		private Color effectColorDarkB = Color.black;
		private Color effectColorDawnB = Color.black;
		private Color effectColorHalfDawnB = Color.black;
		
		public Time timeLerpA = Time.NONE;
		public Time timeLerpB = Time.NONE;
		public float lerpAmount = 0.0f;
		public Room currentRoom = null;
		public int currentCameraPosition = 0;
		
		public readonly DawnDevTools dawnDev = new DawnDevTools();
		
		public static Dawn instance = null;

		public void Log(object data) {
			Logger.LogInfo(data);
			Debug.Log(data);
		}

		public void OnEnable() {
			instance = this;
			
			Log("Hello world!");

			DawnEnums.Initialize();
			dawnDev.Initialize();
			TerrainController.Initialize();

			On.RoomCamera.UpdateDayNightPalette += On_RoomCamera_UpdateDayNightPalette;

			On.PlacedObject.GenerateEmptyData += On_PlacedObject_GenerateEmptyData;

			On.RainCycle.Update += On_RainCycle_Update;

			On.DevInterface.ObjectsPage.CreateObjRep += On_DevInterface_ObjectsPage_CreateObjRep;
			On.DevInterface.ObjectsPage.DevObjectGetCategoryFromPlacedType += On_DevInterface_ObjectsPage_DevObjectGetCategoryFromPlacedType;

			On.DevInterface.RoomSettingsPage.DevEffectGetCategoryFromEffectType += On_DevInterface_RoomSettingsPage_DevEffectGetCategoryFromEffectType;

			On.RainTracker.Utility += On_RainTracker_Utility;

			On.AbstractCreature.InDenUpdate += On_AbstractCreature_InDenUpdate;

			On.HUD.HUD.InitSinglePlayerHud += On_HUD_InitSinglePlayerHud;

			On.World.ctor += On_World_ctor;
			
			On.RoomCamera.ApplyPalette += On_RoomCamera_ApplyPalette;
			On.RoomCamera.ModifyEffectColorA += On_RoomCamera_ModifyEffectColorA;
			On.RoomCamera.ModifyEffectColorB += On_RoomCamera_ModifyEffectColorB;
			
			On.Player.ProcessDebugInputs += On_Player_ProcessDebugInputs;
			
			On.Room.ctor += On_Room_ctor;
		}

		public void OnDisable() {
			DawnEnums.Cleanup();
			dawnDev.Cleanup();
			TerrainController.Cleanup();
		}
		


		private void On_RoomCamera_ApplyPalette(On.RoomCamera.orig_ApplyPalette orig, RoomCamera self) {
			currentRoom = self.room;
			currentCameraPosition = self.currentCameraPosition;
			orig(self);
		}

		private void On_Room_ctor(On.Room.orig_ctor orig, Room self, RainWorldGame game, World world, AbstractRoom abstractRoom, bool devUI) {
			orig(self, game, world, abstractRoom, devUI);
			
			self.roomSettings = new DawnRoomSettings(self.roomSettings);
		}

		private void On_Player_ProcessDebugInputs(On.Player.orig_ProcessDebugInputs orig, Player self) {
			if (self.room == null || !self.room.game.devToolsActive) return;

			orig(self);
			
			if (Input.GetKeyDown("d")) {
				DawnRainCycle rainCycle = self.room.world.rainCycle as DawnRainCycle;

				if (rainCycle == null) return;
				
				rainCycle.dayNightCounter += 4000;
			}
		}
		
		private Color LerpColor(Color orig, Color halfDusk, Color dusk, Color dark, Color dawn, Color halfDawn) {
			return Color.Lerp(Color.Lerp(Color.Lerp(Color.Lerp(Color.Lerp(
				orig,
				halfDusk, fadeBlendHalfDusk),
				dusk, fadeBlendDusk),
				dark, fadeBlendNight),
				dawn, fadeBlendDawn),
				halfDawn, fadeBlendHalfDawn
			);
		}
		
		private Color ModifyColor(Color orig, int index) {
			if (index == 0) {
				return LerpColor(orig, effectColorHalfDuskA, effectColorDuskA, effectColorDarkA, effectColorDawnA, effectColorHalfDawnA);
			} else if (index == 1) {
				return LerpColor(orig, effectColorHalfDuskB, effectColorDuskB, effectColorDarkB, effectColorDawnB, effectColorHalfDawnB);
			}
			
			return orig;
		}

		private Color[] On_RoomCamera_ModifyEffectColorA(On.RoomCamera.orig_ModifyEffectColorA orig, RoomCamera self, Color[] colors) {
			Color[] newColors = orig(self, colors);
			
			for (int i = 0; i < newColors.Length; i++) {
				newColors[i] = ModifyColor(newColors[i], 0);
			}
			
			return newColors;
		}

		private Color[] On_RoomCamera_ModifyEffectColorB(On.RoomCamera.orig_ModifyEffectColorB orig, RoomCamera self, Color[] colors) {
			Color[] newColors = orig(self, colors);
			
			for (int i = 0; i < newColors.Length; i++) {
				newColors[i] = ModifyColor(newColors[i], 1);
			}
			
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
					} else {
						minutes = Mathf.Lerp(game.rainWorld.setup.cycleTimeMin, game.rainWorld.setup.cycleTimeMax, Random.value) / 60f;
					}

					if (ModManager.MMF && MMF.cfgNoRandomCycles.Value) minutes = game.rainWorld.setup.cycleTimeMax / 60f;

					self.rainCycle = new DawnRainCycle(self, minutes);

					if (ModManager.TimelineModule && name == "SB")
						self.rainCycle.filtrationPowerBehavior = new FiltrationPowerController(self);
				} else {
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
							// Debug.Log("Set NIGHT to go out");
							self.Room.MoveEntityOutOfDen(self);
						}
					} else {
						if (self.world.rainCycle.TimeUntilRain > (self.world.game.IsStorySession ? 60 : 15) * 40) {
							self.remainInDenCounter = Random.Range(100, 400);
							// Debug.Log("Set DAY to go out");
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
						if (!self.Room.isBattleArena) self.remainInDenCounter -= time;

						if (self.DrainWorldDenFlooded() && self.remainInDenCounter < 0) self.remainInDenCounter = UnityEngine.Random.Range(100, 400);

						if (self.Room.battleArenaTriggeredTime > 0) {
							self.remainInDenCounter = -1; // Force in den
						}
					} else {
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
			if ((self.rainCycle as DawnRainCycle) == null || !(self.rainCycle as DawnRainCycle).inRoomWithDawn) return orig(self);

			if (self.AI.creature != null && self.AI.creature.nightCreature) {
				if (self.rainCycle.dayNightCounter <= 0 || self.rainCycle.dayNightCounter >= 1320f * (2.92f + (self.rainCycle as DawnRainCycle).GetNightLengthRatio())) return 1.0f;

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
			if (type == DawnEnums.DawnEffect) {
				return RoomSettingsPage.DevEffectsCategories.Decorations;
			}

			return orig(self, type);
		}

		private ObjectsPage.DevObjectCategories On_DevInterface_ObjectsPage_DevObjectGetCategoryFromPlacedType(On.DevInterface.ObjectsPage.orig_DevObjectGetCategoryFromPlacedType orig, DevInterface.ObjectsPage self, PlacedObject.Type type) {
			if (type == DawnEnums.DawnObject) {
				return ObjectsPage.DevObjectCategories.Gameplay;
			}
			
			if (
				type == DawnEnums.HalfDuskEffectColours ||
				type == DawnEnums.DuskEffectColours ||
				type == DawnEnums.NightEffectColours ||
				type == DawnEnums.DawnEffectColours ||
				type == DawnEnums.HalfDawnEffectColours
			) {
				return ObjectsPage.DevObjectCategories.Decoration;
			}

			return orig(self, type);
		}

		private void On_DevInterface_ObjectsPage_CreateObjRep(On.DevInterface.ObjectsPage.orig_CreateObjRep orig, DevInterface.ObjectsPage self, PlacedObject.Type tp, PlacedObject pObj) {
			if (pObj == null) {
				pObj = new PlacedObject(tp, null);
				pObj.pos = self.owner.room.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new Vector2(-683f, 384f), 0.25f) + Custom.DegToVec(UnityEngine.Random.value * 360f) * 0.2f;
				self.RoomSettings.placedObjects.Add(pObj);
				if (tp == PlacedObject.Type.LightFixture) {
					(pObj.data as PlacedObject.LightFixtureData).type = self.lastPlacedLightFixture;
				}
			}
			
			PlacedObjectRepresentation placedObjectRepresentation = null;

			if (tp == DawnEnums.DawnObject) {
				placedObjectRepresentation = new DawnObjectRepresentation(self.owner, tp.ToString() + "_Rep", self, pObj, tp.ToString());
			} else if (
				tp == DawnEnums.HalfDuskEffectColours ||
				tp == DawnEnums.DuskEffectColours ||
				tp == DawnEnums.NightEffectColours ||
				tp == DawnEnums.DawnEffectColours ||
				tp == DawnEnums.HalfDawnEffectColours
			) {
				placedObjectRepresentation = new CustomEffectColours(self.owner, tp.ToString() + "_Rep", self, pObj, tp.ToString());
			}

			if (placedObjectRepresentation != null) {
				self.tempNodes.Add(placedObjectRepresentation);
				self.subNodes.Add(placedObjectRepresentation);
			} else {
				orig(self, tp, pObj);
			}
		}

		public void On_PlacedObject_GenerateEmptyData(On.PlacedObject.orig_GenerateEmptyData orig, PlacedObject self) {
			if (self.type == DawnEnums.DawnObject) {
				self.data = new DawnObjectData(self);
				return;
			}
			
			if (
				self.type == DawnEnums.HalfDuskEffectColours ||
				self.type == DawnEnums.DuskEffectColours ||
				self.type == DawnEnums.NightEffectColours ||
				self.type == DawnEnums.DawnEffectColours ||
				self.type == DawnEnums.HalfDawnEffectColours
			) {
				self.data = new CustomEffectColoursData(self);
				return;
			}

			orig(self);
		}
		
		public void On_RoomCamera_UpdateDayNightPalette(On.RoomCamera.orig_UpdateDayNightPalette orig, RoomCamera self) {
			fadeBlendDay = 1.0f;
			fadeBlendHalfDusk = 0.0f;
			fadeBlendDusk = 0.0f;
			fadeBlendNight = 0.0f;
			fadeBlendDawn = 0.0f;
			fadeBlendHalfDawn = 0.0f;

			timeLerpA = Time.Day;
			timeLerpB = Time.HalfDusk;
			lerpAmount = 0.0f;

			float effect_dawn = self.room.roomSettings.GetEffectAmount(DawnEnums.DawnEffect) * 0.99f;
			DawnRainCycle dawnRainCycle = self.room.world.rainCycle as DawnRainCycle;

			if (dawnRainCycle != null) {
				dawnRainCycle.inRoomWithDawn = effect_dawn > 0.0f;
			}

			if (effect_dawn > 0f && self.room.world.rainCycle.timer >= self.room.world.rainCycle.cycleLength) {
				float num = 1320f;
				float num2 = 1.47f;
				float num3 = 1.92f;
				float num4 = 2.00f + dawnRainCycle.GetNightLengthRatio();
				float num5 = 2.45f + dawnRainCycle.GetNightLengthRatio();
				float num6 = 2.92f + dawnRainCycle.GetNightLengthRatio();
				
				int normalPalette = self.room.roomSettings.Palette;
				int fadePalette = normalPalette;
				int dawnPalette = -1;

				if (self.room.roomSettings.fadePalette?.palette > -1) {
					fadePalette = self.room.roomSettings.fadePalette?.palette ?? normalPalette;
				}

				foreach (PlacedObject placedObject in self.room.roomSettings.placedObjects) {
					if (placedObject.type == DawnEnums.DawnObject) {
						dawnPalette = (placedObject.data as DawnObjectData).dawnPalette;
					} else if (placedObject.type == DawnEnums.HalfDuskEffectColours) {
						effectColorHalfDuskA = (placedObject.data as CustomEffectColoursData).colourA;
						effectColorHalfDuskB = (placedObject.data as CustomEffectColoursData).colourB;
					} else if (placedObject.type == DawnEnums.DuskEffectColours) {
						effectColorDuskA = (placedObject.data as CustomEffectColoursData).colourA;
						effectColorDuskB = (placedObject.data as CustomEffectColoursData).colourB;
					} else if (placedObject.type == DawnEnums.NightEffectColours) {
						effectColorDarkA = (placedObject.data as CustomEffectColoursData).colourA;
						effectColorDarkB = (placedObject.data as CustomEffectColoursData).colourB;
					} else if (placedObject.type == DawnEnums.DawnEffectColours) {
						effectColorDawnA = (placedObject.data as CustomEffectColoursData).colourA;
						effectColorDawnB = (placedObject.data as CustomEffectColoursData).colourB;
					} else if (placedObject.type == DawnEnums.HalfDawnEffectColours) {
						effectColorHalfDawnA = (placedObject.data as CustomEffectColoursData).colourA;
						effectColorHalfDawnB = (placedObject.data as CustomEffectColoursData).colourB;
					}
				}
				
				if (dawnPalette == -1) {
					dawnPalette = 0;
				}

				fadeBlendDay = 0.0f;

				if (self.room.world.rainCycle.dayNightCounter < num) { // Normal -> Fade Palette       NOTE: MILESTONE 1
					if (self.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.AboveCloudsView) > 0f && self.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SkyAndLightBloom) > 0f) {
						self.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.SkyAndLightBloom).amount = 0f;
					}

					float a = 0.0f;
					if (self.room.roomSettings.fadePalette != null && self.room.roomSettings.fadePalette.fades.Length > self.currentCameraPosition) {
						a = self.room.roomSettings.fadePalette.fades[self.currentCameraPosition];
					}

					lerpAmount = self.room.world.rainCycle.dayNightCounter / num;
					timeLerpA = Time.Day;
					timeLerpB = Time.HalfDusk;

					self.paletteBlend = Mathf.Lerp(a, 1f, lerpAmount);
					self.ApplyFade();
					
					fadeBlendDay = Mathf.Lerp(1.0f, 0.0f, lerpAmount);
					fadeBlendHalfDusk = Mathf.Lerp(0.0f, 1.0f, lerpAmount);

				} else if (self.room.world.rainCycle.dayNightCounter == num) { // Fade Palette
					self.ChangeBothPalettes(self.paletteB, self.room.world.rainCycle.duskPalette, 0f);
					
					fadeBlendHalfDusk = 1.0f;
					lerpAmount = 1.0f;
					timeLerpB = Time.HalfDusk;

				} else if (self.room.world.rainCycle.dayNightCounter < num * num2) { // Fade Palette -> Dusk Palette NOTE: MILESTONE 2
					if (self.paletteBlend == 1f || self.paletteB != self.room.world.rainCycle.duskPalette || self.dayNightNeedsRefresh) {
						self.ChangeBothPalettes(self.paletteB, self.room.world.rainCycle.duskPalette, 0f);
					}
					
					lerpAmount = (self.room.world.rainCycle.dayNightCounter - num) / (num * (num2 - 1.0f));
					timeLerpA = Time.HalfDusk;
					timeLerpB = Time.Dusk;
					
					self.paletteBlend = Mathf.InverseLerp(num, num * num2, self.room.world.rainCycle.dayNightCounter);
					self.ApplyFade();

					fadeBlendHalfDusk = Mathf.Lerp(1.0f, 0.0f, lerpAmount);
					fadeBlendDusk = Mathf.Lerp(0.0f, 1.0f, lerpAmount);

				} else if (self.room.world.rainCycle.dayNightCounter == num * num2) { // Dusk Palette
					self.ChangeBothPalettes(self.room.world.rainCycle.duskPalette, self.room.world.rainCycle.nightPalette, 0f);
					
					fadeBlendDusk = 1.0f;

					timeLerpB = Time.Dusk;
					lerpAmount = 1.0f;

				} else if (self.room.world.rainCycle.dayNightCounter < num * num3) { // Dusk Palette -> Evening Palette         NOTE: MILESTONE 3
					if (self.paletteBlend == 1f || self.paletteB != self.room.world.rainCycle.nightPalette || self.paletteA != self.room.world.rainCycle.duskPalette || self.dayNightNeedsRefresh) {
						self.ChangeBothPalettes(self.room.world.rainCycle.duskPalette, self.room.world.rainCycle.nightPalette, 0f);
					}
					
					lerpAmount = (self.room.world.rainCycle.dayNightCounter - num * num2) / (num * (num3 - num2));
					timeLerpA = Time.Dusk;
					timeLerpB = Time.Night;

					self.paletteBlend = Mathf.InverseLerp(num * num2, num * num3, self.room.world.rainCycle.dayNightCounter) * (self.effect_dayNight * 0.99f);
					self.ApplyFade();
					
					fadeBlendDusk = Mathf.Lerp(1.0f, 0.0f, lerpAmount);
					fadeBlendNight = Mathf.Lerp(0.0f, 1.0f, lerpAmount);

				} else if (self.room.world.rainCycle.dayNightCounter == num * num3) { // Evening Palette
					self.ChangeBothPalettes(self.room.world.rainCycle.duskPalette, self.room.world.rainCycle.nightPalette, self.effect_dayNight * 0.99f);
					
					lerpAmount = 1.0f;
					timeLerpB = Time.Night;
					fadeBlendNight = 1.0f;

				} else if (self.room.world.rainCycle.dayNightCounter < num * num4) { // Evening Palette -> Night Palette       NOTE: MILESTONE 4
					if (self.paletteBlend == 1f || self.paletteB != self.room.world.rainCycle.nightPalette || self.paletteA != self.room.world.rainCycle.duskPalette || self.dayNightNeedsRefresh) {
						self.ChangeBothPalettes(self.room.world.rainCycle.duskPalette, self.room.world.rainCycle.nightPalette, self.effect_dayNight);
					}

					lerpAmount = 1.0f;
					timeLerpB = Time.Night;

					self.paletteBlend = 1.0f - (Mathf.InverseLerp(num * num4, num * num3, self.room.world.rainCycle.dayNightCounter) * (1.0f - self.effect_dayNight * 0.99f));
					self.ApplyFade();

					fadeBlendNight = 1.0f;

				} else if (self.room.world.rainCycle.dayNightCounter == num * num4) { // Night Palette
					self.ChangeBothPalettes(self.room.world.rainCycle.nightPalette, dawnPalette, 0.0f);
					
					lerpAmount = 1.0f;
					timeLerpB = Time.Night;

					fadeBlendNight = 1.0f;

				} else if (self.room.world.rainCycle.dayNightCounter < num * num5) { // Night Palette -> Dawn Palette         NOTE: MILESTONE 5
					if (self.paletteBlend == 1f || self.paletteA != self.room.world.rainCycle.nightPalette || self.paletteB != dawnPalette || self.dayNightNeedsRefresh) {
						self.ChangeBothPalettes(self.room.world.rainCycle.nightPalette, dawnPalette, 0.0f);
					}

					lerpAmount = (self.room.world.rainCycle.dayNightCounter - num * num4) / (num * (num5 - num4));
					timeLerpA = Time.Night;
					timeLerpB = Time.Dawn;

					self.paletteBlend = Mathf.InverseLerp(num * num4, num * num5, self.room.world.rainCycle.dayNightCounter);
					self.ApplyFade();

					fadeBlendNight = Mathf.Lerp(1.0f, 0.0f, lerpAmount);
					fadeBlendDawn = Mathf.Lerp(0.0f, 1.0f, lerpAmount);

				} else if (self.room.world.rainCycle.dayNightCounter == num * num5) { // Dawn Palette
					self.ChangeBothPalettes(dawnPalette, dawnPalette, 0.0f);
					
					lerpAmount = 1.0f;
					timeLerpB = Time.Dawn;
					
					fadeBlendDawn = 1.0f;

				} else if (self.room.world.rainCycle.dayNightCounter < num * num6) { // Dawn Palette -> Fade Palette              NOTE: MILESTONE 6
					if (self.paletteBlend == 1f || self.paletteA != dawnPalette || self.dayNightNeedsRefresh) {
						self.ChangeBothPalettes(dawnPalette, fadePalette, 0.0f);
					}

					lerpAmount = (self.room.world.rainCycle.dayNightCounter - num * num5) / (num * (num6 - num5));
					timeLerpA = Time.Dawn;
					timeLerpB = Time.HalfDawn;

					self.paletteBlend = Mathf.InverseLerp(num * num5, num * num6, self.room.world.rainCycle.dayNightCounter);
					self.ApplyFade();
					
					fadeBlendDawn = Mathf.Lerp(1.0f, 0.0f, lerpAmount);
					fadeBlendHalfDawn = Mathf.Lerp(0.0f, 1.0f, lerpAmount);

				} else if (self.room.world.rainCycle.dayNightCounter == num * num6) { // Fade Palette
					self.ChangeBothPalettes(normalPalette, fadePalette, 1.0f);
					
					lerpAmount = 1.0f;
					timeLerpB = Time.HalfDawn;
					
					fadeBlendDawn = 0.0f;
					fadeBlendHalfDawn = 1.0f;

				} else if (self.room.world.rainCycle.dayNightCounter >= num * num6) { // Fade Palette -> Normal               NOTE: MILESTONE 7
					if (self.paletteBlend == 0f || self.paletteA != normalPalette || self.dayNightNeedsRefresh) {
						self.ChangeBothPalettes(normalPalette, fadePalette, 1.0f);
					}

					lerpAmount = (self.room.world.rainCycle.dayNightCounter - num * num6) / num;
					timeLerpA = Time.HalfDawn;
					timeLerpB = Time.Day;

					float a = 0.0f;
					if (self.room.roomSettings.fadePalette != null && self.room.roomSettings.fadePalette.fades.Length > self.currentCameraPosition) {
						a = self.room.roomSettings.fadePalette.fades[self.currentCameraPosition];
					}

					self.paletteBlend = Mathf.Lerp(1f, a, lerpAmount);
					self.ApplyFade();
					
					fadeBlendHalfDawn = Mathf.Lerp(1.0f, 0.0f, lerpAmount);
					fadeBlendDay = Mathf.Lerp(0.0f, 1.0f, lerpAmount);
				}


				timeSinceLog++;
				if (timeSinceLog >= 15 && DO_LOGS) {
					timeSinceLog = 0;
					Debug.Log(self.room.world.rainCycle.dayNightCounter);
					Debug.Log("MILESTONE 1: " + (num * 1.0f) + "    " + (self.room.world.rainCycle.dayNightCounter >= (num * 1.0f) ? "Passed" : ""));
					Debug.Log("MILESTONE 2: " + (num * num2) + "    " + (self.room.world.rainCycle.dayNightCounter >= (num * num2) ? "Passed" : ""));
					Debug.Log("MILESTONE 3: " + (num * num3) + "    " + (self.room.world.rainCycle.dayNightCounter >= (num * num3) ? "Passed" : ""));
					Debug.Log("MILESTONE 4: " + (num * num4) + "    " + (self.room.world.rainCycle.dayNightCounter >= (num * num4) ? "Passed" : ""));
					Debug.Log("MILESTONE 5: " + (num * num5) + "    " + (self.room.world.rainCycle.dayNightCounter >= (num * num5) ? "Passed" : ""));
					Debug.Log("MILESTONE 6: " + (num * num6) + "    " + (self.room.world.rainCycle.dayNightCounter >= (num * num6) ? "Passed" : ""));
					Debug.Log("MILESTONE 7: " + (num * num6 + num) + "    " + (self.room.world.rainCycle.dayNightCounter >= (num * num6 + num) ? "Passed" : ""));
					// Debug.Log("Normal Palette: " + normalPalette);
					// Debug.Log("Fade   Palette: " + fadePalette);
					// Debug.Log("Dusk   Palette: " + self.room.world.rainCycle.duskPalette);
					// Debug.Log("Night  Palette: " + self.room.world.rainCycle.nightPalette);
					// Debug.Log("Dawn   Palette: " + dawnPalette);
					Debug.Log("== Lerp A: " + timeLerpA);
					Debug.Log("== Lerp B: " + timeLerpB);
					Debug.Log("== Lerp T: " + lerpAmount);
					Debug.Log("~~ Current Fade: " + self.paletteBlend);
					Debug.Log("~~ Timer: " + self.room.world.rainCycle.timer + " / " + self.room.world.rainCycle.sunDownStartTime);
				}
			} else {
				orig(self);
			}
			
			self.dayNightNeedsRefresh = false;
			
			self.ApplyEffectColorsToAllPaletteTextures(self.room.roomSettings.EffectColorA, self.room.roomSettings.EffectColorB);
			TimeController.ApplyRoomSettings(self.room);
		}
	}
}