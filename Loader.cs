using System;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;
using System.Threading;

public class Loader : MonoBehaviour
{
    [SerializeField] private int _delayHidden_Sec = 0;
    [SerializeField] private DailyRewards.DailyRewardsSceneManager _dailyRewards;
    [SerializeField] private VirtualShop.VirtualShopSceneManager _virtualShop;
    [SerializeField] private StarterPack.StarterPackSceneManager _starterPack;
    [SerializeField] private WindowSettings _windowSettings;

    private async void Awake()
    {
        try
        {
            await UnityServices.InitializeAsync();

            // Check that scene has not been unloaded while processing async wait to prevent throw.
            if (this == null) return;

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                if (this == null) return;
            }

            Debug.Log($"Player id:{AuthenticationService.Instance.PlayerId}");

            if (_dailyRewards != null) await _dailyRewards.Init();
            if (_virtualShop != null) await _virtualShop.Init();
            if (_starterPack != null) await _starterPack.Init();

            _windowSettings.SetId(AuthenticationService.Instance.PlayerId);
            _windowSettings.LoadAudioPrefs();
            _windowSettings.LoadLanguages();

            Thread.Sleep(_delayHidden_Sec * 1000);
            gameObject.SetActive(false);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}