namespace Dawn {
	public class Time : ExtEnum<Time> {
		public Time(string value, bool register = false) : base(value, register) { }

		public static readonly Time NONE = new Time("NONE", true);

		public static readonly Time Day = new Time("Day", true);
		public static readonly Time HalfDusk = new Time("HalfDusk", true);
		public static readonly Time Dusk = new Time("Dusk", true);
		public static readonly Time Night = new Time("Night", true);
		public static readonly Time Dawn = new Time("Dawn", true);
		public static readonly Time HalfDawn = new Time("HalfDawn", true);
	}
}