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

public class MenuSelection : MonoBehaviour {

	#region Public Data
    public enum MENU_STATE { IDLE = 0, LOADING_BASE, LOADED }
	#endregion


	#region Behaviour Methods
	// Use this for initialization
	void Start () {
        InitSelectionMenu();
	}
	
	// Update is called once per frame
	/*void Update () {
        switch (_state)
        {
            case MENU_STATE.LOADING_BASE:
                if (_asyincOp.isDone)
                {
                    _state = MENU_STATE.LOADED;
                    //Set level values
                    Debug.Log("Setup lvl " + _currentLevelIndex + " . . .");
                    
                }
                break;
        }
	}*/
	#endregion

	#region Public Methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    public void ShowStageAndCenter(int index)
    {
        if (_currentStageIndex == index)
            return;

        //center stage content panel
        //_stageScrollRect.content.Translate(new Vector3(_stageMiniList[index].transform.position.x - _stageCenterRef.transform.position.x, 0f, 0f));
        Debug.Log("Moving index "+index+" : " + _contentPanelInitPos + Vector2.right * _distanceBetweenStageMinis * index);
        _stageScrollRect.content.position =  new Vector2(_contentPanelInitPos.x - _distanceBetweenStageMinis * index, _contentPanelInitPos.y);// new Vector3(transform.position.x + _stageCenterRef.transform.position.x - _stageMiniList[index].transform.position.x, 0f, 0f);// new Vector3(_stageMiniList[index].transform.position.x - _stageCenterRef.transform.position.x, 0f, 0f);
        _stageScrollRect.velocity = Vector2.zero;
        ShowStage(index);
        //switch level panel
        /*if (_currentStageIndex!= -1)
            _stageLevelPanelList[_currentStageIndex].SetActive(false);
        _currentStageIndex = index;
        _stageLevelPanelList[_currentStageIndex].SetActive(true);
        _stageText.text = LocalizationService.Instance.GetTextByKey("loc_stage") + " " + _currentStageIndex.ToString();
        UpdateStageButtons();*/

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    public void ShowStage(int index)
    {
        if (_currentStageIndex == index)

            return;
        //switch level panel
        if (_currentStageIndex != -1)
            _stageLevelPanelList[_currentStageIndex].SetActive(false);
        _currentStageIndex = index;
        _stageLevelPanelList[_currentStageIndex].SetActive(true);
        _stageScrollRect.transform.GetChild(2).GetComponent<Text>().text += " " + (_currentStageIndex + 1).ToString();
        _stageText.text = LocalizationService.Instance.GetTextByKey("loc_stage") + " " + _currentStageIndex.ToString();
        UpdateStageButtons();

    }

    /// <summary>
    /// 
    /// </summary>
    public void CheckScrollStage()
    {
        Debug.Log("...");
        float xDist = Mathf.Infinity;
        float stageDist = -1f;
        int closestStageIndex = -1;

        for (int i = 0; i < _stageMiniList.Count; ++i)
        {
            stageDist = Vector2.Distance(_stageMiniList[i].transform.position, _stageCenterRef.transform.position);
            Debug.Log("Stage dist: " + stageDist);
            if (stageDist < xDist)
            {
                xDist = stageDist;
                closestStageIndex = i;
            }
        }
        Debug.Log("Closest index is: " + closestStageIndex);
        if (_currentStageIndex != closestStageIndex)
            ShowStage(closestStageIndex);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    public void SelectLevel(int index)
    {
        if (_currentLevelIndex == index)
            return;

        _currentLevelIndex = index;
        //TODO: Feedback

        //Play Button
        if (!_playButton.activeSelf)
            _playButton.SetActive(true);
    }
    /// <summary>
    /// 
    /// </summary>
    public void BakckToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void PlayLevel()
    {
        Debug.Log("Play Level");
       // DataMgr.Instance.StageIndex = _currentStageIndex;
        //DataMgr.Instance.LevelIndex = _currentLevelIndex;
        PlayerPrefs.SetInt("Current_Stage", _currentStageIndex);
        PlayerPrefs.SetInt("Current_Level", _currentLevelIndex);
        SceneManager.LoadScene("Game");
        GameMgr.Instance.LoadAndStartCurrentLevel();
   
    }

    /// <summary>
    /// 
    /// </summary>
    public void InitSelectionMenu()
    {
        _currentStageIndex = -1;
        _currentLevelIndex = -1;
        _contentPanelInitPos = _stageScrollRect.content.transform.position;
        SetupLevelList();
        ShowStage(0);
        _playButton.SetActive(false);
        EnableRewardAdsButton(AdsMgr.Instance.RewardAdsReady);
        //TODO: reset stage buttons

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
	#endregion


	#region Private Methods
    /// <summary>
    /// 
    /// </summary>
    private void SetupLevelList()
    {
        Debug.Log("________________________________Setup Level List");
        _stageList = GameMgr.Instance._StageList;

        for (int i = 0; i < _stageList.Count; ++i)
        {
            for (int j = 0; j < _stageList[i].GetLevelList().Count; ++j)
            {
                switch (_stageList[i].GetLevelList()[j].GetState())
                {
                    case Level.AVAILABILITY_STATE.UNLOCKED:
                        _stageLevelPanelList[i].transform.GetChild(j).GetComponent<Image>().color = Color.white;
                        _stageLevelPanelList[i].transform.GetChild(j).GetComponent<Button>().interactable = true;
                        break;

                    case Level.AVAILABILITY_STATE.LOCKED:
                        _stageLevelPanelList[i].transform.GetChild(j).GetComponent<Image>().color = Color.red;
                        _stageLevelPanelList[i].transform.GetChild(j).GetComponent<Button>().interactable = false;
                        break;

                    case Level.AVAILABILITY_STATE.COMPLETED:
                        _stageLevelPanelList[i].transform.GetChild(j).GetComponent<Image>().color = Color.green;
                        _stageLevelPanelList[i].transform.GetChild(j).GetComponent<Button>().interactable = true;
                        break;

                    case Level.AVAILABILITY_STATE.UNAVAILABLE:
                        _stageLevelPanelList[i].transform.GetChild(j).GetComponent<Image>().color = Color.black;
                        _stageLevelPanelList[i].transform.GetChild(j).GetComponent<Button>().interactable = false;
                        break;
                }
                _stageLevelPanelList[i].transform.GetChild(j).GetChild(1).GetComponentInChildren<Text>().text = _stageList[i].GetLevelList()[j].GetMaxScore().ToString("0");
                //Set fruit types on miniature
                _auxfruitTypeList = _stageList[i].GetLevelList()[j].GetFruitTypeSpawnList();
                for (int k = 0; k < _auxfruitTypeList.Count; ++k)
                {
                    if (_auxfruitTypeList[k].FruitTypeIndex != (int)Fruit.F_TYPE.GOLD_ITEM)
                    {
                        _stageLevelPanelList[i].transform.GetChild(j).GetChild(2).GetChild(_auxLvlMiniFruitListIndex).GetComponent<Image>().sprite = _spriteFruitList[(int)_auxfruitTypeList[k].FruitTypeIndex];
                        _stageLevelPanelList[i].transform.GetChild(j).GetChild(2).GetChild(_auxLvlMiniFruitListIndex).gameObject.SetActive(true);
                        ++_auxLvlMiniFruitListIndex;
                    }

                }
                for (int k = _auxLvlMiniFruitListIndex; k < _stageLevelPanelList[i].transform.GetChild(j).GetChild(2).childCount; ++k)
                    _stageLevelPanelList[i].transform.GetChild(j).GetChild(2).GetChild(k).gameObject.SetActive(false);
                //reset iindex for next lvl iteration
                _auxLvlMiniFruitListIndex = 0;
                //text
                _stageLevelPanelList[i].transform.GetChild(j).GetChild(0).GetComponent<Text>().text = string.Format(LocalizationService.Instance.GetTextByKey("loc_level") + (j+1).ToString());

            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateStageButtons()
    {
        for (int i = 0; i < _stageSelectionButtonList.Count; ++i)
        {
            if (_currentStageIndex == i)
            {
                _stageSelectionButtonList[i].GetComponent<RectTransform>().sizeDelta = _selectedStageButtonScale;
            }
            else
            {
                _stageSelectionButtonList[i].GetComponent<RectTransform>().sizeDelta = _unselectedStageButtonScale;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="enable"></param>
    private void EnableRewardAdsButton(bool enable)
    {
        _rewardAdsButton.interactable = enable;
        _rewardAdsButton.GetComponent<Outline>().enabled = enable;

    }
    #endregion


    #region Properties
    public List<Stage> _StageList { get { return _stageList; } set { _stageList = value; } }
	#endregion

	#region Private Serialized Fields
    [SerializeField]
    private ScrollRect _stageScrollRect;
    [SerializeField]
    private Transform _stageCenterRef;      //reference position to freeze selected stage
    [SerializeField]
    private List<GameObject>_stageMiniList;
    [SerializeField]
    private List<GameObject> _stageLevelPanelList;
    [SerializeField]
    private List<Button> _stageSelectionButtonList;
    [SerializeField]
    private GameObject _playButton;
    [SerializeField]
    private List<Stage> _stageList;
    [SerializeField]
    private float _distanceBetweenStageMinis;   //distance between stage miniature; used to seek content panelon stage select
    [SerializeField]
    private Vector2 _selectedStageButtonScale, _unselectedStageButtonScale;
    [SerializeField]
    private List<Sprite> _spriteFruitList;      //used to set lvl miniatures fruits

    [SerializeField]
    private ShopMenu _shopScreen;
    [SerializeField]
    private InventoryScreen _invScreen;
    [SerializeField]
    private GameObject _adPopup;
    [SerializeField]
    private Text _stageText;

    [SerializeField]
    private Button _rewardAdsButton;
	#endregion

	#region Private Non-serialized Fields
    private AsyncOperation _asyincOp;
    private MENU_STATE _state;

    private int _currentStageIndex;
    private int _currentLevelIndex;
    private Vector2 _contentPanelInitPos;

    private List<Level.FruitSpawn> _auxfruitTypeList;
    private int _auxLvlMiniFruitListIndex;      //index used to track the fruit list over the level miniature on setup
	#endregion
}
