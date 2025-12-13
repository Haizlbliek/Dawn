namespace Dawn {
	static class TimeController {
		static private float GetOr(RoomSettings settings, RoomSettings.RoomEffect.Type type, float defaultValue, float strength) {
			if (strength <= 0.01f)
				return defaultValue * strength;

			RoomSettings.RoomEffect effect = settings.GetEffect(type);
			if (effect == null)
				return defaultValue * strength;

			return effect.amount * strength;
		}

		static private float GetStrength(DawnRoomSettings settings, RoomSettings.RoomEffect.Type type) {
			float defaultValue = GetOr(settings.GetTimeSetting(Time.NONE), type, 0.0f, 1.0f);

			return
				GetOr(settings.GetTimeSetting(Time.Day), type, defaultValue, Plugin.instance.fadeBlendDay) +
				GetOr(settings.GetTimeSetting(Time.HalfDusk), type, defaultValue, Plugin.instance.fadeBlendHalfDusk) +
				GetOr(settings.GetTimeSetting(Time.Dusk), type, defaultValue, Plugin.instance.fadeBlendDusk) +
				GetOr(settings.GetTimeSetting(Time.Night), type, defaultValue, Plugin.instance.fadeBlendNight) +
				GetOr(settings.GetTimeSetting(Time.Dawn), type, defaultValue, Plugin.instance.fadeBlendDawn) +
				GetOr(settings.GetTimeSetting(Time.HalfDawn), type, defaultValue, Plugin.instance.fadeBlendHalfDawn);
		}

		static public void ApplyRoomSettings(Room room) {
			if (room.roomSettings is not DawnRoomSettings settings)
				return;

			foreach (RoomSettings.RoomEffect effect in settings.effects) {
				effect.amount = GetStrength(settings, effect.type);
			}
		}
	}
}