namespace StreamlinedQuality
{
	/// <summary>Mod Configuration settings.</summary>
	public sealed class ModConfig
	{
		public bool KeepGoldenVegetables { get; set; } = true;

		public bool KeepGoldenMilkEggs { get; set; } = true;

		public bool KeepGoldenAnimalProducts { get; set; } = true;

		public bool KeepGoldenFruits { get; set; } = true;

		public int MinimumFruitPrice { get; set; } = 0;

		public bool KeepGoldenFish { get; set; } = true;

		public int MinimumFishPrice { get; set; } = 0;

		public bool KeepGoldenShells { get; set; } = true;

		public int MinimumShellPrice { get; set; } = 0;

		public bool KeepGoldenForage { get; set; } = true;

		public int MinimumForagePrice { get; set; } = 0;

		public bool KeepGoldenFlowers { get; set; } = true;

		public int MinimumFlowerPrice { get; set; } = 0;
	}
}
