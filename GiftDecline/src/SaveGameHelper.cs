namespace GiftDecline
{
	using System.Collections.Generic;
	using Common;
	using StardewModdingAPI;
	using StardewValley;

	/// <summary>Methods to alter the save game data.</summary>
	internal static class SaveGameHelper
	{
		/// <summary>Identifier to store and retrive this mod's data from the save game.</summary>
		public static string Key { get; } = "GiftDecline";

		/// <summary>Clear the list of gift taste differences.</summary>
		/// <param name="data">Save game data.</param>
		public static void ResetGiftTastes(ModData data)
		{
			data.GiftTasteOverwrites = new Dictionary<string, Dictionary<string, int>>();
		}

		/// <summary>Set the gift taste overwrite value for an npc and item.</summary>
		/// <param name="data">Save game data.</param>
		/// <param name="npcName">NPC name.</param>
		/// <param name="itemId">Item id.</param>
		/// <param name="giftTaste">Overwritten gift taste value.</param>
		public static void SetGiftTaste(ModData data, string npcName, string itemId, int giftTaste)
		{
			if (!data.GiftTasteOverwrites.ContainsKey(npcName))
			{
				data.GiftTasteOverwrites.Add(npcName, new Dictionary<string, int>());
			}

			data.GiftTasteOverwrites[npcName][itemId] = giftTaste;
		}

		/// <summary>Apply the current overwrites to the game.</summary>
		/// <param name="data">Save game data.</param>
		public static void Apply(ModData data)
		{
			foreach (string npcName in data.GiftTasteOverwrites.Keys)
			{
				NPC npc = Game1.getCharacterFromName(npcName);

				foreach (string itemId in data.GiftTasteOverwrites[npcName].Keys)
				{
					Item item = new Object(int.Parse(itemId), 1);
					NpcHelper.SetGiftTasteLevel(npc, item, data.GiftTasteOverwrites[npcName][itemId]);
				}
			}
		}

		/// <summary>Add command to reset the save state of the mod.</summary>
		/// <param name="helper">Helper object through which the command can be added.</param>
		/// <param name="getData">Save game data. Passed in as a function so it can be retrieved after initialization.</param>
		public static void AddResetCommand(IModHelper helper, System.Func<ModData> getData)
		{
			helper.ConsoleCommands.Add(
				"reset_gift_tastes",
				"Reset gift taste of all NPCs to their default value.",
				(string _, string[] __) => ResetCommandHandler(getData()));
		}

		private static void ResetCommandHandler(ModData data)
		{
			if (data == null)
			{
				Logger.Error("No data to reset. Are you still in the main menu?");
			}

			NpcHelper.ResetGiftTastes();
			ResetGiftTastes(data);
			Logger.Info("Success");
		}
	}
}
