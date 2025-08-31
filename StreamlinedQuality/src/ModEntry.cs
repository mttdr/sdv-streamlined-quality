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
		int AdjustedPrice;

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Logger.Init(this.Monitor);
			this.Config = this.Helper.ReadConfig<ModConfig>();
			helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
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

		private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
		{
			// get Generic Mod Config Menu's API (if it's installed)
			var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
			if (configMenu is null)
				return;

			// register mod
			configMenu.Register(
				mod: this.ModManifest,
				reset: () => this.Config = new ModConfig(),
				save: () => this.Helper.WriteConfig(this.Config));

			configMenu.AddParagraph(
				mod: this.ModManifest,
				text: () => "Artisan goods and Cooked dishes are not impacted by this mod and will always retain their quality.\n\nOther kinds of items are only influenced when moved into the player inventory, feel free to test and change these options, objects stored in chests will not be impacted until picked back up by the player.");

			configMenu.AddSectionTitle(
				mod: this.ModManifest,
				text: () => "Items allowed to retain Gold or Iridium Quality:",
				tooltip: null);

			configMenu.AddParagraph(
				mod: this.ModManifest,
				text: () => "Selected categories will mantain Gold/Iridium quality when picked up.");

			configMenu.AddBoolOption(
				mod: this.ModManifest,
				name: () => "Vegetables",
				tooltip: () => "Most of the crops you grow on the farm",
				getValue: () => this.Config.KeepGoldenVegetables,
				setValue: value => this.Config.KeepGoldenVegetables = value);

			configMenu.AddBoolOption(
				mod: this.ModManifest,
				name: () => "Fruits",
				tooltip: () => "Includes Sweet Gem Berry, does NOT include wild berries",
				getValue: () => this.Config.KeepGoldenFruits,
				setValue: value => this.Config.KeepGoldenFruits = value);

			configMenu.AddBoolOption(
				mod: this.ModManifest,
				name: () => "Milk & Eggs",
				tooltip: () => "Gold quality Milk/Eggs do NOT make for better cheese/mayo (Big Milk/Eggs do)",
				getValue: () => this.Config.KeepGoldenMilkEggs,
				setValue: value => this.Config.KeepGoldenMilkEggs = value);

			configMenu.AddBoolOption(
				mod: this.ModManifest,
				name: () => "Other Animal Products",
				tooltip: () => "Duck Feather, Wool, Truffles and Rabbit Foot",
				getValue: () => this.Config.KeepGoldenAnimalProducts,
				setValue: value => this.Config.KeepGoldenAnimalProducts = value);

			configMenu.AddBoolOption(
				mod: this.ModManifest,
				name: () => "Fish",
				tooltip: () => "Fish that you actively catch with a fishing rod",
				getValue: () => this.Config.KeepGoldenFish,
				setValue: value => this.Config.KeepGoldenFish = value);

			configMenu.AddBoolOption(
				mod: this.ModManifest,
				name: () => "Shells & Shellfish",
				tooltip: () => "What you either find on the beach or in Crab Pots",
				getValue: () => this.Config.KeepGoldenShells,
				setValue: value => this.Config.KeepGoldenShells = value);

			configMenu.AddBoolOption(
				mod: this.ModManifest,
				name: () => "Foraged Items",
				tooltip: () => "Mushrooms and other seasonal items that spawn in the wild",
				getValue: () => this.Config.KeepGoldenForage,
				setValue: value => this.Config.KeepGoldenForage = value);

			configMenu.AddBoolOption(
				mod: this.ModManifest,
				name: () => "Flowers",
				tooltip: () => "Includes both flowers that you plant and ones that spawn in the wild",
				getValue: () => this.Config.KeepGoldenFlowers,
				setValue: value => this.Config.KeepGoldenFlowers = value);

			configMenu.AddSectionTitle(
				mod: this.ModManifest,
				text: () => "\nMinimum Price Settings:",
				tooltip: null);

			configMenu.AddParagraph(
				mod: this.ModManifest,
				text: () => "Having cheap items occupy precious inventory slots is very annoying!\n\nUse the sliders below to only retain Gold/Iridium Quality items that sell above a certain price.\n(the corresponding checkbox above must also be selected!)");

			configMenu.AddNumberOption(
				mod: this.ModManifest,
				name: () => "Fruit price threshold:",
				tooltip: () => "Fruit below this sell price will be turned into Regular quality",
				min: 0,
				max: 1000,
				interval: 25,
				getValue: () => this.Config.MinimumFruitPrice,
				setValue: value => this.Config.MinimumFruitPrice = value);

			configMenu.AddNumberOption(
				mod: this.ModManifest,
				name: () => "Fish price threshold:",
				tooltip: () => "Fish below this sell price will be turned into Regular quality",
				min: 0,
				max: 1000,
				interval: 25,
				getValue: () => this.Config.MinimumFishPrice,
				setValue: value => this.Config.MinimumFishPrice = value);

			configMenu.AddNumberOption(
				mod: this.ModManifest,
				name: () => "Shell price threshold:",
				tooltip: () => "Shells below this sell price will be turned into Regular quality",
				min: 0,
				max: 1000,
				interval: 25,
				getValue: () => this.Config.MinimumShellPrice,
				setValue: value => this.Config.MinimumShellPrice = value);

			configMenu.AddNumberOption(
				mod: this.ModManifest,
				name: () => "Forage price threshold:",
				tooltip: () => "Forage below this sell price will be turned into Regular quality",
				min: 0,
				max: 1000,
				interval: 25,
				getValue: () => this.Config.MinimumForagePrice,
				setValue: value => this.Config.MinimumForagePrice = value);

			configMenu.AddNumberOption(
				mod: this.ModManifest,
				name: () => "Flowers price threshold:",
				tooltip: () => "Flowers below this sell price will be turned into Regular quality",
				min: 0,
				max: 1000,
				interval: 25,
				getValue: () => this.Config.MinimumFlowerPrice,
				setValue: value => this.Config.MinimumFlowerPrice = value);

			configMenu.AddSectionTitle(
				mod: this.ModManifest,
				text: () => "\nIridium Quality Reduction:",
				tooltip: null);

			configMenu.AddBoolOption(
				mod: this.ModManifest,
				name: () => "Reduce to double regular items",
				tooltip: () => "Uncheck to keep collecting Iridium quality items",
				getValue: () => this.Config.ReduceIridiumQuality,
				setValue: value => this.Config.ReduceIridiumQuality = value);

			configMenu.AddParagraph(
				mod: this.ModManifest,
				text: () => "Reducing Iridium quality to regular items will double the quantity of the object obtained.\n\nThis will ease inventory management while letting you keep the same profit when selling the items outright. It also gives you the chance to process double the artisan goods.");
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
				// otherwise re-adding the item would "autosort" them to
				// the first free slot when manually organizing the inventory
				if (item.Quality == 0) return;

				// Always retain quality of Artisan Goods (ie: aged in the Cellar)
				// and cooked dishes (otherwise Qi Seasoning would be useless)
				if (item.Category is Object.artisanGoodsCategory) return;
				if (item.Category is Object.CookingCategory) return;

				// reduce to Iridium items to regular quality, but double item quantity
				// (fair, since iridium quality sells for twice the regular price)
				// can be disabled in the config menu
				if (item.Quality == 4 && this.Config.ReduceIridiumQuality)
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

				// Keep Gold/Iridium quality items of selected categories
				// (mostly for selling at higher profit early game,
				// complete bundles, give as gift, use at Luau or the Fair)
				if (item.Quality >= 2)
				{
					// Price adjustment according to the quality:
					if (item.Quality == 2)
						this.AdjustedPrice = item.Price + (item.Price / 2);
					else if (item.Quality == 4)
						this.AdjustedPrice = item.Price * 2;

					if ((item.Category is Object.VegetableCategory) && this.Config.KeepGoldenVegetables) return;
					if ((item.Category is Object.EggCategory) && this.Config.KeepGoldenMilkEggs) return;
					if ((item.Category is Object.MilkCategory) && this.Config.KeepGoldenMilkEggs) return;
					if (item.Category is Object.GreensCategory && this.Config.KeepGoldenForage)
						if (this.AdjustedPrice >= this.Config.MinimumForagePrice) return;

					if (item.Category is Object.flowersCategory && this.Config.KeepGoldenFlowers)
						if (this.AdjustedPrice >= this.Config.MinimumFlowerPrice) return;

					if (item.Category is Object.sellAtFishShopCategory && this.Config.KeepGoldenShells)
						if (this.AdjustedPrice >= this.Config.MinimumShellPrice) return;

					if ((item.Category is Object.sellAtPierresAndMarnies || item.Name == "Truffle") &&
						 this.Config.KeepGoldenAnimalProducts) return;

					if (item.Name == "Sweet Gem Berry" && this.Config.KeepGoldenFruits)
						if (this.AdjustedPrice >= this.Config.MinimumFruitPrice) return;

					if (item.Category is Object.FruitsCategory && this.Config.KeepGoldenFruits)
					{
						// Price check and always discard wild berries
						if (this.AdjustedPrice >= this.Config.MinimumFruitPrice &&
							item.Name != "Blackberry" &&
							item.Name != "Salmonberry") return;
					}

					if (item.Category is Object.FishCategory)
					{
						// Check just for fish caught with a fishing rod
						if (this.Config.KeepGoldenFish &&
							this.AdjustedPrice >= this.Config.MinimumFishPrice &&
							item.Name != "Clam" &&
							item.Name != "Cockle" &&
							item.Name != "Mussel" &&
							item.Name != "Oyster") return;

						// Check for shells and molluscs
						if (this.Config.KeepGoldenShells &&
							this.AdjustedPrice >= this.Config.MinimumShellPrice &&
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
