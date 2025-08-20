namespace StreamlinedQuality
{
	/// <summary>Mod Configuration settings.</summary>
	public sealed class ModConfig
	{
		public int PriceOverride { get; set; } = 150;

		public bool KeepGoldenVegetables { get; set; } = true;

		public bool KeepGoldenMilkEggs { get; set; } = true;

		public bool KeepGoldenAnimalProducts { get; set; } = true;

		public bool KeepGoldenFruits { get; set; } = false;

		public bool KeepGoldenFish { get; set; } = false;

		public bool KeepGoldenShells { get; set; } = false;

		public bool KeepGoldenForage { get; set; } = false;

		public bool KeepGoldenFlowers { get; set; } = false;
	}
}
