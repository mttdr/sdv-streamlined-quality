namespace Common
{
	using System.Collections.Generic;
	using StardewValley;
	using StardewValley.Quests;

	/// <summary>Utility functions to work with the various quest types.</summary>
	internal static class QuestLogHelper
	{
		/// <summary>Get a daily quest the player has accepted.</summary>
		/// <returns>Array of daily quests.</returns>
		public static ItemDeliveryQuest[] GetDailyItemDeliveryQuests()
		{
			List<ItemDeliveryQuest> quests = new List<ItemDeliveryQuest>();

			var enumerator = Game1.player.questLog.GetEnumerator();
			while (enumerator.MoveNext())
			{
				// daily quests have no ID
				if (enumerator.Current.id.Value == null && enumerator.Current is ItemDeliveryQuest itemDeliveryQuest)
				{
					quests.Add(itemDeliveryQuest);
				}
			}

			return quests.ToArray();
		}
	}
}
