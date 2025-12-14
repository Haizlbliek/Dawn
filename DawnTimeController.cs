using UnityEngine;

namespace Dawn;

public static class TimeController {
	static private float GetOr(RoomSettings settings, RoomSettings.RoomEffect.Type type, float defaultValue) {
		RoomSettings.RoomEffect effect = settings.GetEffect(type);

		return effect?.amount ?? defaultValue;
	}

	static private float GetStrength(DawnRoomSettings settings, RoomSettings.RoomEffect.Type type) {
		float defaultValue = GetOr(settings.GetTimeSetting(Time.NONE), type, 0.0f);

		return Mathf.Lerp(
			GetOr(settings.GetTimeSetting(Plugin.instance.timeLerpA), type, defaultValue),
			GetOr(settings.GetTimeSetting(Plugin.instance.timeLerpB), type, defaultValue),
			Plugin.instance.lerpAmount
		);
	}

	static public void ApplyRoomSettings(Room room) {
		if (room.roomSettings is not DawnRoomSettings settings)
			return;

		foreach (RoomSettings.RoomEffect effect in settings.effects) {
			effect.amount = GetStrength(settings, effect.type);
		}
	}
}