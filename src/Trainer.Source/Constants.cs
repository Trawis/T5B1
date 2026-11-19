namespace Trainer_v5
{
	public static class Constants
	{
		public const int ELEMENT_WIDTH = 150;
		public const int ELEMENT_HEIGHT = 32;

		public const int X_EMPLOYEESKILLCHANGE_WINDOW = SECOND_COLUMN + ELEMENT_WIDTH + FIRST_COLUMN;

		public const int FIRST_COLUMN = 1;
		public const int SECOND_COLUMN = 161;
		public const int THIRD_COLUMN = 322;
		public const int FOURTH_COLUMN = 483;

		public const float BOOKSHELF_AURA_BOOSTED = 0.75f;
		public const float CHAIR_COMFORT_LOW = 1.2f;
		public const float CHAIR_COMFORT_HIGH = 1.5f;
		public const int ENV_FULL = 8;
		public const int ROOM_BRIGHTNESS_FULL = 16;
		public const int MAX_FLOOR = 100;
		public const int MAX_BOXES_BOOSTED = 108;
		public const int MAX_BOXES_DEFAULT = 54;
		public const int MAX_CARRY_BOOSTED = 18;
		public const int MAX_CARRY_DEFAULT = 9;
		public const float WALK_SPEED_BOOSTED = 4f;
		public const float WALK_SPEED_DEFAULT = 2f;
		public const float PRINTER_PRICE_DEFAULT = 0.1f;

		// Offered by the new-game screen in place of the game's four-entry defaults. The game only reads these arrays
		// (ActorCustomization.StartYears / StartLoans), so one shared instance is safe.
		public static readonly int[] CUSTOMIZATION_START_YEARS = { 1970, 1975, 1980, 1985, 1990, 1995, 2000, 2005, 2010, 2015, 2020, 2025, 2030, 2035, 2040, 2045, 2050, 2060, 2070, 2080, 2090, 2100 };
		public static readonly int[] CUSTOMIZATION_START_LOANS = { 0, 1000, 2000, 5000, 10000, 20000, 50000, 100000, 200000, 500000, 1000000, 5000000, 10000000 };
	}
}
