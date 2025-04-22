using UnityEngine;

namespace Dawn {
    class DawnRainCycle : RainCycle {
        public bool inRoomWithDawn = false;

        public int nextCycleLength = -1;

        private float nightLength;

        public DawnRainCycle(World world, float minutes) : base(world, minutes) {
            RandomLength();
        }

        public float GetNightLengthRatio() {
            return nightLength;
        }

        private int GenerateLength() {
            float minutes;

            if (world.game.GetStorySession.characterStats.name == SlugcatStats.Name.Yellow || (ModManager.MSC && (world.game.GetStorySession.characterStats.name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Rivulet || world.game.GetStorySession.characterStats.name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Gourmand || world.game.GetStorySession.characterStats.name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Saint))) {
                minutes = Mathf.Lerp((float)world.game.rainWorld.setup.cycleTimeMin, (float)world.game.rainWorld.setup.cycleTimeMax, 0.35f + 0.65f * Mathf.Pow(UnityEngine.Random.value, 1.2f)) / 60f;
            } else {
                minutes = Mathf.Lerp((float)world.game.rainWorld.setup.cycleTimeMin, (float)world.game.rainWorld.setup.cycleTimeMax, UnityEngine.Random.value) / 60f;
            }

            if (ModManager.MMF && MoreSlugcats.MMF.cfgNoRandomCycles.Value) {
                minutes = (float)world.game.rainWorld.setup.cycleTimeMax / 60f;
            }

            return (int) (minutes * 40f * 60f);
        }
        
        public void NextCycle() {
            this.RandomLength();

            this.dayNightCounter = 0;
			this.timer = 0;
            this.rainbowSeed = UnityEngine.Random.Range(0, 10000);
            this.sunDownStartTime = (int)Mathf.Lerp((float)this.baseCycleLength, (float)this.world.game.rainWorld.setup.cycleTimeMax * 60f, UnityEngine.Random.Range(0.02f, 0.045f));
            this.maxPreTimer = 0;
            this.preTimer = this.maxPreTimer;
            this.world.game.GetStorySession.saveState.cycleNumber += 1;
        }

        public void RandomLength() {
            if (nextCycleLength == -1) {
                nextCycleLength = GenerateLength();
            }

            this.baseCycleLength = this.nextCycleLength;
            this.cycleLength = this.GetDesiredCycleLength();
            this.nightLength = 2f * 60f / 70f * UnityEngine.Random.Range(5f, 11f);
            this.sunDownStartTime = (int) Mathf.Lerp(this.baseCycleLength, world.game.rainWorld.setup.cycleTimeMax * 60f, UnityEngine.Random.Range(0.02f, 0.045f));
            
            nextCycleLength = GenerateLength();
        }
    }
}