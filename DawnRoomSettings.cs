using System.Collections.Generic;

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
				RoomSettings timeSetting = new RoomSettings(null, "roottemplate", null, false, false, null, null);
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