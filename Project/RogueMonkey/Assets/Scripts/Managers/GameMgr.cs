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

public class GameMgr : MonoBehaviour {

	#region Public Data
    public static GameMgr Instance;

    public enum GAME_STATE { IDLE = 0, LOADING_LEVEL, RUN_LEVEL, WAITING_FOR_GUARD, WAITING_MONKEYS_FLEE, LEVEL_ENDED }

    public const float _monkeysFleeTime = 2f;
    public const long _missFruitVibrationTime = 300;
    public const long _loseVirationTime = 1000;

    public delegate void OnAlarmRaisedEvt();


    public event OnAlarmRaisedEvt AlarmRaisedEvt;

	#endregion


	#region Behaviour Methods
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        
    }
	// Use this for initialization
	void Start () {
        
        
        _rightFingerTouch.fingerId = -1;
        _leftFingerTouch.fingerId = -1;
        Input.multiTouchEnabled = true;
        
        
        _stageCollectedFruits = new List<Fruit>();
        _gameState = GAME_STATE.IDLE;

        //_invScreen = GameObject.FindGameObjectWithTag("InventoryScreen").GetComponent<InventoryScreen>();
        //_shopScreen = GameObject.FindGameObjectWithTag("ShopScreen").GetComponent<ShopMenu>(); ;
        
        //GameMgr.Instance.SetLevelIndex(_currentLevelIndex);
        
        
        //_cocoFlyTime = .5f;
        //_gravity = -9.8f;
	}
	
	// Update is called once per frame
	void Update ()
    {
       
        switch (_gameState)
        {
            case GAME_STATE.IDLE:

                break;

            case GAME_STATE.LOADING_LEVEL:
                /*if (_levelLoaded.isDone)
                {
                    
                }*/
                break;
            case GAME_STATE.RUN_LEVEL:
#if UNITY_EDITOR
                if (Input.GetMouseButtonDown(0))
                {
                    _strikerMonkey.MoveToHit(GameCamera.ScreenToWorldPoint(Input.mousePosition));
                }
                if (Input.GetAxis("Horizontal") != 0f)
                {
                    _collectorMonkey.transform.position += Input.GetAxis("Horizontal")*Vector3.right*700f*Time.deltaTime;
                }
                if (Input.GetKeyDown(KeyCode.Space))
                   _collectorMonkey._Sack.Reload(); //collect sack fruits
#endif

#region Touch management
                foreach (Touch t in Input.touches)
                {
                    Debug.Log("Touch::: " + t.fingerId);
                    switch (t.phase)
                    {
                        case TouchPhase.Began:

                            //Check either rightFinger isnt init or it has ended
                            if (//_rightFingerTouch.fingerId == -1 
                                /*|| (_rightFingerTouch.fingerId == t.fingerId))*/
                                /*&&*/ IsOnRightFingerZone(t))

                            {
                                _rightFingerTouch = t;
                                _strikerMonkey.MoveToHit(GameCamera.ScreenToWorldPoint(t.position));
                        
                            }
                            else if (/*(_leftFingerTouch.fingerId == -1
                                || (_leftFingerTouch.phase == TouchPhase.Ended || _leftFingerTouch.phase == TouchPhase.Canceled))
                                &&*/ IsOnLeftFingerZone(t))
                            {
                                //TOCHEC: rBody moveposition
                                _leftFingerTouch = t;
                                _collectorMonkey.Move(t);
                                //_leftMonkey.transform.position = new Vector3(GameCamera.ScreenToWorldPoint(t.position).x, transform.position.y, transform.position.z);
                            }
                            break;

                        case TouchPhase.Canceled |TouchPhase.Ended:
                            if (t.fingerId == _rightFingerTouch.fingerId)
                            {
                        
                                //TODO: send right monkey to floor
                            }
                            else if (t.fingerId == _leftFingerTouch.fingerId)
                            {
                                //TODO: leftmonkey animation stop
                                _collectorMonkey.Stop();
                            }

                            break;

                        case TouchPhase.Moved:
                            if (IsOnLeftFingerZone(t))
                            {
                                //TOCHEC: rBody moveposition
                                _leftFingerTouch = t;
                                _collectorMonkey.Move(t);
                                //_leftMonkey.transform.position = new Vector3(GameCamera.ScreenToWorldPoint(t.position).x, FloorYPos, _leftMonkey.transform.position.z);
                            }
                            break;

                        case TouchPhase.Stationary:

                            break;
                    }
                }
#endregion

                if (_currentAlarmLvl > 0f)
                {
                    _currentAlarmLvl -= _currentAlarmDepletion * Time.deltaTime;
                    UIHelper.Instance.AlarmDepleted(_currentAlarmDepletion * Time.deltaTime/_maxAlarmLvl);
                }

                //_currentLevelTime -= Time.deltaTime;
                //UIHelper.Instance.SetLvlScreenTime(_currentLevelTime);
                //UpdateScreenTime();//TODO: change to change it sec by sec
                /*if (Input.GetKeyDown(KeyCode.Space))
                    _fruitTree.StartSpawn();*/
                break;

            case GAME_STATE.WAITING_MONKEYS_FLEE:
                _fleeTimer += Time.deltaTime;
                if (_fleeTimer >= _monkeysFleeTime)
                {
                    Lose();
                    /*UIHelper.Instance.ShowLoseScreen(true);
                    _gameState = GAME_STATE.IDLE;*/
                    //ShowLoseScreen();
                }
                break;
        }
        
	}
	#endregion

	#region Public Methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pts"></param>
    public void AddScore(int pts)
    {
        
        LeanTween.value(gameObject,_score, _score + pts, _scoreTimeCount).setOnUpdate((float val) =>
        {
            UIHelper.Instance.ScoreText.text = val.ToString("0000");
            //_scoreText.text = val.ToString("0000");
        });
        _score += pts;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gAmount"></param>
    public void AddGold(int gAmount)
    {
        _gold += gAmount;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fr"></param>
    public void CollectFruit(Fruit fr)
    {
        _stageCollectedFruits.Add(fr);
    }

    

    /// <summary>
    /// 
    /// </summary>
    public void UpdateScreenTime()
    {
        _levelTimeText.text = _currentLevelTime.ToString("00");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    public void SetStageIndex(int index)
    {
        _currentStageIndex = index;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    public void SetLevelIndex(int index)
    {
        _currentLevelIndex = index;
    }

    /// <summary>
    /// 
    /// </summary>
    public void LoadAndStartCurrentLevel()
    {
        _currentStageIndex = PlayerPrefs.GetInt("Current_Stage");
        _currentLevelIndex = PlayerPrefs.GetInt("Current_Level");
        _currentStage = _stageList[_currentStageIndex];
        _currentLevel = _stageList[_currentStageIndex].GetLevelList()[_currentLevelIndex];

        Debug.Log("Load and start level. . . ." + _currentStageIndex+"_"+_currentLevelIndex);
        _stageCollectedFruits.Clear();
        //_currentLevel = _stageList[_currentStageIndex].GetLevelList()[_currentLevelIndex];
        _levelLoaded = _currentLevel.SetupLevel();
        _gameState = GAME_STATE.LOADING_LEVEL;
    }

    /// <summary>
    /// 
    /// </summary>
    public void LoadAndStartNextLevel()
    {
        //stage cmompleted
        if (_currentLevelIndex == _currentStage.GetLevelList().Count - 1)
        {
            if (_currentStageIndex == _stageList.Count - 1)
            {
                //TODO: WIN!!
            }
            else
            {
                _currentLevel.UnloadLevelLayout();
                ++_currentStageIndex;
                _currentLevelIndex = 0;
                PlayerPrefs.SetInt("Current_Stage", _currentStageIndex);
                PlayerPrefs.SetInt("Current_Level", _currentLevelIndex);
                LoadAndStartCurrentLevel();
            }
        }
        else
        {
            //check if we have to load a new layout and unload previous one
            if (_currentLevel.SceneLayoutId.CompareTo(_currentStage.GetLevelList()[_currentLevelIndex + 1].SceneLayoutId) != 0)
            {
                _currentLevel.UnloadLevelLayout();
                ++_currentLevelIndex;
                PlayerPrefs.SetInt("Current_Level", _currentLevelIndex);
                LoadAndStartCurrentLevel();
            }
            else
            {
                ++_currentLevelIndex;
                PlayerPrefs.SetInt("Current_Level", _currentLevelIndex);
                _currentLevel = _currentStage.GetLevelList()[_currentLevelIndex];
                _currentLevel.LoadReferences();
                StartCurrentLevel();
            }
        }
        
    }

    /// <summary>
    /// 
    /// </summary>
    public void StartCurrentLevel()
    {
        Debug.Log("Start cLevel");
        //Show Adv
        //Once ad ended, LEvelReady is called from AdsMgr
        if (!AdsMgr.Instance.ShowAd())
            LevelReady();
        //_levelText.text = "Level "+_currentLevelIndex;
    }

    public void LevelReady()
    {
        Debug.Log("Level Ready");
        if (_stageCollectedFruits == null)
            _stageCollectedFruits = new List<Fruit>();
        else
            _stageCollectedFruits.Clear();

        GetLevelReferences();

        //Reset Monkeys
        _strikerMonkey.transform.position = _currentLevel.GetSceneLayout().GetStrikerStartPos().position;
        _strikerMonkey.Reset();
        _collectorMonkey.transform.position = _currentLevel.GetSceneLayout().GetCollectorStartPos().position;
        _collectorMonkey.Reset();
        _fruitTree.Reset();

        //Load equipped items
        if (_strikerSlotA != null)
        {
            _strikerMonkey.SlotA = _strikerSlotA;
            _strikerSlotA = null;
        }
        if (_strikerSlotB != null)
        {
            _strikerMonkey.SlotB = _strikerSlotB;
            _strikerSlotB = null;
        }
        if (_collectorSlotA != null)
        {
            _collectorMonkey.SlotA= _collectorSlotA;
            _collectorSlotA = null;
        }
        if (_collectorSlotB!= null)
        {
            _CollectorMonkey.SlotB = _collectorSlotB;
            _collectorSlotB = null;
        }
        if (_shakerSlotA != null)
        {
            _fruitTree.SlotA= _shakerSlotA;
            _shakerSlotA = null;
        }
        if (_shakerSlotB != null)
        {
            _fruitTree.SlotB = _shakerSlotB;
            _shakerSlotB = null;
        }
        _strikerMonkey.LoadItemsStats();
        _fruitTree.LoadItemsStats();
        _collectorMonkey.LoadItemsStats();

        UIHelper.Instance.SetLevelText(_currentLevelIndex);
        _currentAlarmLvl = 0f;
        _currentLevelTime = _currentLevel.LevelTime;
        UIHelper.Instance.SetAlarmLevel();
        UIHelper.Instance.ResetAlarmUI();

        //Start Level
        _currentLevel.StartLevel();
        _gameState = GAME_STATE.RUN_LEVEL;
        /*if (GameObject.FindGameObjectWithTag("MainMenu") != null)
        {
            Tutorial.Instance.InitTutorial();
        }*/
        if (Tutorial.Instance != null)
            Tutorial.Instance.InitTutorial();
    }

    /// <summary>
    /// 
    /// </summary>
    public void LevelEnded()
    {
        Debug.Log("________________________>>Level Edned, saving data...");
        _gameState = GAME_STATE.LEVEL_ENDED;
        /*if (_stageCollectedFruits.Find((fr) => (fr._Ftype == Fruit.F_TYPE.GOLD_ITEM)) != null)
            DataMgr.Instance.SaveGold();
        if (_stageCollectedFruits.Find((fr) => (fr._Ftype == Fruit.F_TYPE.EQUIPMENT)) != null)
            DataMgr.Instance.SaveInventoryItems();
        DataMgr.Instance.SaveLevelData(_currentStageIndex, _currentLevelIndex);*/
        //DataMgr.Instance.SaveData();
         //collect sack fruits
        UIHelper.Instance.ShowLevelFinishedScreen();
    }

    /// <summary>
    /// 
    /// </summary>
    public void SaveProgress()
    {
        if (_stageCollectedFruits.Find((fr) => (fr._Ftype == Fruit.F_TYPE.GOLD_ITEM)) != null)
            DataMgr.Instance.SaveGold();
        if (_stageCollectedFruits.Find((fr) => (fr._Ftype == Fruit.F_TYPE.EQUIPMENT)) != null)
            DataMgr.Instance.SaveInventoryItems();
        DataMgr.Instance.SaveLevelData(_currentStageIndex, _currentLevelIndex);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Level GetCurrentLevel()
    {
        return _currentLevel;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Stage GetCurrentStage()
    {
        return _currentStage;
    }

    

    /// <summary>
    /// 
    /// </summary>
    /// <param name="alarm"></param>
    /// <returns>if game is lost due to alarm lvl</returns>
    public bool RaiseAlarm(float alarm)
    {
        
        _currentAlarmLvl += _alarmIncrease;
        if (AlarmRaisedEvt != null)
            AlarmRaisedEvt();
        //_alarmText.text = _currentAlarmLvl + " / " + _maxAlarmLvl;
        UIHelper.Instance.SetAlarmLevel();
        if (_currentAlarmLvl >= _maxAlarmLvl)
        {
            if (DataMgr.Instance.Vibration == 1)
                Vibration.Vibrate(_loseVirationTime);

            _fruitTree.Stop();
            WaitForGuardWakeUp();
            _currentLevel.EndLevel();
            return true;
        }
        else
        {
            //Vibration feedback
        }
        {
            if (DataMgr.Instance.Vibration == 1)
                Vibration.Vibrate(_missFruitVibrationTime);
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void GuardWokenUp()
    {
        _collectorMonkey.Flee();
        _strikerMonkey.Flee();
        _gameState = GAME_STATE.WAITING_MONKEYS_FLEE;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Lose()
    {
        //TOOO: analytics
        UIHelper.Instance.ShowLoseScreen(true);
        _collectorMonkey._Sack.Stop();
        _gameState = GAME_STATE.IDLE;
        Debug.Log("LOSE_______________________________________LOSE");
    }

    public void WaitForGuardWakeUp()
    {
        _gameState = GAME_STATE.WAITING_FOR_GUARD;
    }

    public void WaitForMonkeysFlee()
    {
        _gameState = GAME_STATE.WAITING_MONKEYS_FLEE;
    }
    /// <summary>
    /// Check if any guard get alarmed without repetition
    /// </summary>
    /// <param name="xPos"></param>
    public void FruitMissed(float xPos)
    {
        foreach (Guard g in _currentLevel.GuardList)
        {
            if (g.CheckAlarm(xPos))
                return;
        }
    }

    

    /// <summary>
    /// 
    /// </summary>
    public void Retry()
    {
        //SceneManager.LoadScene("Game");
        StartCurrentLevel();
    }

    /// <summary>
    /// 
    /// </summary>
    public void GetManagerReferences()
    {
        GameObject returnGO = null;

        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (_mainCamera == null)
            Debug.LogError("No main cam found!");

        //Collector
        if (_collectorMonkey == null)
        {
            returnGO = GameObject.FindGameObjectWithTag("Collector");
            if (returnGO == null)
                Debug.LogError("No Collector go found");
            _collectorMonkey = returnGO.GetComponent<CollectorMonkey>();
            if (_collectorMonkey == null)
                Debug.LogError("No Collector comp attached!");
        }

        //Striker
        if (_strikerMonkey == null)
        {
            returnGO = GameObject.FindGameObjectWithTag("Striker");
            if (returnGO == null)
                Debug.LogError("No Striker go found");
            _strikerMonkey = returnGO.GetComponent<TapMonkey>();
            if (_strikerMonkey == null)
                Debug.LogError("No Striker comp attached!");

        }
        if (_guardPool == null)
            _guardPool = new List<Guard>();
        else
        {
            _guardPool.Clear();
            foreach (GameObject goGuard in GameObject.FindGameObjectsWithTag("Guard"))
                _guardPool.Add(goGuard.GetComponent<Guard>());
    
        }

    }

    public void Pause(bool pause)
    {
        if (pause && _gameState != GAME_STATE.IDLE)
        {
            _lastState = _gameState;
            _gameState = GAME_STATE.IDLE;
            LeanTween.pauseAll();
        }
        else if (!pause &&_gameState == GAME_STATE.IDLE)
        {
            _gameState = _lastState;
            LeanTween.resumeAll();
        }
        _currentLevel.Pause(pause);
        _fruitTree.Pause(pause);
        
    }
	#endregion


	#region Private Methods
    
    private void GetLevelReferences()
    {
        _currentHRef = _currentLevel.NetHeightRef;
        _floorRef = _currentLevel.FloorHeightRef;
        _minHRef = _currentLevel.LeftCollectorLimit;
        _maxHRef = _currentLevel.RightCollectorLimit;
        _minLeftFingerRef = _currentLevel.MinLeftFingerRef;
        _maxLeftFingerRef =  _currentLevel.MaxLeftFingerRef;
        _minRightFingerRef = _currentLevel.MinRightFingerRef;
        _maxRightFingerRef = _currentLevel.MaxRightFingerRef;
    }

    private void GetCurrentCocoHeight()
    {
        _currentHRef = GameObject.FindGameObjectWithTag("HeightRef").transform;
    }

    private void GetCurrentFloorReference()
    {
        
    }

    /// <summary>
    /// Check if Touch t is inside left finger - drag zone
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    private bool IsOnLeftFingerZone(Touch t)
    {
        Vector2 tWorldPosition;

        tWorldPosition = _mainCamera.GetComponent<Camera>().ScreenToWorldPoint(t.position);
        if (tWorldPosition.x > _minLeftFingerRef.position.x && tWorldPosition.x < _maxLeftFingerRef.position.x)
            return true;
        return false;
    }

    /// <summary>
    /// Check if Touch t is inside right finger - tap zone
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    private bool IsOnRightFingerZone(Touch t)
    {
        Vector2 tWorldPosition;

        tWorldPosition = GameCamera.ScreenToWorldPoint(t.position);
        if (tWorldPosition.x > _minRightFingerRef.position.x && tWorldPosition.x < _maxRightFingerRef.position.x)
            return true;
        return false;
    }

    

    private void ShowLoseScreen()
    {
        _loseScreen.SetActive(true);
    }
	#endregion


	#region Properties
    public float Gravity { get { return _gravity; } set { _gravity = value; } }
    public Camera GameCamera { get { return _mainCamera.GetComponent<Camera>(); } private set {  } }
    public float FruitFlyTime { get { return _fruitFlyTime; } set { _fruitFlyTime = value; } }
    public float FruitFallTime { get { return _fruitFallTime; } set { _fruitFallTime = value; } }
    public float FloorYPos { get { return _floorRef.position.y; } private set { } }
    public float MaxCocoHeight { get { return _currentHRef.position.y; } private set { } }
    public float MinHorizontalCocoPos { get { return _minHRef.position.x; } private set { } }
    public float MaxHorizontalCocoPos { get { return _maxHRef.position.x; } private set { } }
    public float FrameTime { get { return _frameTime; } set { _frameTime = value; } }
    public float MaxCocoOffset { get { return _maxFruitOffset; } set { _maxFruitOffset = value; } }

    public List<Fruit> StageCollectedFruits { get { return _stageCollectedFruits; } set { _stageCollectedFruits = value; } }

    public TapMonkey _StrikerMonkey { get { return _strikerMonkey; } set { _strikerMonkey = value; } }
    public CollectorMonkey _CollectorMonkey { get { return _collectorMonkey; } set { _CollectorMonkey = value; } }

    public List<Stage> _StageList { get { return _stageList; } set { _stageList = value; } }

    public List<Guard> _GuardPool { get { return _guardPool; } set { _guardPool = value; } }

    public FruitTree _FruitTree { get { return _fruitTree; } set { _fruitTree = value; } }

    public int StageIndex { get { return _currentStageIndex; } set { _currentStageIndex = value; } }
    public int LevelIndex { get { return _currentLevelIndex; } set { _currentLevelIndex = value; } }
    public int Gold { get { return _gold; } set { _gold = value; } }
    public int Score { get { return _score; } set { _score = value; } }
    public int _Vibration { get { return _vibration; } set { _vibration = value; } }

    public float CurrentAlarmLevel { get { return _currentAlarmLvl; } set { _currentAlarmLvl = value; } }
    public float MaxAlarmLevel { get { return _maxAlarmLvl; } set { _maxAlarmLvl = value; } }
    public float CurrentAlarmDepletion { get { return _currentAlarmDepletion; } set { _currentAlarmDepletion = value; } }
    public float AlarmIncrease { get { return _alarmIncrease; } set { _alarmIncrease = value; } }

    public EquipmentItem ShakerSlotA { get { return _shakerSlotA; } set { _shakerSlotA = value; } }
    public EquipmentItem ShakerSlotB { get { return _shakerSlotB; } set { _shakerSlotB = value; } }
    public EquipmentItem CollectorSlotA { get { return _collectorSlotA; } set { _collectorSlotA = value; } }
    public EquipmentItem CollectorSlotB { get { return _collectorSlotB; } set { _collectorSlotB = value; } }
    public EquipmentItem StrikerSlotA { get { return _strikerSlotA; } set { _strikerSlotA = value; } }
    public EquipmentItem StrikerSlotB { get { return _strikerSlotB; } set { _strikerSlotB = value; } }
	#endregion

	#region Private Serialized Fields
    //Level references
    [SerializeField]
    private Transform _floorRef;
    [SerializeField]
    private Transform _minHRef, _maxHRef;
    

    //Coconut
    [SerializeField]
    private float _fruitFlyTime;
    [SerializeField]
    private float _fruitFallTime;
    [SerializeField]
    private FruitTree _fruitTree;
    [SerializeField]
    private float _fruitSpawnTime;
    [SerializeField]
    private float _maxFruitOffset;
    //Monkeys
    [SerializeField]
    private CollectorMonkey _collectorMonkey;
    [SerializeField]
    private TapMonkey _strikerMonkey;
    [SerializeField]
    private Image _leftMonkeyBodyImg, _leftMonkeyLegsImg;
    [SerializeField]
    private Image _rightMonkeyImg;
    
    [SerializeField]
    private float _frameTime;

    

    //UI
    [SerializeField]
    private Text _scoreText;
    [SerializeField]
    private float _scoreTimeCount;
    [SerializeField]
    private Text _alarmText;

    [SerializeField]
    private Text _levelTimeText;

    [SerializeField]
    private List<Stage> _stageList;
    [SerializeField]
    private Level _currentLevel;
    [SerializeField]
    private List<Level> _levelList;
    [SerializeField]
    private LevelFinishedScreen _lvlFinishedScr;
    [SerializeField]
    private GameObject _loseScreen;
    [SerializeField]
    private List<Guard> _guardPool;
    [SerializeField]
    private Text _levelText;

    //Screens
    //[SerializeField]
    private ShopMenu _shopScreen;
    //[SerializeField]
    private InventoryScreen _invScreen;
	#endregion

	#region Private Non-serialized Fields
    private Transform _minLeftFingerRef, _maxLeftFingerRef, _minRightFingerRef, _maxRightFingerRef;
    private float _gravity;
    private Transform _currentHRef;
    private GameObject _mainCamera;

    private List<Fruit> _stageCollectedFruits;
    private Touch _leftFingerTouch, _rightFingerTouch;
    

    private int _score, _gold;
    private int _vibration; //0 diabled, 1 enabled
    
    private float _currentLevelTime;
    private int _currentStageIndex, _currentLevelIndex;

    private GAME_STATE _gameState, _lastState;

    private AsyncOperation _levelLoaded;
    private Stage _currentStage;




    private float _currentAlarmLvl, _maxAlarmLvl, _alarmIncrease;
    private float _currentAlarmDepletion;   //alarm depletion over second
    private float _fleeTimer;

    private EquipmentItem _shakerSlotA, _shakerSlotB, _collectorSlotA, _collectorSlotB, _strikerSlotA, _strikerSlotB;
    #endregion

}
