namespace StreamlinedQuality
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Transactions;
	using Common;
	using Force.DeepCloner;
	using StardewModdingAPI;
	using StardewModdingAPI.Events;
	using StardewValley;
	using StardewValley.Extensions;
	using StardewValley.GameData.Bundles;

	/// <summary>Main class.</summary>
	internal class ModEntry : Mod
	{
		/// <summary>The mod configuration from the player.</summary>
		private ModConfig Config;

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Logger.Init(this.Monitor);
			this.Config = this.Helper.ReadConfig<ModConfig>();
			helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
		}

		static void FixMessages(StardewValley.Object item)
		{
			// Look for existing "message notification" and if so delete it
			// Necessary to avoid a notification with double or triple quantity
			if (Game1.doesHUDMessageExist(item.DisplayName))
			{
				for (int i = 0; i < Game1.hudMessages.Count; i++)
				{
					if (Game1.hudMessages[i].message == item.DisplayName)
						Game1.hudMessages.Remove(Game1.hudMessages[i]);
				}
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

				// Always retain quality of Artisan Goods (ie: aged in the Cellar)
				if (item.Category is Object.artisanGoodsCategory) return;

				// quality is already regular, nothing to do
				// otherwise re-adding the item would "autosort" them to
				// the first free slot when manually organizing the inventory
				if (item.Quality == 0) return;

				// reduce to Iridium items to regular quality, but double item quantity
				// (fair, in the vanilla game iridium quality sells for twice the price)
				if (item.Quality == 4)
				{
					Game1.player.removeItemFromInventory(item);
					item.Quality = 0;
					StardewValley.Item duplicate = item.DeepClone();

					// Clear last notification then re-add the item
					FixMessages(item);

					Game1.player.addItemToInventory(item);
					Game1.player.addItemToInventory(duplicate);
					return;
				}

				// Keep Gold quality items of selected categories
				// (mostly for selling at higher profit early game,
				// complete bundles, give as gift, use at Luau or the Fair)
				if (item.Quality == 2)
				{
					if ((item.Category is Object.EggCategory) && this.Config.KeepGoldenEggs) return;
					if ((item.Category is Object.MilkCategory) && this.Config.KeepGoldenMilk) return;
					if ((item.Category is Object.GreensCategory) && this.Config.KeepGoldenForage) return;
					if ((item.Category is Object.flowersCategory) && this.Config.KeepGoldenFlowers) return;
					if ((item.Category is Object.VegetableCategory) && this.Config.KeepGoldenVegetables) return;
					if ((item.Category is Object.sellAtPierres) && this.Config.KeepGoldenAnimalProducts) return;
					if ((item.Category is Object.sellAtPierresAndMarnies) && this.Config.KeepGoldenAnimalProducts) return;
					if (item.Category is Object.FruitsCategory)
					{
						// Separate normal Fruit from "Wild Fruit"
						if (this.Config.KeepGoldenFruits &&
							item.Name != "Spice Berry" &&
							item.Name != "Blackberry" &&
							item.Name != "Salmonberry" &&
							item.Name != "Wild Plum" &&
							item.Name != "Cactus Fruit" &&
							item.Name != "Coconut") return;

						if (this.Config.KeepGoldenWildFruits &&
							(item.Name == "Spice Berry" ||
							item.Name == "Blackberry" ||
							item.Name == "Salmonberry" ||
							item.Name == "Wild Plum" ||
							item.Name == "Cactus Fruit" ||
							item.Name == "Coconut")) return;
					}

					if (item.Category is Object.FishCategory)
					{
						// Check just for fish caught with a fishing rod
						if (this.Config.KeepGoldenFish &&
							item.Name != "Clam" &&
							item.Name != "Cockle" &&
							item.Name != "Mussel" &&
							item.Name != "Oyster") return;
					}

					if (item.Category is Object.sellAtFishShopCategory)
					{
						// Check for shells and molluscs
						if (this.Config.KeepGoldenShells &&
							(item.Name == "Clam" ||
							item.Name == "Cockle" ||
							item.Name == "Mussel" ||
							item.Name == "Oyster")) return;
					}
				}

				// Finally reduce unwanted items to Regular quality
				// (because this happens only AFTER the item was added to the inventory,
				// make a best effort to stack the item with an already existing stack)
				Game1.player.removeItemFromInventory(item);
				item.Quality = 0;

				// Clear last notification then re-add the item
				FixMessages(item);
				Game1.player.addItemToInventory(item);
			}
		}
	}
}
