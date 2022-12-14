using Unity.Services.Core;
using Unity.Services.Mediation;
using UnityEngine;
using System;
using UnityEngine.UI;

namespace SuperUltra.Container
{

    public class AdManager : MonoBehaviour
    {
        [Header("Ad Unit Ids"), Tooltip("Ad Unit Ids for each platform that represent Mediation waterfalls.")]
        public string androidAdUnitId;
        [Tooltip("Ad Unit Ids for each platform that represent Mediation waterfalls.")]
        public string iosAdUnitId;
        public static AdManager _instance;
        public static Action<bool> _rewardAction;
        IRewardedAd m_RewardedAd;
        public Button[] _adbuttons;
        bool _started = false;

        void OnEnable()
        {
            ContainerInterface.OnRequestRewardedAds += OnRequestRewardedAds;
        }

        void OnDisable()
        {
            ContainerInterface.OnRequestRewardedAds -= OnRequestRewardedAds;
        }

        async void Start()
        {
            if (_instance != null)
            {
                Destroy(this.gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(this.gameObject);

            try
            {
                Debug.Log("Initializing...");
                await UnityServices.InitializeAsync();
                Debug.Log("Initialized!");
                InitializationComplete();
            }
            catch (Exception e)
            {
                InitializationFailed(e);
            }
        }

        public async void Initialize()
        {
            try
            {
                Debug.Log("Initializing...");
                await UnityServices.InitializeAsync();
                Debug.Log("Initialized!");
                InitializationComplete();
            }
            catch (Exception e)
            {
                InitializationFailed(e);
            }
        }

        void OnDestroy()
        {
            m_RewardedAd?.Dispose();
        }

        public async void ShowRewarded()
        {
            if(m_RewardedAd != null)
            {
                Debug.Log("ShowRewarded AdState " + m_RewardedAd.AdState);
            }
            if (m_RewardedAd?.AdState == AdState.Loaded)
            {
                try
                {
                    RewardedAdShowOptions showOptions = new RewardedAdShowOptions();
                    showOptions.AutoReload = true;
                    await m_RewardedAd.ShowAsync(showOptions);
                    Debug.Log("Rewarded Shown!");
                }
                catch (ShowFailedException e)
                {
                    Debug.LogWarning($"Rewarded failed to show: {e.Message}");
                    _rewardAction?.Invoke(false);
                }
            }
        }

        public async void ShowRewardedWithOptions()
        {
            if (m_RewardedAd?.AdState == AdState.Loaded)
            {
                try
                {
                    //Here we provide a user id and custom data for server to server validation.
                    RewardedAdShowOptions showOptions = new RewardedAdShowOptions();
                    showOptions.AutoReload = true;
                    S2SRedeemData s2SData;
                    s2SData.UserId = "my cool user id";
                    s2SData.CustomData = "{\"reward\":\"Gems\",\"amount\":20}";
                    showOptions.S2SData = s2SData;

                    await m_RewardedAd.ShowAsync(showOptions);
                    Debug.Log("Rewarded Shown!");
                }
                catch (ShowFailedException e)
                {
                    Debug.LogWarning($"Rewarded failed to show: {e.Message}");
                }
            }
        }

        async void LoadAd()
        {
            try
            {
                await m_RewardedAd.LoadAsync();
            }
            catch (LoadFailedException)
            {
                // We will handle the failure in the OnFailedLoad callback
            }
        }

        void OnRequestRewardedAds(Action<bool> callback)
        {
            Debug.Log("admanager OnRequestRewardedAds");
            _rewardAction = callback;
            ShowRewarded();
        }

        void InitializationComplete()
        {
            // Impression Event
            MediationService.Instance.ImpressionEventPublisher.OnImpression += ImpressionEvent;

            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    m_RewardedAd = MediationService.Instance.CreateRewardedAd(androidAdUnitId);
                    break;

                case RuntimePlatform.IPhonePlayer:
                    m_RewardedAd = MediationService.Instance.CreateRewardedAd(iosAdUnitId);
                    break;
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.LinuxEditor:
                    m_RewardedAd = MediationService.Instance.CreateRewardedAd(!string.IsNullOrEmpty(androidAdUnitId) ? androidAdUnitId : iosAdUnitId);
                    break;
                default:
                    Debug.LogWarning("Mediation service is not available for this platform:" + Application.platform);
                    return;
            }

            // Load Events
            m_RewardedAd.OnLoaded += AdLoaded;
            m_RewardedAd.OnFailedLoad += AdFailedLoad;

            // Show Events
            m_RewardedAd.OnUserRewarded += UserRewarded;
            m_RewardedAd.OnClosed += AdClosed;

            Debug.Log($"Initialized On Start. Loading Ad...");

            LoadAd();
        }

        void InitializationFailed(Exception error)
        {
            SdkInitializationError initializationError = SdkInitializationError.Unknown;
            if (error is InitializeFailedException initializeFailedException)
            {
                initializationError = initializeFailedException.initializationError;
            }
            Debug.Log($"Initialization Failed: {initializationError}:{error.Message}");
        }

        void UserRewarded(object sender, RewardEventArgs e)
        {
            Debug.Log($"User Rewarded! Type: {e.Type} Amount: {e.Amount}");
            _rewardAction?.Invoke(true);
            _rewardAction = null;
        }

        void AdClosed(object sender, EventArgs args)
        {
            Debug.Log("Rewarded Closed! Loading Ad...");
        }

        void AdLoaded(object sender, EventArgs e)
        {
            Debug.Log("Ad loaded");
            /**
            if (_started == false)
            {
                for (int i = 0; i<_adbuttons.Length; i++)
                {
                    _adbuttons[i].interactable = true;
                }
                _started = true;
            }
            **/

        }

        void AdFailedLoad(object sender, LoadErrorEventArgs e)
        {
            Debug.Log("Failed to load ad " + e.Message);
            _rewardAction?.Invoke(false);
            _rewardAction = null;
            LoadAd();
        }

        void ImpressionEvent(object sender, ImpressionEventArgs args)
        {
            var impressionData = args.ImpressionData != null ? JsonUtility.ToJson(args.ImpressionData, true) : "null";
            Debug.Log($"Impression event from ad unit id {args.AdUnitId} : {impressionData}");
        }

    }

}

