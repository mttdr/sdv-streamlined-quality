namespace StreamlinedQuality
{
	using System.Collections.Generic;
	using System.Linq;
	using Common;
	using StardewModdingAPI;
	using StardewModdingAPI.Events;
	using StardewValley;
	using StardewValley.GameData.Bundles;

	/// <summary>Main class.</summary>
	internal class ModEntry : Mod
	{
		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Logger.Init(this.Monitor);
			helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
		}

		{
			{
				{
			}
		}

		/// <summary>Raised AFTER the player receives an item.</summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event data.</param>
		private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
		{
			// only update the inventory of the target player
			if (!e.IsLocalPlayer) return;

			IEnumerator<Item> enumerator = e.Added.GetEnumerator();
			while (enumerator.MoveNext())
			{
				// not an item with a quality property, skip
				if (!(enumerator.Current is StardewValley.Object item)) return;

				// quality is already regular, nothing to do
				// otherwise re-adding the item would "autosort" them to the first free slot when manually organizing the inventory
				if (item.Quality == 0) return;

				// remove quality
				// because this happens only AFTER the item was added to the inventory,
				// make a best effort to stack the item with an already existing stack
				Game1.player.removeItemFromInventory(item);
				item.Quality = 0;
				Game1.player.addItemToInventory(item);
			}
		}
	}
}
}
