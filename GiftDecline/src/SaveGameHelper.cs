namespace GiftDecline
{
	using System.Collections.Generic;
	using Common;
	using StardewModdingAPI;
	using StardewValley;

	/// <summary>Methods to alter the save game data.</summary>
	internal static class SaveGameHelper
	{
		private static IDataHelper dataHelper;

		/// <summary>Save game data. This is only persisted for the main player.</summary>
		public static ModData SaveState { get; set; }

		/// <summary>Identifier to store and retrive this mod's data from the save game.</summary>
		public static string Key { get; } = "GiftDecline";

		/// <summary>Initialize the helper.</summary>
		/// <param name="helper">Instance to use for reading from and writing to the save file.</param>
		public static void Init(IDataHelper helper)
		{
			dataHelper = helper;
		}

		/// <summary>Write the current save state to the game's save file.</summary>
		public static void WriteToFile()
		{
			dataHelper.WriteSaveData(Key, SaveState);
		}

		/// <summary>Load an existing save state, or create a new one.</summary>
		public static void LoadFromFileOrInitialize()
		{
			SaveState = dataHelper.ReadSaveData<ModData>(Key) ?? new ModData();
		}

		/// <summary>Clear the list of gift taste differences.</summary>
		public static void ResetGiftTastes()
		{
			SaveState.GiftTasteOverwrites = new Dictionary<string, Dictionary<string, int>>();
		}

		/// <summary>Set the gift taste overwrite value for an npc and item.</summary>
		/// <param name="npcName">NPC name.</param>
		/// <param name="itemId">Item id.</param>
		/// <param name="giftTaste">Overwritten gift taste value.</param>
		public static void SetGiftTaste(string npcName, string itemId, int giftTaste)
		{
			if (!SaveState.GiftTasteOverwrites.ContainsKey(npcName))
			{
				SaveState.GiftTasteOverwrites.Add(npcName, new Dictionary<string, int>());
			}

			SaveState.GiftTasteOverwrites[npcName][itemId] = giftTaste;

			MultiplayerHelper.SendMessage(SaveState, Key);
		}

		/// <summary>Apply the current overwrites to the game.</summary>
		public static void Apply()
		{
			foreach (string npcName in SaveState.GiftTasteOverwrites.Keys)
			{
				NPC npc = Game1.getCharacterFromName(npcName);

				foreach (string itemId in SaveState.GiftTasteOverwrites[npcName].Keys)
				{
					Item item = new Object(int.Parse(itemId), 1);
					NpcHelper.SetGiftTasteLevel(npc, item, SaveState.GiftTasteOverwrites[npcName][itemId]);
				}
			}
		}

		/// <summary>Add command to reset the save state of the mod.</summary>
		/// <param name="helper">Helper object through which the command can be added.</param>
		public static void AddResetCommand(IModHelper helper)
		{
			helper.ConsoleCommands.Add(
				"reset_gift_tastes",
				"Reset gift taste of all NPCs to their default value.",
				(string _, string[] __) => ResetCommandHandler());
		}

		private static void ResetCommandHandler()
		{
			if (SaveState == null)
			{
				Logger.Error("No data to reset. Are you still in the main menu?");
				return;
			}

			NpcHelper.ResetGiftTastes();
			ResetGiftTastes();
			Logger.Info("Success");
		}
	}
}
