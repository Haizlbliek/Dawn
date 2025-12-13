using UnityEngine;

namespace Dawn {
	class DawnRainCycle : RainCycle {
		public bool inRoomWithDawn = false;

		public int nextCycleLength = -1;

		private float nightLength;

		public DawnRainCycle(World world, float minutes) : base(world, minutes) {
			this.RandomLength();
		}

		public float GetNightLengthRatio() {
			return this.nightLength;
		}

		private int GenerateLength() {
			float minutes;

			if (this.world.game.GetStorySession.characterStats.name == SlugcatStats.Name.Yellow || (ModManager.MSC && (this.world.game.GetStorySession.characterStats.name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Rivulet || this.world.game.GetStorySession.characterStats.name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Gourmand || this.world.game.GetStorySession.characterStats.name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Saint))) {
				minutes = Mathf.Lerp(this.world.game.rainWorld.setup.cycleTimeMin, this.world.game.rainWorld.setup.cycleTimeMax, 0.35f + 0.65f * Mathf.Pow(UnityEngine.Random.value, 1.2f)) / 60f;
			}
			else {
				minutes = Mathf.Lerp(this.world.game.rainWorld.setup.cycleTimeMin, this.world.game.rainWorld.setup.cycleTimeMax, UnityEngine.Random.value) / 60f;
			}

			if (ModManager.MMF && MoreSlugcats.MMF.cfgNoRandomCycles.Value) {
				minutes = this.world.game.rainWorld.setup.cycleTimeMax / 60f;
			}

			return (int) (minutes * 40f * 60f);
		}

		public void NextCycle() {
			this.RandomLength();

			this.dayNightCounter = 0;
			this.timer = 0;
			this.rainbowSeed = UnityEngine.Random.Range(0, 10000);
			this.sunDownStartTime = (int) Mathf.Lerp(this.baseCycleLength, this.world.game.rainWorld.setup.cycleTimeMax * 60f, UnityEngine.Random.Range(0.02f, 0.045f));
			this.maxPreTimer = 0;
			this.preTimer = this.maxPreTimer;
			this.world.game.GetStorySession.saveState.cycleNumber += 1;
		}

		public void RandomLength() {
			if (this.nextCycleLength == -1) {
				this.nextCycleLength = this.GenerateLength();
			}

			this.baseCycleLength = this.nextCycleLength;
			this.cycleLength = this.GetDesiredCycleLength();
			this.nightLength = 2f * 60f / 70f * UnityEngine.Random.Range(5f, 11f);
			this.sunDownStartTime = (int) Mathf.Lerp(this.baseCycleLength, this.world.game.rainWorld.setup.cycleTimeMax * 60f, UnityEngine.Random.Range(0.02f, 0.045f));

			this.nextCycleLength = this.GenerateLength();
		}
	}
}