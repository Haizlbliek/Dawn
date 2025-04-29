using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dawn {
	static class TerrainController {
		static private TerrainPalette.PaletteInfo testSand = null;
		private static readonly Dictionary<string, TerrainPalette.PaletteInfo> paletteCache = [];
		
		static public void Initialize() {
			On.TerrainPalette.UpdateFade += On_TerrainPalette_UpdateFade;

			On.DevInterface.TerrainPanel.ctor += On_TerrainPanel_ctor;
			On.DevInterface.TerrainPanel.Refresh += On_TerrainPanel_Refresh;
			On.DevInterface.TerrainPanel.Signal += On_TerrainPanel_Signal;
			On.DevInterface.TerrainPanel.FadeSlider.Refresh += On_FadeSlider_Refresh;
			On.DevInterface.TerrainPanel.FadeSlider.NubDragged += On_FadeSlider_NubDragged;
		}

		static public void Cleanup() {
			On.TerrainPalette.UpdateFade -= On_TerrainPalette_UpdateFade;

			On.DevInterface.TerrainPanel.ctor -= On_TerrainPanel_ctor;
			On.DevInterface.TerrainPanel.Refresh -= On_TerrainPanel_Refresh;
			On.DevInterface.TerrainPanel.Signal -= On_TerrainPanel_Signal;
			On.DevInterface.TerrainPanel.FadeSlider.Refresh -= On_FadeSlider_Refresh;
			On.DevInterface.TerrainPanel.FadeSlider.NubDragged -= On_FadeSlider_NubDragged;
		}

		private static void On_TerrainPanel_ctor(On.DevInterface.TerrainPanel.orig_ctor orig, DevInterface.TerrainPanel self, DevInterface.DevUI owner, string id, DevInterface.DevUINode parentNode, Vector2 pos) {
			RoomSettings backup = owner.room.roomSettings;
			
			if (backup is DawnRoomSettings settings) owner.room.roomSettings = settings.GetTimeSetting(DawnDevTools.currentTime);
			orig(self, owner, id, parentNode, pos);
			
			owner.room.roomSettings = backup;
		}

		private static void On_FadeSlider_NubDragged(On.DevInterface.TerrainPanel.FadeSlider.orig_NubDragged orig, DevInterface.TerrainPanel.FadeSlider self, float nubPos) {
			RoomSettings backup = self.owner.room.roomSettings;
			
			if (backup is DawnRoomSettings settings) self.owner.room.roomSettings = settings.GetTimeSetting(DawnDevTools.GetCurrentTime());
			orig(self, nubPos);
			
			self.owner.room.roomSettings = backup;
		}

		private static void On_FadeSlider_Refresh(On.DevInterface.TerrainPanel.FadeSlider.orig_Refresh orig, DevInterface.TerrainPanel.FadeSlider self) {
			RoomSettings backup = self.owner.room.roomSettings;
			
			if (backup is DawnRoomSettings settings) self.owner.room.roomSettings = settings.GetTimeSetting(DawnDevTools.GetCurrentTime());
			orig(self);
			
			self.owner.room.roomSettings = backup;
		}

		private static void On_TerrainPanel_Signal(On.DevInterface.TerrainPanel.orig_Signal orig, DevInterface.TerrainPanel self, DevInterface.DevUISignalType type, DevInterface.DevUINode sender, string message) {
			RoomSettings backup = self.owner.room.roomSettings;
			
			if (backup is DawnRoomSettings settings) self.owner.room.roomSettings = settings.GetTimeSetting(DawnDevTools.GetCurrentTime());
			orig(self, type, sender, message);
			
			self.owner.room.roomSettings = backup;
		}

		private static void On_TerrainPanel_Refresh(On.DevInterface.TerrainPanel.orig_Refresh orig, DevInterface.TerrainPanel self) {
			RoomSettings backup = self.owner.room.roomSettings;
			
			if (backup is DawnRoomSettings settings) self.owner.room.roomSettings = settings.GetTimeSetting(DawnDevTools.GetCurrentTime());
			orig(self);
			
			self.owner.room.roomSettings = backup;
		}
		
		private static void LerpBetween(TerrainPalette.PaletteInfo paletteA, TerrainPalette.PaletteInfo paletteB, Color[] colorsA, Color[] colorsB, float fade, float rain, float echo) {
			paletteA?.GetColors(colorsA, rain, echo);
			if (paletteB != null && fade > 0f) {
				paletteB.GetColors(colorsB, rain, echo);
				TerrainPalette.LerpColors(colorsA, colorsB, fade);
			}
		}
		
		private static TerrainPalette.PaletteInfo GetPalette(string key) {
			try {
				return paletteCache[key];
			} catch (KeyNotFoundException) {
				TerrainPalette.PaletteInfo palette = new TerrainPalette.PaletteInfo(key);
				
				paletteCache[key] = palette;
				
				return palette;
			}
		}
		
		private static void InTexture(in DawnRoomSettings settings, in Time time, out TerrainPalette.PaletteInfo mainPalette, out TerrainPalette.PaletteInfo fadePalette, out float fade) {
			RoomSettings timeSettings = settings.GetTimeSetting(time);
			
			try {
				mainPalette = GetPalette(timeSettings.terrainPalette);
			} catch (Exception) {
				mainPalette = null;
			}
			
			try {
				fadePalette = GetPalette(timeSettings.terrainFadePalette?.palette);
			} catch (Exception) {
				fadePalette = null;
			}
			
			fade = timeSettings.terrainFadePalette?.fades[Dawn.instance.currentCameraPosition] ?? 0.0f;
		}

		private static void On_TerrainPalette_UpdateFade(On.TerrainPalette.orig_UpdateFade orig, TerrainPalette self, float fade, float mushroom, float rain, float echo) {
			if (Dawn.instance.currentRoom.roomSettings is not DawnRoomSettings settings) {
				orig(self, fade, mushroom, rain, echo);
				return;
			}
			
			testSand ??= new TerrainPalette.PaletteInfo("test_sand");
			
			InTexture(settings, Dawn.instance.timeLerpA, out TerrainPalette.PaletteInfo paletteAMain, out TerrainPalette.PaletteInfo paletteAFade, out float fadeA);
			InTexture(settings, Dawn.instance.timeLerpB, out TerrainPalette.PaletteInfo paletteBMain, out TerrainPalette.PaletteInfo paletteBFade, out float fadeB);
			
			if (paletteAMain == null) {
				InTexture(settings, Time.NONE, out paletteAMain, out paletteAFade, out fadeA);
				
				paletteAMain ??= testSand;
			}

			if (paletteBMain == null) {
				InTexture(settings, Time.NONE, out paletteBMain, out paletteBFade, out fadeB);
				
				paletteBMain ??= testSand;
			}

			if (Dawn.instance.lerpAmount <= 0.01f || (paletteAMain?.name == paletteBMain?.name && paletteAFade?.name == paletteBFade?.name)) {
				self.texturePixels = new Color[(paletteAMain?.PaletteSize.x * paletteAMain?.PaletteSize.y) ?? 0];
				self.fadePixels    = new Color[(paletteAFade?.PaletteSize.x * paletteAFade?.PaletteSize.y) ?? 0];

				LerpBetween(paletteAMain, paletteAFade, self.texturePixels, self.fadePixels, fadeA, rain, echo);
			} else if (Dawn.instance.lerpAmount >= 0.99f) {
				self.texturePixels = new Color[(paletteBMain?.PaletteSize.x * paletteBMain?.PaletteSize.y) ?? 0];
				self.fadePixels    = new Color[(paletteBFade?.PaletteSize.x * paletteBFade?.PaletteSize.y) ?? 0];

				LerpBetween(paletteBMain, paletteBFade, self.texturePixels, self.fadePixels, fadeB, rain, echo);
			} else {
				Color[] tempPixels = new Color[(paletteBFade?.PaletteSize.x * paletteBFade?.PaletteSize.y) ?? 0];

				self.texturePixels = new Color[(paletteAMain?.PaletteSize.x * paletteAMain?.PaletteSize.y) ?? 0];
				self.fadePixels    = new Color[(paletteAFade?.PaletteSize.x * paletteAFade?.PaletteSize.y) ?? 0];
				LerpBetween(paletteAMain, paletteAFade, self.texturePixels, self.fadePixels, fadeA, rain, echo);

				self.fadePixels    = new Color[(paletteBMain?.PaletteSize.x * paletteBMain?.PaletteSize.y) ?? 0];
				LerpBetween(paletteBMain, paletteBFade, self.fadePixels, tempPixels, fadeB, rain, echo);
				
				TerrainPalette.LerpColors(self.texturePixels, self.fadePixels, Dawn.instance.lerpAmount);
			}
			
			self.texture.SetPixels(self.texturePixels);
			self.texture.Apply();

			self.GlitterColor = self.texturePixels[0];
			self.LightTint = self.texturePixels[1];
			self.DarkDustColor = self.texturePixels[2];
			self.LightDustColor = self.texturePixels[3];
			self.SandstormColor = new Color?(self.texturePixels[4]);

			if (self.SandstormColor == Color.black) {
				self.SandstormColor = null;
			}
			
			RoomSettings time_ = settings.GetTimeSetting(Time.NONE);
			RoomSettings timeA = settings.GetTimeSetting(Dawn.instance.timeLerpA);
			RoomSettings timeB = settings.GetTimeSetting(Dawn.instance.timeLerpB);
			settings.terrainLight           = Mathf.Lerp(timeA.terrainLight           ?? time_.terrainLight           ?? 0.0f, timeB.terrainLight           ?? time_.terrainLight           ?? 0.0f, Dawn.instance.lerpAmount);
			settings.terrainStainAmount     = Mathf.Lerp(timeA.terrainStainAmount     ?? time_.terrainStainAmount     ?? 0.0f, timeB.terrainStainAmount     ?? time_.terrainStainAmount     ?? 0.0f, Dawn.instance.lerpAmount);
			settings.terrainStainBrightness = Mathf.Lerp(timeA.terrainStainBrightness ?? time_.terrainStainBrightness ?? 0.0f, timeB.terrainStainBrightness ?? time_.terrainStainBrightness ?? 0.0f, Dawn.instance.lerpAmount);
			settings.terrainStainHeight     = Mathf.Lerp(timeA.terrainStainHeight     ?? time_.terrainStainHeight     ?? 0.0f, timeB.terrainStainHeight     ?? time_.terrainStainHeight     ?? 0.0f, Dawn.instance.lerpAmount);
			settings.terrainWaves           = Mathf.Lerp(timeA.terrainWaves           ?? time_.terrainWaves           ?? 0.0f, timeB.terrainWaves           ?? time_.terrainWaves           ?? 0.0f, Dawn.instance.lerpAmount);
			settings.terrainEdgeRadius      = Mathf.Lerp(timeA.terrainEdgeRadius      ?? time_.terrainEdgeRadius      ?? 0.0f, timeB.terrainEdgeRadius      ?? time_.terrainEdgeRadius      ?? 0.0f, Dawn.instance.lerpAmount);
			settings.terrainGooHeight       = Mathf.Lerp(timeA.terrainGooHeight       ?? time_.terrainGooHeight       ?? 0.0f, timeB.terrainGooHeight       ?? time_.terrainGooHeight       ?? 0.0f, Dawn.instance.lerpAmount);
			settings.terrainGrain           = Mathf.Lerp(timeA.terrainGrain           ?? time_.terrainGrain           ?? 0.0f, timeB.terrainGrain           ?? time_.terrainGrain           ?? 0.0f, Dawn.instance.lerpAmount);
			settings.terrainDepth           = Mathf.Lerp(timeA.terrainDepth           ?? time_.terrainDepth           ?? 0.0f, timeB.terrainDepth           ?? time_.terrainDepth           ?? 0.0f, Dawn.instance.lerpAmount);
			settings.terrainSkyFade         = Mathf.Lerp(timeA.terrainSkyFade         ?? time_.terrainSkyFade         ?? 0.0f, timeB.terrainSkyFade         ?? time_.terrainSkyFade         ?? 0.0f, Dawn.instance.lerpAmount);
		}
	}
}