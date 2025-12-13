namespace Dawn {
	static class DawnEnums {
		public static RoomSettings.RoomEffect.Type Dawn;

		public static PlacedObject.Type DawnData;
		public static PlacedObject.Type HalfDuskEffectColours;
		public static PlacedObject.Type DuskEffectColours;
		public static PlacedObject.Type NightEffectColours;
		public static PlacedObject.Type DawnEffectColours;
		public static PlacedObject.Type HalfDawnEffectColours;

		public static void Initialize() {
			Dawn = new RoomSettings.RoomEffect.Type("Dawn", true);

			DawnData = new PlacedObject.Type("DawnData", true);
			HalfDuskEffectColours = new PlacedObject.Type("HalfDuskEffectColours", true);
			DuskEffectColours = new PlacedObject.Type("DuskEffectColours", true);
			NightEffectColours = new PlacedObject.Type("NightEffectColours", true);
			DawnEffectColours = new PlacedObject.Type("DawnEffectColours", true);
			HalfDawnEffectColours = new PlacedObject.Type("HalfDawnEffectColours", true);
		}

		public static void Cleanup() {
			Dawn?.Unregister(); Dawn = null;

			DawnData?.Unregister(); DawnData = null;
			HalfDuskEffectColours?.Unregister(); HalfDuskEffectColours = null;
			DuskEffectColours?.Unregister(); DuskEffectColours = null;
			NightEffectColours?.Unregister(); NightEffectColours = null;
			DawnEffectColours?.Unregister(); DawnEffectColours = null;
			HalfDawnEffectColours?.Unregister(); HalfDawnEffectColours = null;
		}
	}
}