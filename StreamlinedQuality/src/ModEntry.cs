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
				text: () => "Reminder: Artisan goods are not impacted by this mod and will always retain their quality.\n\nOther kinds of items are only influenced when moved into the player inventory, feel free to test and change these options, objects stored in chests will not be impacted until picked up.");

			configMenu.AddSectionTitle(
				mod: this.ModManifest,
				text: () => "Items allowed to retain Gold Quality:",
				tooltip: null);

			configMenu.AddParagraph(
				mod: this.ModManifest,
				text: () => "Selected categories will mantain gold quality when picked up.");

			configMenu.AddBoolOption(
				mod: this.ModManifest,
				name: () => "Vegetables",
				tooltip: () => null,
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
				tooltip: () => "Remember that Gold quality Milk/Eggs do NOT make for better cheese/mayo (Big Milk/Eggs do)",
				getValue: () => this.Config.KeepGoldenMilkEggs,
				setValue: value => this.Config.KeepGoldenMilkEggs = value);

			configMenu.AddBoolOption(
				mod: this.ModManifest,
				name: () => "Other Animal Products",
				tooltip: () => "Duck Feather, Wools, Truffles and Rabbit Foot",
				getValue: () => this.Config.KeepGoldenAnimalProducts,
				setValue: value => this.Config.KeepGoldenAnimalProducts = value);

			configMenu.AddBoolOption(
				mod: this.ModManifest,
				name: () => "Fish",
				tooltip: () => "Includes fish that you catch with a fishing rod",
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
				tooltip: () => "Mushrooms and other seasonable items that spawn in the wild",
				getValue: () => this.Config.KeepGoldenForage,
				setValue: value => this.Config.KeepGoldenForage = value);

			configMenu.AddBoolOption(
				mod: this.ModManifest,
				name: () => "Flowers",
				tooltip: () => null,
				getValue: () => this.Config.KeepGoldenFlowers,
				setValue: value => this.Config.KeepGoldenFlowers = value);

			configMenu.AddParagraph(
				mod: this.ModManifest,
				text: () => "Use these sliders to only pick up items above a certain sell price.\n(the corresponding checkbox above must be selected)");

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
					if ((item.Category is Object.VegetableCategory) && this.Config.KeepGoldenVegetables) return;
					if ((item.Category is Object.EggCategory) && this.Config.KeepGoldenMilkEggs) return;
					if ((item.Category is Object.MilkCategory) && this.Config.KeepGoldenMilkEggs) return;
					if (item.Category is Object.GreensCategory && this.Config.KeepGoldenForage)
						if (item.Price >= this.Config.MinimumForagePrice) return;

					if (item.Category is Object.flowersCategory && this.Config.KeepGoldenFlowers)
						if (item.Price >= this.Config.MinimumFlowerPrice) return;

					if (item.Category is Object.sellAtFishShopCategory && this.Config.KeepGoldenShells)
						if (item.Price >= this.Config.MinimumShellPrice) return;

					if ((item.Category is Object.sellAtPierresAndMarnies || item.Name == "Truffle") &&
						 this.Config.KeepGoldenAnimalProducts) return;

					if (item.Name == "Sweet Gem Berry" && this.Config.KeepGoldenFruits)
						if (item.Price >= this.Config.MinimumFruitPrice) return;

					if (item.Category is Object.FruitsCategory && this.Config.KeepGoldenFruits)
					{
						// Price check and always discard wild berries
						if (item.Price >= this.Config.MinimumFruitPrice &&
							item.Name != "Blackberry" &&
							item.Name != "Salmonberry") return;
					}

					if (item.Category is Object.FishCategory)
					{
						// Check just for fish caught with a fishing rod
						if (this.Config.KeepGoldenFish &&
							item.Price >= this.Config.MinimumFishPrice &&
							item.Name != "Clam" &&
							item.Name != "Cockle" &&
							item.Name != "Mussel" &&
							item.Name != "Oyster") return;

						// Check for shells and molluscs
						if (this.Config.KeepGoldenShells &&
							item.Price >= this.Config.MinimumShellPrice &&
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
