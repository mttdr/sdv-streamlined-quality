namespace GiftDecline
{
	using System.Collections.Generic;

	/// <summary>Mod save game data.</summary>
	internal class ModData
	{
		/// <summary>List of gift taste differences.</summary>
		public Dictionary<string, Dictionary<string, int>> GiftTasteOverwrites { get; set; } = new Dictionary<string, Dictionary<string, int>>();
	}
}