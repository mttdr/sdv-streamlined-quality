namespace GiftDecline
{
	using StardewModdingAPI;

	/// <summary>Mod Configuration helper.</summary>
	internal static class ConfigHelper
	{
		/// <summary>Mod Configuration settings.</summary>
		public static ModConfig Config { get; set; }

		/// <summary>Initialize the helper.</summary>
		/// <param name="helper">Instance to use for reading the config file.</param>
		public static void Init(IModHelper helper)
		{
			Config = helper.ReadConfig<ModConfig>();
			if (Config.ResetEveryXDays < 0)
			{
				throw new System.Exception("Error in config.json: \"ResetEveryXDays\" must be at least 0.");
			}

			if (Config.MaxReduction < 1)
			{
				throw new System.Exception("Error in config.json: \"MaxReduction\" must be at least 1.");
			}

			if (Config.MaxReduction > 4)
			{
				throw new System.Exception("Error in config.json: \"MaxReduction\" must be at most 4.");
			}
		}
	}
}