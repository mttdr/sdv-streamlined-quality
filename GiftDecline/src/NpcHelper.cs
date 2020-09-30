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

		private static readonly int[] GiftTasteLevels =
		{
			NPC.gift_taste_love, NPC.gift_taste_like, NPC.gift_taste_neutral, NPC.gift_taste_dislike, NPC.gift_taste_hate,
		};

		private static readonly Dictionary<string, string> DefaultTastesXnb = new Dictionary<string, string>();
		private static readonly Dictionary<string, Dictionary<int, int>> DefaultTastesMap = new Dictionary<string, Dictionary<int, int>>();
		private static readonly Dictionary<string, Dictionary<int, int>> GiftsReceived = new Dictionary<string, Dictionary<int, int>>();

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

		/// <summary>Set the taste level for an item of an NPC.</summary>
		/// <param name="npc">NPC of whom to overwrite the taste level.</param>
		/// <param name="item">Item affected by the change.</param>
		/// <param name="newGiftTaste">New taste level.</param>
		public static void SetGiftTasteLevel(NPC npc, Item item, int newGiftTaste)
		{
			int oldGiftTaste = npc.getGiftTasteForThisItem(item);
			if (newGiftTaste == oldGiftTaste) return;

			int itemId = item.ParentSheetIndex;

			if (!HasGiftTasteBeenOverwritten(npc.Name, itemId))
			{
				StoreDefaultGiftTasteForItem(npc.Name, itemId, oldGiftTaste);
			}

			int reduction = GetCurrentReduction(npc, item);
			int maxReduction = ConfigHelper.Config.MaxReduction;
			if (reduction >= maxReduction)
			{
				string logDontAdjust = $"Not adjusting gift taste for {npc.Name}, item #{itemId} ({item.Name}) : ";
				logDontAdjust += "MaxReduction (" + maxReduction + ") has been reached. (" + reduction + ")";
				Logger.Trace(logDontAdjust);
				return;
			}

			int clampedNewGiftTaste = GetClampedGiftTasteOverwrite(oldGiftTaste, newGiftTaste);
			if (clampedNewGiftTaste != newGiftTaste)
			{
				string logClamped = "Limiting intended gift taste overwrite from ";
				logClamped += GetGiftTasteString(newGiftTaste) + " to " + GetGiftTasteString(clampedNewGiftTaste);
				Logger.Trace(logClamped);
				newGiftTaste = clampedNewGiftTaste;
			}

			string logDoAdjust = $"Adjusting gift taste for {npc.Name}, item #{itemId} ({item.Name}) : ";
			logDoAdjust += GetGiftTasteString(oldGiftTaste) + " -> " + GetGiftTasteString(newGiftTaste);
			Logger.Trace(logDoAdjust);

			// Original index refers to the reaction text. The corresponding items for this come afterwards.
			++oldGiftTaste;
			++newGiftTaste;

			string itemIdString = itemId.ToString();
			string[] giftTasteData = Game1.NPCGiftTastes[npc.Name].Split(XnbFieldSeparator);

			List<string> oldGiftTasteData = giftTasteData[oldGiftTaste].Split(XnbGiftSeparator).ToList();
			if (oldGiftTasteData.Contains(itemIdString))
			{
				oldGiftTasteData.RemoveAt(oldGiftTasteData.IndexOf(itemIdString));
				giftTasteData[oldGiftTaste] = string.Join(XnbGiftSeparator.ToString(), oldGiftTasteData);
			}

			List<string> newGiftTasteData = giftTasteData[newGiftTaste].Split(XnbGiftSeparator).ToList();
			newGiftTasteData.Add(itemIdString);
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
			if (DefaultTastesXnb.Count > 0) return;

			foreach (string name in Game1.NPCGiftTastes.Keys)
			{
				DefaultTastesXnb.Add(name, Game1.NPCGiftTastes[name]);
			}
		}

		/// <summary>Reset the gift taste of all NPCs.</summary>
		public static void ResetGiftTastes()
		{
			if (DefaultTastesXnb.Count == 0) throw new Exception("Cannot restore default tastes. They are not yet stored.");

			foreach (string name in DefaultTastesXnb.Keys)
			{
				Game1.NPCGiftTastes[name] = DefaultTastesXnb[name];
			}
		}

		/// <summary>Check wether the friendship level differs from the last known state.</summary>
		/// <param name="npc">NPC whose friendship to store.</param>
		/// <param name="item">Item to maybe have received.</param>
		/// <returns>Wether or not the friendship level differs.</returns>
		public static bool HasJustReceivedGift(NPC npc, Item item)
		{
			var giftedItems = Game1.player.giftedItems;
			if (!giftedItems.ContainsKey(npc.Name)) return false;

			var giftedToNpc = giftedItems[npc.Name];
			if (!giftedToNpc.ContainsKey(item.ParentSheetIndex)) return false;

			if (!GiftsReceived.ContainsKey(npc.Name)) return true;
			if (!GiftsReceived[npc.Name].ContainsKey(item.ParentSheetIndex)) return true;

			int lastAmount = GiftsReceived[npc.Name][item.ParentSheetIndex];
			int currentAmount = giftedToNpc[item.ParentSheetIndex];
			return lastAmount != currentAmount;
		}

		/// <summary>Get and store the current friendship level of a list of NPCs.</summary>
		/// <param name="npcCollection">NPCs whose friendship to store.</param>
		public static void StoreAmountOfGiftsReceived(IEnumerable<NPC> npcCollection)
		{
			IEnumerator<NPC> characters = npcCollection.GetEnumerator();
			while (characters.MoveNext())
			{
				NPC npc = characters.Current;

				var giftedItems = Game1.player.giftedItems;
				if (!giftedItems.ContainsKey(npc.Name)) continue;

				var giftedToNpc = giftedItems[npc.Name];
				foreach (int itemId in giftedToNpc.Keys)
				{
					StoreReceivedGift(npc.Name, itemId, giftedToNpc[itemId]);
				}
			}
		}

		private static void StoreReceivedGift(string npcName, int itemId, int amount)
		{
			if (!GiftsReceived.ContainsKey(npcName))
			{
				GiftsReceived.Add(npcName, new Dictionary<int, int>());
			}

			GiftsReceived[npcName][itemId] = amount;
		}

		private static void StoreDefaultGiftTasteForItem(string npcName, int itemId, int giftTaste)
		{
			if (!DefaultTastesMap.ContainsKey(npcName))
			{
				DefaultTastesMap.Add(npcName, new Dictionary<int, int>());
			}

			DefaultTastesMap[npcName][itemId] = giftTaste;
		}

		private static bool HasGiftTasteBeenOverwritten(string npcName, int itemId)
		{
			if (!DefaultTastesMap.ContainsKey(npcName)) return false;
			if (!DefaultTastesMap[npcName].ContainsKey(itemId)) return false;
			return true;
		}

		/// <summary>Get how by how much the teaste of a gift has been reduced already.</summary>
		private static int GetCurrentReduction(NPC npc, Item item)
		{
			string npcName = npc.Name;
			int itemId = item.ParentSheetIndex;
			if (!HasGiftTasteBeenOverwritten(npcName, itemId)) return 0;

			int originalTasteLevel = DefaultTastesMap[npc.Name][itemId];
			int currentTasteLevel = npc.getGiftTasteForThisItem(item);

			int originalTasteIndex = Array.IndexOf(GiftTasteLevels, originalTasteLevel);
			int currentTasteIndex = Array.IndexOf(GiftTasteLevels, currentTasteLevel);
			return currentTasteIndex - originalTasteIndex;
		}

		/// <summary>Get the target taste reduction, limited by the config setting.</summary>
		private static int GetClampedGiftTasteOverwrite(int fromTaste, int toTaste)
		{
			int fromIndex = Array.IndexOf(GiftTasteLevels, fromTaste);
			int toIndex = Array.IndexOf(GiftTasteLevels, toTaste);

			int reduction = toIndex - fromIndex;
			int maxReduction = ConfigHelper.Config.MaxReduction;
			if (reduction > maxReduction)
			{
				toIndex = fromIndex + maxReduction;
			}

			return GiftTasteLevels[toIndex];
		}

		/// <summary>Get a readable version of a gift taste.</summary>
		private static string GetGiftTasteString(int tasteLevel)
		{
			string label;
			switch (tasteLevel)
			{
				case NPC.gift_taste_love:    label = "Love"; break;
				case NPC.gift_taste_like:    label = "Like"; break;
				case NPC.gift_taste_neutral: label = "Neutral"; break;
				case NPC.gift_taste_dislike: label = "Dislike"; break;
				case NPC.gift_taste_hate:    label = "Hate"; break;
				default: throw new Exception("GetGiftTasteString: Unknown gift taste level \"" + tasteLevel + "\".");
			}

			return label + "(" + tasteLevel + ")";
		}
	}
}