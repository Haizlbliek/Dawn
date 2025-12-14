namespace Dawn;

public static class DawnEnums {
	public static RoomSettings.RoomEffect.Type Dawn;

	public static void Initialize() {
		Dawn = new RoomSettings.RoomEffect.Type("Dawn", true);
	}

	public static void Cleanup() {
		Dawn?.Unregister(); Dawn = null;
	}
}