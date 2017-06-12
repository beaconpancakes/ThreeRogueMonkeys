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

public class LevelFinishedScreen : MonoBehaviour {

	#region Public Data
    /// <summary>
    /// Ratios over S rank score
    /// </summary>
    public const float _rankARatio = .9f;
    public const float _rankBRatio = .8f;
    public const float _rankCRatio = .7f;
    public const float _rankDRatio = .6f;
    public const float _rankERatio = .5f;
    public const float _rankFRatio = 0f;

    public enum LF_MENU_STATE { IDLE = 0, COUNTING, RANK, FINISHED }

    public class FruitIndex
    {
        public Fruit.F_TYPE _Ftype;
        public int _Index;       //index in image list
        public int _Count;

        public FruitIndex(Fruit.F_TYPE t, int index)
        {
            _Ftype = t;
            this._Index = index;
            _Count = 1;
        }
    }
	#endregion


	#region Behaviour Methods
	// Use this for initialization
	void Start () {
        _fruitIndex = new List<FruitIndex>();
        EnableIconButtons(false);
	}
	
	// Update is called once per frame
	void Update () {
        switch (_state)
        {
            case LF_MENU_STATE.IDLE:

                break;

            case LF_MENU_STATE.COUNTING:
                _timer += Time.deltaTime;
                if (_timer >= _countingTimePerFruit)
                {
                    _timer = 0f;
                    //Item
                    if (GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._Ftype == Fruit.F_TYPE.EQUIPMENT ||
                        GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._Ftype == Fruit.F_TYPE.GOLD_ITEM)
                    {
                        _itemImageList[_collectedItemIndex].sprite = GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex].GetFruitSprite();
                        _itemImageList[_collectedItemIndex].gameObject.SetActive(true);
                        _tempGold += GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex].GetGold();
                        _goldText.text = _tempGold.ToString();
                        ++_collectedItemIndex;
                        if (!_goldIcon.activeSelf)
                            _goldIcon.SetActive(true);
                        
                    }
                    else//Fruit
                    {
                        _temp = _fruitIndex.Find((fi) => (fi._Ftype == GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._Ftype));

                        //add this fruit as new list element
                        if (_temp == null)
                        {
                            _fruitIndex.Add(new FruitIndex(GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex]._Ftype, _maxFruitDisplayedIndex));

                            _fruitImageList[_maxFruitDisplayedIndex].sprite = GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex].GetFruitSprite();
                            _fruitTextCountList[_maxFruitDisplayedIndex].text = "1";
                            _fruitImageList[_maxFruitDisplayedIndex].gameObject.SetActive(true);
                            ++_maxFruitDisplayedIndex;
                        }
                        //increment count if fruit is already set on UI
                        else
                        {
                            //Add one element to fruit count and update screen counter
                            ++_temp._Count;
                            Debug.Log("Coun augmented so: "+_temp._Count);
                            _fruitTextCountList[_temp._Index].text = _temp._Count.ToString("0");
                        }
                        _tempScore += GameMgr.Instance.StageCollectedFruits[_collectedFruitIndex].GetScore();
                        _scoreText.text = _tempScore.ToString("0");

                        
                    }
                    //Update collected index and check if list ended to show rank
                    ++_collectedFruitIndex;
                    //End count
                    if (_collectedFruitIndex == GameMgr.Instance.StageCollectedFruits.Count)
                    {
                        _state = LF_MENU_STATE.RANK;
                        _timer = 0f;

                        //TODO: Rank animation
                        _rankLetterImg.sprite = GetRankSpriteLetter();
                        _rankLetterImg.gameObject.SetActive(true);
                        if (GameMgr.Instance.GetCurrentLevel().CheckForHighScore(_tempScore))
                        {
                            //TODO: new score feedback
                        }
                    }
                }
                break;

            case LF_MENU_STATE.RANK:
                _timer += Time.deltaTime;
                if (_timer >= _rankShowTime)
                {
                    //Add gold
                    GameMgr.Instance.AddGold(_tempGold);
                    GameMgr.Instance.SaveProgress();
                    _state = LF_MENU_STATE.FINISHED;
                    _nextButton.SetActive(true);

                    EnableIconButtons(true);
                }
                break;

            case LF_MENU_STATE.FINISHED:
                
                
                break;
        }
	}
	#endregion

	#region Public Methods
    /// <summary>
    /// 
    /// </summary>
    public void SetupMenu()
    {
        if (_fruitIndex == null)
            _fruitIndex = new List<FruitIndex>();
        else
            if (_fruitIndex.Count != 0)
                _fruitIndex.Clear();
        _state = LF_MENU_STATE.IDLE;
        EnableIconButtons(false);
        foreach (Image fI in _fruitImageList)
            fI.gameObject.SetActive(false);
        foreach (Text t in _fruitTextCountList)
            t.text = "0";
        foreach (Image _itemImg in _itemImageList)
            _itemImg.gameObject.SetActive(false);

        _goldText.text = "";

    }

    /// <summary>
    /// 
    /// </summary>
    public void StartScreen()
    {
        Debug.Log("tartscreeen");
        if (GameMgr.Instance.StageCollectedFruits.Count == 0)
            _state = LF_MENU_STATE.RANK;
        else
            _state = LF_MENU_STATE.COUNTING;
        _timer = 0f;
        _temp = null;
        _tempScore = 0;
        _tempGold = 0;
        _goldText.text = "";
        _goldIcon.SetActive(false);
        _stageFinishedText.gameObject.SetActive(false);
        _nextButton.SetActive(false);
        _maxFruitDisplayedIndex = 0;
        _collectedFruitIndex = 0;
        _collectedItemIndex = 0;
        _countingTimePerFruit = _countingTime/GameMgr.Instance.StageCollectedFruits.Count;
    }


    public void EnableIconButtons(bool enable)
    {
        foreach (Button b in _iconButtonList)
            b.interactable = enable;
        
    }

    public void GoToNextLevel()
    {
        
        GameMgr.Instance.LoadAndStartNextLevel();
        gameObject.SetActive(false);
    }

    public void GoToMenu()
    {

    }
	#endregion


	#region Private Methods
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private Sprite GetRankSpriteLetter()
    {
        float currentScoreRatio = (GameMgr.Instance.Score / GameMgr.Instance.GetCurrentLevel().MinRankSuperScore) - GameMgr.Instance.CurrentAlarmLevel * GameMgr.Instance.GetCurrentLevel().PenaltyPerAlarmPt;
        if (currentScoreRatio >= 1f)
            return _rankLetterSpriteList[0];
        else if (currentScoreRatio >= _rankARatio)
            return _rankLetterSpriteList[1];
        else if (currentScoreRatio >= _rankBRatio)
            return _rankLetterSpriteList[2];
        else if (currentScoreRatio >= _rankCRatio)
            return _rankLetterSpriteList[3];
        if (currentScoreRatio >= _rankDRatio)
            return _rankLetterSpriteList[4];
        if (currentScoreRatio >= _rankERatio)
            return _rankLetterSpriteList[5];
        if (currentScoreRatio >= _rankFRatio)
            return _rankLetterSpriteList[6];
        else
        {
            Debug.LogError("Error, score < 0");
            return null;
        }
        
    }
    #endregion


    #region Properties

    #endregion

    #region Private Serialized Fields
    [SerializeField]
    private List<Image> _fruitImageList;
    [SerializeField]
    private List<Text> _fruitTextCountList;
    [SerializeField]
    private List<Image> _itemImageList;
    [SerializeField]
    private Image _rankLetterImg;
    [SerializeField]
    private Text _scoreText;

    [SerializeField]
    private Text _stageFinishedText;        //displayed when last stage level has finished
    [SerializeField]
    private GameObject _retryButton, _nextButton;
    [SerializeField]
    private List<Button> _iconButtonList;

    [SerializeField]
    private float _countingTime;
    [SerializeField]
    private float _rankShowTime;

    [SerializeField]
    private GameObject _goldIcon;
    [SerializeField]
    private Text _goldText;
    [SerializeField]
    private List<Sprite> _rankLetterSpriteList;
	#endregion

	#region Private Non-serialized Fields
    private LF_MENU_STATE _state;

    private float _timer;
    private float _countingTimePerFruit;
    private int _maxFruitDisplayedIndex;     //determines the last fruit type shown on collected fruit list
    private int _collectedFruitIndex;       //index sued to go through level collected uirt list
    private List<FruitIndex> _fruitIndex;
    private int _collectedItemIndex;

    private FruitIndex _temp;       //used to go through list
    private int _tempScore;
    private int _tempGold;
	#endregion
}
