/************************************************************************/
/* @Author: Author Name
 * @Date: Date
 * @Brief: BBBrief
 * @Description: DDDescription
 * ***********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class AnalyticsMgr : MonoBehaviour {

	#region Public Data
    public static AnalyticsMgr Instance;
	#endregion


	#region Behaviour Methods
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);
        else
            DontDestroyOnLoad(this);
    }
    void Start()
    {
        _eventTimer = 0f;
    }
	// Update is called once per frame
	void Update () {
        _eventTimer += Time.deltaTime;
        if (_eventTimer >= _eventPushTimeInterval)
            PushAds();
	}
	#endregion

	#region Public Methods
    /// <summary>
    /// 
    /// </summary>
    public void PushAds()
    {
        Analytics.CustomEvent("Large_Ads", new Dictionary<string, object>
        {
            { "attempts", _adsAttemptCount },
            { "shown", _adsShownCount },
            { "attempts_on_exit", _adsAttemptOnExitCount },
            { "shown_on_exit", _adsShownOnExitCount },
            { "skipped", _adsSkippedCount },
            { "failed", _adsFailedCount }
        });
    }
	#endregion


	#region Private Methods

	#endregion


	#region Properties
    public int AdsAttemptCount { get { return _adsAttemptCount; } set { _adsAttemptCount = value; } }
    public int AdsShownCount { get { return _adsShownCount; } set { _adsShownCount = value; } }
    public int AdsAttemptOnExitCount { get { return _adsAttemptOnExitCount; } set { _adsAttemptOnExitCount = value; } }
    public int AdsShownOnExitCount { get { return _adsShownOnExitCount; } set { _adsShownOnExitCount = value; } }
    public int AdsSkippedCount { get { return _adsSkippedCount; } set { _adsSkippedCount = value; } }
    public int AdsFailedCount { get { return _adsFailedCount; } set { _adsFailedCount = value; } }
    public int AdsFinishedCount { get { return _adsFinishedCount; } set { _adsFinishedCount = value; } }
    public int RewarAdsAttemptCount { get { return _rewarAdsAttemptCount; } set { _rewarAdsAttemptCount = value; } }
    public int RewardAdsShownCount { get { return _rewardAdsShownCount; } set { _rewardAdsShownCount = value; } }
    public int RewardAdsSkippedCount { get { return _rewardAdsSkippedCount; } set { _rewardAdsSkippedCount = value; } }
    public int RewardAdsFailedCount { get { return _rewardAdsFailedCount; } set { _rewardAdsFailedCount = value; } }
    public int RewardAdsFinishedCount { get { return _rewardAdsFinishedCount; } set { _rewardAdsFinishedCount = value; } }
	#endregion

	#region Private Serialized Fields
    [SerializeField]
    private float _eventPushTimeInterval;
	#endregion

	#region Private Non-serialized Fields
    private float _eventTimer;

    private int _adsAttemptCount, _adsShownCount;
    private int _rewarAdsAttemptCount, _rewardAdsShownCount;
    private int _adsAttemptOnExitCount, _adsShownOnExitCount;
    private int _adsSkippedCount, _adsFailedCount, _adsFinishedCount; //ad result state
    private int _rewardAdsSkippedCount, _rewardAdsFailedCount, _rewardAdsFinishedCount;

	#endregion
}
