/************************************************************************/
/* @Author: Author Name
 * @Date: Date
 * @Brief: BBBrief
 * @Description: DDDescription
 * ***********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SunCubeStudio.Localization;

public class UIHelper : MonoBehaviour {

	#region Public Data
    public static UIHelper Instance;
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

	// Use this for initialization
	void Start () {
        _targetBarValue = 0f;
        _barGrowing = false;
        _initGlowAlpha = _glowSprite.color.a;
        _paused = false;
        _showLevelTime = true;
	}
	
	// Update is called once per frame
	void Update () {
        if (_barGrowing)
        {
            _barTimer += Time.deltaTime;
            _alarmSbar.value = _initBarValue + (_targetBarValue-_initBarValue) *_growCurve.Evaluate(_barTimer / _barGrowTime);
            //_alarmSbar.value = Mathf.Lerp(_initBarValue, _targetBarValue, _growCurve.Evaluate(_barTimer / _barGrowTime));
            if (_barTimer >= _barGrowTime)
                _barGrowing = false;
        }
        if (glowEnabled)
        {
            _barGlowTimer += Time.deltaTime;
            _glowSprite.color = new Color(_glowSprite.color.r, _glowSprite.color.g, _glowSprite.color.b, Mathf.Lerp(_initGlowAlpha, _endGlowAlpha, _barGlowTimer / _currentFadeTime));
            if (_barGlowTimer >= _currentFadeTime)
            {
                _auxAlpha = _endGlowAlpha;
                _endGlowAlpha = _initGlowAlpha;
                _initGlowAlpha = _auxAlpha;
                _barGlowTimer = 0f;
            }

        }
	}
	#endregion

	#region Public Methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="time"></param>
    public void SetLvlScreenTime(float time)
    {
        if (time < 0f)
            time = 0f;
        _timeText.text = time.ToString("00");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="show"></param>
    public void ShowShop(bool show)
    {
        if (show)
            _shopScreen.InitShop();
        _shopScreen.gameObject.SetActive(show);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="show"></param>
    public void ShowInventory(bool show)
    {

        if (show)
            _invScreen.InitScreen();
        _invScreen.gameObject.SetActive(show);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="show"></param>
    public void ShowAdRewardPopup(bool show)
    {
        _adPopup.SetActive(show);
    }

    public void ShowRewardAd()
    {
        AdsMgr.Instance.ShowRewardAd();
        ShowAdRewardPopup(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ShowLevelFinishedScreen()
    {
        _lvlFinishedScr.SetupMenu();
        _lvlFinishedScr.gameObject.SetActive(true);
        _lvlFinishedScr.StartScreen();
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetAlarmLevel()
    {

        //_currentAlarmLvl = 0;
        _alarmText.text = GameMgr.Instance.CurrentAlarmLevel.ToString("0") + " / " + GameMgr.Instance.MaxAlarmLevel;
        _initBarValue = _targetBarValue;
        _targetBarValue = GameMgr.Instance.CurrentAlarmLevel / GameMgr.Instance.MaxAlarmLevel;
        if (_targetBarValue >= 0.5f)
        {
            if (_targetBarValue >= 0.75f)
                _currentFadeTime = _glowFadeTime_75pc;
            else
                _currentFadeTime = _glowFadeTime_50pc;
            
            if (!glowEnabled)
            {
                glowEnabled = true;
                _barGlowTimer = 0f;
                _glowSprite.gameObject.SetActive(true);
                _endGlowAlpha = 0f;
            }
        }

        _barGrowing = true;
        _barTimer = 0f;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="val"></param>
    public void AlarmDepleted(float val)
    {
        if (_barGrowing)
            _targetBarValue -= val;
        else
            _alarmSbar.value -= val;
        _alarmText.text = GameMgr.Instance.CurrentAlarmLevel.ToString("0") + " / " + GameMgr.Instance.MaxAlarmLevel; 
        if (glowEnabled && _targetBarValue < 0.5f)
            glowEnabled = false;
        else if (glowEnabled && _targetBarValue < 0.75f)
        {
            _currentFadeTime = _glowFadeTime_50pc;
        }
        
    }
    public void ResetAlarmUI()
    {
        glowEnabled = false;
        _glowSprite.gameObject.SetActive(false);
    }
    public void SetLevelText(int level)
    {
        _levelText.text = LocalizationService.Instance.GetTextByKey("loc_level") +" "+ (level+1).ToString();
    }

    public void ShowLoseScreen(bool show)
    {
        _loseScreen.SetActive(show);
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("LevelSelection");
    }

    public void Pause()
    {
        _paused = !_paused;
        GameMgr.Instance.Pause(_paused);
    }

    public void ShowLevelTime(bool show)
    {
        _showLevelTime = show;
        _timeText.gameObject.SetActive(show);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fromLoseScreen"></param>
    public void Retry(bool fromLoseScreen)
    {
        GameMgr.Instance.StartCurrentLevel();
        if (fromLoseScreen)
            ShowLoseScreen(false);
        else
            _lvlFinishedScr.gameObject.SetActive(false);
    }


    #endregion


    #region Private Methods

    #endregion


    #region Properties
    public Text ScoreText { get { return _scoreText; } set { _scoreText = value; } }
    public bool _ShowLevelTime { get { return _showLevelTime; } set { _showLevelTime = value; } }
	#endregion

	#region Private Serialized Fields
    [SerializeField]
    private InventoryScreen _invScreen;
    [SerializeField]
    private ShopMenu _shopScreen;
    [SerializeField]
    private LevelFinishedScreen _lvlFinishedScr;
    [SerializeField]
    private GameObject _loseScreen;

    [SerializeField]
    private Text _alarmText;
    [SerializeField]
    private Text _levelText;
    [SerializeField]
    private Text _scoreText;
    [SerializeField]
    private Text _timeText;
    [SerializeField]
    private GameObject _adPopup;
    [SerializeField]
    private Scrollbar _alarmSbar;
    [SerializeField]
    private float _barGrowTime;
    [SerializeField]
    private AnimationCurve _growCurve;
    [SerializeField]
    private Image _glowSprite;
    [SerializeField]
    private float _glowFadeTime_50pc, _glowFadeTime_75pc;
	#endregion

	#region Private Non-serialized Fields
    private float _initBarValue, _targetBarValue;   //used to lerp bar values
    private float _barTimer;
    private float _barGlowTimer, _currentFadeTime;
    private float _initGlowAlpha, _endGlowAlpha;
    private float _auxAlpha;
    private bool _barGrowing, glowEnabled;
    private bool _paused;
    private bool _showLevelTime;
	#endregion
}
