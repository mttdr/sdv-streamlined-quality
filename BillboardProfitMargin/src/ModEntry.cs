namespace BillboardProfitMargin
{
	using System;
	using System.Collections.Generic;
	using Common;
	using StardewModdingAPI;
	using StardewModdingAPI.Events;
	using StardewValley;
	using StardewValley.GameData.SpecialOrders;
	using StardewValley.Menus;
	using StardewValley.Quests;

	/// <summary>Main class.</summary>
	internal class ModEntry : Mod
	{
		private ModConfig config;

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Logger.Init(this.Monitor);

			this.config = this.Helper.ReadConfig<ModConfig>();
			if (this.config.CustomProfitMargin < 0)
			{
				Logger.Error("Error in config.json: \"CustomProfitMargin\" must be at least 0.");
				return;
			}

			if (this.config.CustomProfitMarginForSpecialOrders < 0)
			{
				Logger.Error("Error in config.json: \"CustomProfitMarginForSpecialOrders\" must be at least 0.");
				return;
			}

			helper.Events.Content.AssetRequested += this.OnAssetRequested;
			helper.Events.GameLoop.DayStarted += this.OnDayStarted;
			helper.Events.Display.MenuChanged += this.OnMenuChanged;
		}

		private void UpdateItemDeliveryQuest(ItemDeliveryQuest quest)
		{
			if (quest.ItemId.Value == null)
			{
				Logger.Trace("Can not adjust reward for daily quest that is managed by Quest Framework.");
				return;
			}

			Item questDeliveryItem = ItemRegistry.Create(quest.ItemId.Value);
			if (questDeliveryItem.sellToStorePrice() == 0)
			{
				Logger.Warn("Quest item '" + questDeliveryItem.Name + "' has no selling price. Reward won't be adjusted.");
				return;
			}

			// item delivery quests don't have a reward property
			// instead, the reward is calculated from the item being requested once the quest has been completed
			// this assumes that the reward is always three times the item value
			int originalReward = questDeliveryItem.sellToStorePrice() * 3;

			int adjustedReward = QuestHelper.GetAdjustedReward(originalReward, this.config);
			QuestHelper.SetReward(quest, adjustedReward);
			QuestHelper.UpdateDescription(quest, originalReward, adjustedReward);
		}

		private void OnDayStarted(object sender, DayStartedEventArgs e)
		{
			// wait for Quest Framework to potentially initialize a quest
			this.Helper.Events.GameLoop.UpdateTicked += this.OnDayStartedDelayed;
		}

		private void OnMenuChanged(object sender, MenuChangedEventArgs e)
		{
			// for item delivery quests, the description and reward would reset when they get completed, so we set it every time it is viewed
			if (e.NewMenu is QuestLog)
			{
				foreach (ItemDeliveryQuest quest in QuestLogHelper.GetDailyItemDeliveryQuests())
				{
					this.UpdateItemDeliveryQuest(quest);
				}
			}
		}

		private void OnDayStartedDelayed(object sender, UpdateTickedEventArgs e)
		{
			this.Helper.Events.GameLoop.UpdateTicked -= this.OnDayStartedDelayed;

			Quest dailyQuest = Game1.questOfTheDay;
			if (dailyQuest == null) return;

			QuestHelper.LoadQuestInfo(dailyQuest);

			if (dailyQuest is ItemDeliveryQuest itemDeliveryQuest)
			{
				this.UpdateItemDeliveryQuest(itemDeliveryQuest);
				return;
			}

			QuestHelper.AdjustRewardImmediately(dailyQuest, this.config);
		}

		private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
		{
			if (e.NameWithoutLocale.IsEquivalentTo("Data/SpecialOrders"))
			{
				var specialOrderMultiplier = this.config.UseProfitMarginForSpecialOrders
				? Game1.player.difficultyModifier
				: this.config.CustomProfitMarginForSpecialOrders;

				e.Edit(questsData =>
				{
					// update monetary rewards for special order quests
					IDictionary<string, SpecialOrderData> quests = questsData.AsDictionary<string, SpecialOrderData>().Data;

					// https://stackoverflow.com/a/31767807
					// .ToList is part of System.Linq
					// Without it, the loop would error after an assignment to a dictionary element
					foreach (KeyValuePair<string, SpecialOrderData> questData in quests)
					{
						SpecialOrderData quest = questData.Value;
						foreach (SpecialOrderRewardData reward in quest.Rewards)
						{
							if (reward.Type != "Money") continue;

							Dictionary<string, string> data = reward.Data;

							if (!data.ContainsKey("Amount")) throw new Exception("Could not get 'Amount' for special order quest.");
							string amount = data["Amount"];

							// amount is dictated by the requested resource with a multiplier
							if (amount.StartsWith("{"))
							{
								// There is actually nothing to do here.
								// The base price is already taking the profit margin into account.
							}

							// reward is a fixed gold amount
							else
							{
								int newAmount = (int)Math.Ceiling(int.Parse(amount) * specialOrderMultiplier);
								data["Amount"] = newAmount.ToString();
							}
						}
					}
				});
			}
		}
	}
}