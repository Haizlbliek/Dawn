namespace Dawn {
	static class TimeController {
		static private float GetOr(RoomSettings settings, RoomSettings.RoomEffect.Type type, float defaultValue, float strength) {
			if (strength <= 0.01f) return defaultValue * strength;

			RoomSettings.RoomEffect effect = settings.GetEffect(type);
			if (effect == null) return defaultValue * strength;
			
			return effect.amount * strength;
		}
	
		static private float GetStrength(DawnRoomSettings settings, RoomSettings.RoomEffect.Type type) {
			float defaultValue = GetOr(settings.GetTimeSetting(Time.NONE), type, 0.0f, 1.0f);
			
			float totalStrength = DayNight.instance.fadeBlendDay + DayNight.instance.fadeBlendHalfDusk + DayNight.instance.fadeBlendDusk + DayNight.instance.fadeBlendDark + DayNight.instance.fadeBlendDawn + DayNight.instance.fadeBlendHalfDawn;
			if (totalStrength <= 0.01f) return defaultValue;
			
			return
				GetOr(settings.GetTimeSetting(Time.Day), type, defaultValue, DayNight.instance.fadeBlendDay) +
				GetOr(settings.GetTimeSetting(Time.HalfDusk), type, defaultValue, DayNight.instance.fadeBlendHalfDusk) +
				GetOr(settings.GetTimeSetting(Time.Dusk), type, defaultValue, DayNight.instance.fadeBlendDusk) +
				GetOr(settings.GetTimeSetting(Time.Night), type, defaultValue, DayNight.instance.fadeBlendDark) +
				GetOr(settings.GetTimeSetting(Time.Dawn), type, defaultValue, DayNight.instance.fadeBlendDawn) +
				GetOr(settings.GetTimeSetting(Time.HalfDawn), type, defaultValue, DayNight.instance.fadeBlendHalfDawn)
			;
		}
	
		static public void ApplyRoomSettings(Room room) {
			DawnRoomSettings settings = room.roomSettings as DawnRoomSettings;
			if (settings == null) return;
			
			foreach (RoomSettings.RoomEffect effect in settings.effects) {
				effect.amount = GetStrength(settings, effect.type);
			}
		}
	}
}