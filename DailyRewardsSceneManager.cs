using System;
using System.Threading.Tasks;
using UnityEngine;

namespace DailyRewards
{
    public class DailyRewardsSceneManager : MonoBehaviour
    {
        public DailyRewardsSampleView sceneView;
        private DailyRewardsEventManager eventManager;

        void Awake()
        {
            eventManager = GetComponent<DailyRewardsEventManager>();
        }

        public async Task Init()
        {
            try
            {
                await EconomyManager.instance.RefreshEconomyConfiguration();
                if (this == null) return;

                await Task.WhenAll(
                    EconomyManager.instance.FetchCurrencySprites(),
                    EconomyManager.instance.RefreshCurrencyBalances(),
                    eventManager.RefreshDailyRewardsEventStatus()
                );
                if (this == null) return;

                Debug.Log("Initialization and signin complete.");

                if (eventManager.isEnded)
                {
                    await eventManager.Demonstration_StartNextMonth();
                    if (this == null) return;
                }

                ShowStatus();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        void Update()
        {
            if (!eventManager.isEventReady)
            {
                return;
            }

            if (eventManager.isStarted && !eventManager.isEnded)
            {
                if (eventManager.UpdateRewardsStatus(sceneView))
                {
                    sceneView.UpdateStatus(eventManager);
                }
                else
                {
                    sceneView.UpdateTimers(eventManager);
                }
            }
        }

        void ShowStatus()
        {
            sceneView.UpdateStatus(eventManager);

            if (eventManager.firstVisit)
            {
                eventManager.MarkFirstVisitComplete();
            }
        }

        public async void OnClaimButtonPressed()
        {
            Debug.Log(eventManager.totalCalendarDays);
            try
            {
                sceneView.SetAllDaysUnclaimable();

                await eventManager.ClaimDailyReward();
                if (this == null) return;

                ShowStatus();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public async void OnOpenEventButtonPressed()
        {
            try
            {
                if (!eventManager.isEventReady)
                {
                    return;
                }

                if (eventManager.isEnded)
                {
                    await eventManager.Demonstration_StartNextMonth();
                    if (this == null) return;
                }

                ShowStatus();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
