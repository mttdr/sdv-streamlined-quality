namespace GiftDecline
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Common;
	using StardewValley;

	/// <summary>Utility functions for gift handling.</summary>
	internal static class NpcHelper
	{
		private const char XnbFieldSeparator = '/';
		private const char XnbGiftSeparator = ' ';

		private static readonly Dictionary<string, string> DefaultTastes = new Dictionary<string, string>();
		private static readonly Dictionary<string, int> FriendshipMap = new Dictionary<string, int>();

		/// <summary>Get the next lower gift taste level for an item from an NPC.</summary>
		/// <param name="npc">NPC for getting their current gift taste.</param>
		/// <param name="item">Item to reduce the interest for.</param>
		/// <returns>Gift taste level reduced by one.</returns>
		public static int GetReduceGiftTaste(NPC npc, Item item)
		{
			int tasteLevel = npc.getGiftTasteForThisItem(item);
			switch (tasteLevel)
			{
				case NPC.gift_taste_love:    return NPC.gift_taste_like;
				case NPC.gift_taste_like:    return NPC.gift_taste_neutral;
				case NPC.gift_taste_neutral: return NPC.gift_taste_dislike;
				case NPC.gift_taste_dislike: return NPC.gift_taste_hate;
				case NPC.gift_taste_hate:    return NPC.gift_taste_hate;
				default: throw new Exception("GetReduceGiftTaste: Unknown gift taste level \"" + tasteLevel + "\".");
			}
		}

		/// <summary>Get a readable version of a gift taste.</summary>
		/// <param name="tasteLevel">Taste level to get the name for.</param>
		/// <returns>Gift taste level reduced by one.</returns>
		public static string GetGiftTasteString(int tasteLevel)
		{
			switch (tasteLevel)
			{
				case NPC.gift_taste_love:    return "Love";
				case NPC.gift_taste_like:    return "Like";
				case NPC.gift_taste_neutral: return "Neutral";
				case NPC.gift_taste_dislike: return "Dislike";
				case NPC.gift_taste_hate:    return "Hate";
				default: throw new Exception("GetGiftTasteString: Unknown gift taste level \"" + tasteLevel + "\".");
			}
		}

		/// <summary>Set the taste level for an item of an NPC.</summary>
		/// <param name="npc">NPC of whom to overwrite the taste level.</param>
		/// <param name="item">Item affected by the change.</param>
		/// <param name="newGiftTaste">New taste level.</param>
		public static void SetGiftTasteLevel(NPC npc, Item item, int newGiftTaste)
		{
			int oldGiftTaste = npc.getGiftTasteForThisItem(item);
			if (newGiftTaste == oldGiftTaste) return;

			string itemId = item.ParentSheetIndex.ToString();
			string logString = $"Adjusting gift taste for {npc.Name}, item #{itemId} ({item.Name}) : ";
			logString += GetGiftTasteString(oldGiftTaste) + "(" + oldGiftTaste + ") -> ";
			logString += GetGiftTasteString(newGiftTaste) + "(" + newGiftTaste + ")";
			Logger.Trace(logString);

			// Original index refers to the reaction text. The corresponding items for this come afterwards.
			++oldGiftTaste;
			++newGiftTaste;

			string[] giftTasteData = Game1.NPCGiftTastes[npc.Name].Split(XnbFieldSeparator);

			List<string> oldGiftTasteData = giftTasteData[oldGiftTaste].Split(XnbGiftSeparator).ToList();
			if (oldGiftTasteData.Contains(itemId))
			{
				oldGiftTasteData.RemoveAt(oldGiftTasteData.IndexOf(itemId));
				giftTasteData[oldGiftTaste] = string.Join(XnbGiftSeparator.ToString(), oldGiftTasteData);
			}

			List<string> newGiftTasteData = giftTasteData[newGiftTaste].Split(XnbGiftSeparator).ToList();
			newGiftTasteData.Add(itemId);
			giftTasteData[newGiftTaste] = string.Join(XnbGiftSeparator.ToString(), newGiftTasteData);

			Game1.NPCGiftTastes[npc.Name] = string.Join(XnbFieldSeparator.ToString(), giftTasteData);
		}

		/// <summary>Check if a given NPC can receive gifts.</summary>
		/// <param name="npc">NPC to check.</param>
		/// <returns>Wether or not the NPC can receive gifts.</returns>
		public static bool AcceptsGifts(NPC npc)
		{
			// npc.CanSocialize -> Dwarf has this on false
			// npc.getGiftTasteForThisItem(item) -> Throws when used on characters that can't receive gifts, e.g. Gunther
			// npc.canReceiveThisItemAsGift(item) -> true for everyone
			return npc.Birthday_Day != 0; // valid birthdays start from 1
		}

		/// <summary>Save the default gift tastes of all NPCs to be able to reset them, should the need arise.</summary>
		public static void StoreDefaultGiftTastes()
		{
			if (DefaultTastes.Count > 0) return;

			foreach (string name in Game1.NPCGiftTastes.Keys)
			{
				DefaultTastes.Add(name, Game1.NPCGiftTastes[name]);
			}
		}

		/// <summary>Reset the gift taste of all NPCs.</summary>
		public static void ResetGiftTastes()
		{
			if (DefaultTastes.Count == 0) throw new Exception("Cannot restore default tastes. They are not yet stored.");

			foreach (string name in DefaultTastes.Keys)
			{
				Game1.NPCGiftTastes[name] = DefaultTastes[name];
			}
		}

		/// <summary>Check wether the friendship level differs from the last known state.</summary>
		/// <param name="npc">NPC whose friendship to store.</param>
		/// <returns>Wether or not the friendship level differs.</returns>
		public static bool HasFriendshipLevelChanged(NPC npc)
		{
			int lastFriendship = FriendshipMap[npc.Name];
			int currentFriendship = Game1.player.getFriendshipLevelForNPC(npc.Name);
			return lastFriendship != currentFriendship;
		}

		/// <summary>Get and store the current friendship level of an NPC.</summary>
		/// <param name="npcCollection">NPCs whose friendship to store.</param>
		public static void StoreFriendshipLevels(IEnumerable<NPC> npcCollection)
		{
			IEnumerator<NPC> characters = npcCollection.GetEnumerator();
			while (characters.MoveNext())
			{
				NPC npc = characters.Current;
				FriendshipMap[npc.Name] = Game1.player.getFriendshipLevelForNPC(npc.Name);
			}
		}
	}
}
