using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Economy;
using Unity.Services.Economy.Model;
using UnityEngine;

namespace VirtualShop
{
    public class VirtualShopSceneManager : MonoBehaviour
    {
        public VirtualShopSampleView virtualShopSampleView;
        
        private const int k_EconomyPurchaseCostsNotMetStatusCode = 10504;
        private List<VirtualShopCategory> _categories = new();

        public async Task Init()
        {
            try
            {
                await EconomyManager.instance.RefreshEconomyConfiguration();
                if (this == null) return;

                EconomyManager.instance.InitializeVirtualPurchaseLookup();

                await Task.WhenAll(AddressablesManager.instance.PreloadAllEconomySprites(),
                    RemoteConfigManager.instance.FetchConfigs(),
                    EconomyManager.instance.RefreshCurrencyBalances());
                if (this == null) return;

                await AddressablesManager.instance.PreloadAllShopBadgeSprites(
                    RemoteConfigManager.instance.virtualShopConfig.categories);

                VirtualShopManager.instance.Initialize();

                foreach (var cat in RemoteConfigManager.instance.virtualShopConfig.categories)
                {
                    var firstCategoryId = cat.id;
                    if (!VirtualShopManager.instance.virtualShopCategories.TryGetValue(
                        firstCategoryId, out var category))
                    {
                        throw new KeyNotFoundException($"Unable to find shop category {firstCategoryId}.");
                    }
                    _categories.Add(category);
                }                

                virtualShopSampleView.ShowCategory(_categories);

                Debug.Log("Initialization and sign in complete.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public async Task OnPurchaseClicked(VirtualShopItem virtualShopItem)
        {
            try
            {
                var result = await EconomyManager.instance.MakeVirtualPurchaseAsync(virtualShopItem.id);
                if (this == null) return;

                await EconomyManager.instance.RefreshCurrencyBalances();
                if (this == null) return;

                ShowRewardPopup(result.Rewards);
            }
            catch (EconomyException e)
            when (e.ErrorCode == k_EconomyPurchaseCostsNotMetStatusCode)
            {
                virtualShopSampleView.ShowVirtualPurchaseFailedErrorPopup();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public async void OnGainCurrencyDebugButtonClicked()
        {
            try
            {
                await EconomyManager.instance.GrantDebugCurrency("GEM", 30);
                if (this == null) return;

                await EconomyManager.instance.RefreshCurrencyBalances();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        void ShowRewardPopup(Rewards rewards)
        {
            var addressablesManager = AddressablesManager.instance;

            var rewardDetails = new List<RewardDetail>();
            foreach (var inventoryReward in rewards.Inventory)
            {
                rewardDetails.Add(new RewardDetail()
                {
                    id = inventoryReward.Id,
                    quantity = inventoryReward.Amount,
                    sprite = addressablesManager.preloadedSpritesByEconomyId[inventoryReward.Id]
                });
            }

            foreach (var currencyReward in rewards.Currency)
            {
                rewardDetails.Add(new RewardDetail()
                {
                    id = currencyReward.Id,
                    quantity = currencyReward.Amount,
                    sprite = addressablesManager.preloadedSpritesByEconomyId[currencyReward.Id]
                });
            }

            virtualShopSampleView.ShowRewardPopup(rewardDetails);
        }
    }
}
