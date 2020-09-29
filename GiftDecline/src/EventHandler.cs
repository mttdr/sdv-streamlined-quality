namespace GiftDecline
{
	using System;
	using System.Collections.Generic;
	using Common;
	using StardewModdingAPI.Events;
	using StardewValley;
	using StardewValley.Menus;

	/// <summary>Listeners for SMAPI events.</summary>
	internal static class EventHandler
	{
		/// <summary>Switching GUI windows.</summary>
		/// <param name="isInDialog">Reference to a boolean to flip.</param>
		/// <param name="e">Event data.</param>
		public static void OnMenuChanged(ref bool isInDialog, MenuChangedEventArgs e)
		{
			isInDialog = false;

			// just talking to people increases their friendship
			// so update the friendship list each time a dialog is closed
			if (e.OldMenu is DialogueBox oldDialog && oldDialog.isPortraitBox())
			{
				NpcHelper.StoreFriendshipLevels(Game1.player.currentLocation.characters);
				return;
			}

			if (!(e.NewMenu is DialogueBox newDialog && newDialog.isPortraitBox())) return;

			isInDialog = true;
		}

		/// <summary>Player changes location.</summary>
		/// <param name="onItemRemoved">Callback.</param>
		/// <param name="e">Event data.</param>
		public static void OnInventoryChanged(Action<Item> onItemRemoved, InventoryChangedEventArgs e)
		{
			// only update the inventory of the target player
			if (!e.IsLocalPlayer) return;

			IEnumerator<Item> removed = e.Removed.GetEnumerator();
			while (removed.MoveNext())
			{
				onItemRemoved(removed.Current);
			}

			IEnumerator<ItemStackSizeChange> quantityChanged = e.QuantityChanged.GetEnumerator();
			while (quantityChanged.MoveNext())
			{
				if (quantityChanged.Current.NewSize < quantityChanged.Current.OldSize)
				{
					onItemRemoved(quantityChanged.Current.Item);
				}
			}
		}

		/// <summary>Player changes location.</summary>
		public static void OnWarped()
		{
			NpcHelper.StoreFriendshipLevels(Game1.player.currentLocation.characters);
		}

		/// <summary>Day ends (before save).</summary>
		/// <param name="config">Mod configuration object.</param>
		/// <param name="data">Save game data.</param>
		public static void OnDayEnding(ModConfig config, ModData data)
		{
			// apply gift taste changes at the end of day (and not immediately after gifting)
			// this way the social tab will show the reaction you actually got for that day
			SaveGameHelper.Apply(data);

			if (config.ResetEveryXDays == 0) return;

			int nextDay = Game1.Date.TotalDays + 1;
			if (nextDay % config.ResetEveryXDays == 0)
			{
				Logger.Trace("Resetting gift tastes");
				NpcHelper.ResetGiftTastes();
				SaveGameHelper.ResetGiftTastes(data);
			}
		}

		/// <summary>Just before a game is being saved.</summary>
		/// <param name="data">Save game data.</param>
		/// <param name="writeSaveData">Callback for writing save data.</param>
		public static void OnSaving(ModData data, Action<string, ModData> writeSaveData)
		{
			writeSaveData(SaveGameHelper.Key, data);
		}

		/// <summary>After save game got loaded (or new one is created).</summary>
		/// <param name="data">Save game data.</param>
		/// <param name="readSaveData">Callback for getting save data.</param>
		public static void OnSaveLoaded(ref ModData data, Func<string, ModData> readSaveData)
		{
			NpcHelper.StoreDefaultGiftTastes();

			data = readSaveData(SaveGameHelper.Key) ?? new ModData();
			SaveGameHelper.Apply(data);
		}

		/// <summary>NPCs got loaded.</summary>
		/// <param name="e">Event data.</param>
		public static void OnNpcListChanged(NpcListChangedEventArgs e)
		{
			if (!e.IsCurrentLocation) return;
			NpcHelper.StoreFriendshipLevels(e.Added);
		}
	}
}