using System.Collections.Generic;
using Microsoft.Win32;

namespace Dawn {
	class DawnRoomSettings : RoomSettings {
		public DawnRoomSettings(RoomSettings settings) : base(settings.room, "roottemplate", null, settings.isTemplate, settings.isFirstTemplate, null, settings.game) {
			this.name = settings.name;
			this.parent = settings.parent;
			this.isAncestor = settings.isAncestor;
			this.filePath = settings.filePath;

			this.effects = settings.effects;
			this.ambientSounds = settings.ambientSounds;
			this.placedObjects = settings.placedObjects;
			this.triggers = settings.triggers;
			this.fadePalette = settings.fadePalette;
			this.terrainFadePalette = settings.terrainFadePalette;

			this.dType = settings.dType;
			this.rInts = settings.rInts;
			this.rumInts = settings.rumInts;
			this.cDrips = settings.cDrips;
			this.wSpeed = settings.wSpeed;
			this.wAmp = settings.wAmp;
			this.wLength = settings.wLength;
			this.swAmp = settings.swAmp;
			this.swLength = settings.swLength;
			this.clds = settings.clds;
			this.grm = settings.grm;
			this.bkgDrnVl = settings.bkgDrnVl;
			this.bkgDrnNoThreatVol = settings.bkgDrnNoThreatVol;
			this.rndItmDns = settings.rndItmDns;
			this.rndItmSprChnc = settings.rndItmSprChnc;
			this.wtrRflctAlpha = settings.wtrRflctAlpha;
			this.pal = settings.pal;
			this.eColA = settings.eColA;
			this.eColB = settings.eColB;
			this.roomSpecificScript = settings.roomSpecificScript;
			this.wetTerrain = settings.wetTerrain;
			this.terrainLight = settings.terrainLight;
			this.terrainStainAmount = settings.terrainStainAmount;
			this.terrainStainBrightness = settings.terrainStainBrightness;
			this.terrainStainHeight = settings.terrainStainHeight;
			this.terrainWaves = settings.terrainWaves;
			this.terrainEdgeRadius = settings.terrainEdgeRadius;
			this.terrainGooHeight = settings.terrainGooHeight;
			this.terrainGrain = settings.terrainGrain;
			this.terrainDepth = settings.terrainDepth;
			this.terrainSkyFade = settings.terrainSkyFade;
			this.terrainPalette = settings.terrainPalette;
			
			this.timeSettings = [];
			foreach (string time in Time.values.entries) {
				RoomSettings timeSetting;
				if (time == "NONE") {
					timeSetting = settings;
				} else {
					timeSetting = new RoomSettings(null, "roottemplate", null, false, false, null, null);
				}
				timeSetting.parent = DefaultRoomSettings.ancestor;
				timeSettings.Add(new Time(time, false), timeSetting);
			}

			if (!this.Load(this.game?.TimelinePoint)) {
				string text2 = WorldLoader.FindRoomFile(name, false, ".txt", true);
				if (text2 != null) {
					this.filePath = text2.Substring(0, text2.Length - 4) + "_settings.txt";
					this.Load((SlugcatStats.Timeline) null);
				}
			}
		}
		
		public void SetAll() {
			List<RoomSettings.RoomEffect.Type> types = [ ];
			List<RoomSettings.RoomEffect.Type> selfHas = [ ];
			
			foreach (KeyValuePair<Time, RoomSettings> entry in timeSettings) {
				if (entry.Key == Time.NONE) {
					foreach (RoomEffect effect in entry.Value.effects) {
						types.Add(effect.type);
						selfHas.Add(effect.type);
					}
				} else {
					foreach (RoomEffect effect in entry.Value.effects) {
						types.Add(effect.type);
					}
				}
			}
			
			foreach (RoomEffect.Type type in types) {
				if (selfHas.Contains(type)) continue;
				
				effects.Add(new RoomEffect(type, 0.0f, false));
			}
		}
		
		public RoomSettings CopyMainTo(RoomSettings settings) {
			// settings.effects = this.effects; NOTE: COMPLETE

			settings.ambientSounds = this.ambientSounds;
			settings.placedObjects = this.placedObjects;
			settings.triggers = this.triggers;
			settings.fadePalette = this.fadePalette;
			settings.terrainFadePalette = this.terrainFadePalette;

			settings.dType = this.dType;
			settings.rInts = this.rInts;
			settings.rumInts = this.rumInts;
			settings.cDrips = this.cDrips;
			settings.wSpeed = this.wSpeed;
			settings.wAmp = this.wAmp;
			settings.wLength = this.wLength;
			settings.swAmp = this.swAmp;
			settings.swLength = this.swLength;
			settings.clds = this.clds;
			settings.grm = this.grm;
			settings.bkgDrnVl = this.bkgDrnVl;
			settings.bkgDrnNoThreatVol = this.bkgDrnNoThreatVol;
			settings.rndItmDns = this.rndItmDns;
			settings.rndItmSprChnc = this.rndItmSprChnc;
			settings.wtrRflctAlpha = this.wtrRflctAlpha;
			settings.pal = this.pal;
			settings.eColA = this.eColA;
			settings.eColB = this.eColB;
			settings.roomSpecificScript = this.roomSpecificScript;
			settings.wetTerrain = this.wetTerrain;
			settings.terrainLight = this.terrainLight;
			settings.terrainStainAmount = this.terrainStainAmount;
			settings.terrainStainBrightness = this.terrainStainBrightness;
			settings.terrainStainHeight = this.terrainStainHeight;
			settings.terrainWaves = this.terrainWaves;
			settings.terrainEdgeRadius = this.terrainEdgeRadius;
			settings.terrainGooHeight = this.terrainGooHeight;
			settings.terrainGrain = this.terrainGrain;
			settings.terrainDepth = this.terrainDepth;
			settings.terrainSkyFade = this.terrainSkyFade;
			settings.terrainPalette = this.terrainPalette;
			
			return settings;
		}
		
		public RoomSettings GetTimeSetting(Time time) {
			try {
				return timeSettings[time];
			} catch (KeyNotFoundException) {
				return null;
			}
		}
		
		public Dictionary<Time, RoomSettings> timeSettings;
	}
}