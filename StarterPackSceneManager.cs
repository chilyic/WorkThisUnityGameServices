using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.Economy;
using UnityEngine;

namespace StarterPack
{
    public class StarterPackSceneManager : MonoBehaviour
    {
        public static event Action<bool> StarterPackStatusChecked;

        private const string k_StarterPackCloudSaveKey = "STARTER_PACK_STATUS";

        public async Task Init()
        {
            try
            {
                await EconomyManager.instance.RefreshEconomyConfiguration();
                if (this == null) return;

                EconomyManager.instance.InitializeVirtualPurchaseLookup();

                await Task.WhenAll(EconomyManager.instance.RefreshCurrencyBalances(),
                    EconomyManager.instance.RefreshInventory(),
                    RefreshStarterPackStatus());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                if (this != null)
                {
                    StarterPackSampleView.instance.SetInteractable();
                }
            }
        }

        public async void OnBuyButtonPressed()
        {
            try
            {
                StarterPackSampleView.instance.SetInteractable(false);

                await CloudCodeManager.instance.CallPurchaseStarterPackEndpoint();
                if (this == null) return;

                await Task.WhenAll(EconomyManager.instance.RefreshCurrencyBalances(),
                    EconomyManager.instance.RefreshInventory(),
                    RefreshStarterPackStatus());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                if (this != null)
                {
                    StarterPackSampleView.instance.SetInteractable();
                }
            }
        }

        public async void OnGiveTenGemsButtonPressed()
        {
            try
            {
                StarterPackSampleView.instance.SetInteractable(false);

                var balanceResponse = await EconomyService.Instance.PlayerBalances.IncrementBalanceAsync("GEM", 10);
                if (this == null) return;

                EconomyManager.instance.SetCurrencyBalance(balanceResponse.CurrencyId, balanceResponse.Balance);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                if (this != null)
                {
                    StarterPackSampleView.instance.SetInteractable();
                }
            }
        }

        public async void OnResetPlayerDataButtonPressed()
        {
            try
            {
                StarterPackSampleView.instance.SetInteractable(false);

                await CloudSaveService.Instance.Data.ForceDeleteAsync(k_StarterPackCloudSaveKey);
                if (this == null) return;

                await RefreshStarterPackStatus();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                if (this != null)
                {
                    StarterPackSampleView.instance.SetInteractable();
                }
            }
        }

        static async Task RefreshStarterPackStatus()
        {
            var starterPackIsClaimed = false;

            try
            {
                var starterPackStatusCloudSaveResult = await CloudSaveService.Instance.Data.LoadAsync(
                    new HashSet<string> { k_StarterPackCloudSaveKey });
                Debug.Log(starterPackStatusCloudSaveResult.Keys);
                
                if (starterPackStatusCloudSaveResult.TryGetValue(k_StarterPackCloudSaveKey, out var result))
                {
                    Debug.Log($"{k_StarterPackCloudSaveKey} value: {result}");

                    if (result.Contains("\"claimed\":true"))
                    {
                        starterPackIsClaimed = true;
                    }
                }
                else
                {
                    Debug.Log($"{k_StarterPackCloudSaveKey} key not set");
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            StarterPackStatusChecked?.Invoke(starterPackIsClaimed);
        }
    }
}
