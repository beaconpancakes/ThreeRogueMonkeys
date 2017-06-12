/************************************************************************/
/* @Author: Rodrigo Ribeiro-Pinto Carvajal
 * @Date: 2017/05/15
 * @Brief: BBBrief
 * @Description: DDDescription
 * ***********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Fruit : MonoBehaviour {

	#region Public Data
    public enum FRUIT_ST { IDLE = 0, FALLING_FROM_TREE, LAUNCHING, ON_SACK, DISMISSED, COLLECTED, WAITING_FOR_LAUNCH }
    /// <summary>
    /// COCO = 0
    /// BANANA = 1
    /// CACAO = 2
    /// CLUSTER_SEED = 3
    /// CLUSTER_UNIT = 4
    /// MULTI_SEED = 5
    /// MULTI_UNIT = 6
    /// SPIKY = 7
    /// GOLD_ITEM = 8
    /// EQUIPMENT = 9
    /// </summary>
    public enum F_TYPE { COCO = 0, BANANA, CACAO, CLUSTER_SEED, CLUSTER_UNIT, MULTI_SEED, MULTI_UNIT, SPIKY, GOLD_ITEM, EQUIPMENT }
    public enum G_TYPE { XS = 0, S, M, L, XL, XXL, RELIC }
    public const float _floorCollisionOffset = 5f;  //distance below floor reference to destroy missed fruits
    public const float _multiLaunchDelay = .2f;
    public const int _multiUnits = 3;
    public const int _clusterUnits = 2;

    public const int _cocoScore = 5;
    public const int _bananaScore = 10;
    public const int _cacaoScore = 4;
    public const int _clusterScore = 5;
    public const int _goldItemScore = 20;
    public const int _equipmentItemScore = 35;
	#endregion


	#region Behaviour Methods
	// Use this for initialization
	void Start () {
        _img = GetComponent<Image>();
        if (_img == null)
            Debug.LogError("No attached img found");
	}
	
	// Update is called once per frame
	void Update () {
        switch (_state)
        {
            case FRUIT_ST.WAITING_FOR_LAUNCH:
                _waitingTimer += Time.deltaTime;
                if (_waitingTimer >= _waitCooldown)
                {
                    _state = FRUIT_ST.LAUNCHING;
                    AudioController.Play("fruit_1b");
                    //_flyingTime = 0f;
                }
                break;

            case FRUIT_ST.FALLING_FROM_TREE:
                _flyingTime += Time.deltaTime;
                transform.position = new Vector2(_initPos.x + _initSpeed.x * _flyingTime,
                                            _initPos.y + _initSpeed.y * _flyingTime + 0.5f * _virtualGravity * _flyingTime * _flyingTime);
                //Check if floor collision
                if (transform.position.y <=  GameMgr.Instance.FloorYPos - _floorCollisionOffset)
                   _fTree.DestroyFruit(this, true);
                break;

            case FRUIT_ST.LAUNCHING:
                _flyingTime += Time.deltaTime;
                transform.position = new Vector2(_initPos.x + _initSpeed.x * _flyingTime,
                                            _initPos.y  + _initSpeed.y * _flyingTime + 0.5f * _virtualGravity * _flyingTime * _flyingTime);
                //Check if floor collision
                if (transform.position.y <= GameMgr.Instance.FloorYPos - _floorCollisionOffset)
                    _fTree.DestroyFruit(this, true);
                break;

            case FRUIT_ST.DISMISSED:
                _flyingTime += Time.deltaTime;
                if (_flyingTime >= _dissmissAnimationTime)
                {
                    _fTree.DestroyFruit(this);
                }
                /*_flyingTime += Time.deltaTime;
                transform.position = new Vector2(_initPos.x + _initSpeed.x * _flyingTime,
                                            _initPos.y  + _initSpeed.y * _flyingTime + 0.5f * _virtualGravity * _flyingTime * _flyingTime);*/
                //Check if floor collision
                //if (transform.position.y <= GameMgr.Instance.FloorYPos - _floorCollisionOffset)
                    //_fTree.DestroyFruit(this);//
                break;

            case FRUIT_ST.COLLECTED:
                _flyingTime += Time.deltaTime;
                if (_flyingTime >= _collectedAnimTime)
                    _fTree.DestroyFruit(this);
                break;
        }
	}
	#endregion

	#region Public Methods
    /// <summary>
    /// 
    /// </summary>
    public void StartFruit()
    {
        GetFruitFallSpeed(_fTree.CurrentFallSpeed);
        _state = FRUIT_ST.FALLING_FROM_TREE;
        LeanTween.rotateZ(gameObject, -540f, GameMgr.Instance.FruitFallTime);
    }
    
    /// <summary>
    /// 
    /// </summary>
    public void Launch()
    {
        if (_state == FRUIT_ST.FALLING_FROM_TREE || (_state == FRUIT_ST.LAUNCHING && _maturityLevel > 0))
        {

            _initPos = new Vector2(transform.position.x, transform.position.y);
            --_maturityLevel;
            switch (_type)
            {
                case F_TYPE.COCO:
                    GetFruitLaunchSpeed(GameMgr.Instance.FruitFlyTime);
                    _state = FRUIT_ST.LAUNCHING;
                    AudioController.Play("fruit_1b");
                    break;

                case F_TYPE.BANANA:
                    if (_maturityLevel == 0)
                    {
                        GetFruitLaunchSpeed(GameMgr.Instance.FruitFlyTime);
                        _state = FRUIT_ST.LAUNCHING;
                    }
                    else
                    {
                        //keep falling state
                        GetFruitLaunchSpeed(GameMgr.Instance.FruitFlyTime, false);  
                        //switch sprite
                        _img.sprite = _yellowBanana;
                    }
                    AudioController.Play("fruit_1b");
                    break;

                case F_TYPE.CACAO:
                    GetFruitLaunchSpeed(GameMgr.Instance.FruitFlyTime);
                    _state = FRUIT_ST.LAUNCHING;
                    AudioController.Play("fruit_1b");
                    break;

                case F_TYPE.CLUSTER_SEED:
                    for (int i = 0; i < _clusterUnits; ++i)
                    {
                        Fruit clusterUnit =_fTree.GetClusterUnit();
                        clusterUnit.LaunchClusterUnitFruit(transform.position, _initPos);
                        
                        AudioController.Play("fruit_1b");

                    }
                    //Destroy seed
                    _fTree.DestroyFruit(this);
                    break;

                case F_TYPE.MULTI_SEED:
                    //Launch 1st seed and then create others with same speed
                    
                    Fruit multiUnit = _fTree.GetMultiUnit();
                    multiUnit.LaunchClusterUnitFruit(transform.position, _initPos);
                    AudioController.Play("fruit_1b");
                    //Get speed and gravity from frist to apply the same to others
                    Vector2 unitsSpeed = multiUnit._InitSpeed;
                    float vGrav = multiUnit._virtualGravity;
                    for (int i = 0; i < _multiUnits - 1; ++i)
                    {
                        multiUnit = _fTree.GetMultiUnit();
                        multiUnit.LaunchMultiUnitFruit(transform.position, _initPos, vGrav, unitsSpeed, _multiLaunchDelay*(i+1));

                        

                    }
                    //Destroy seed
                    _fTree.DestroyFruit(this);
                    break;

                case F_TYPE.SPIKY:
                    GameMgr.Instance.RaiseAlarm(15f);
                    ThrowOut();
                    break;

                case F_TYPE.GOLD_ITEM:
                    //TODO: Faster speed? Special Sound
                    GetFruitLaunchSpeed(GameMgr.Instance.FruitFlyTime);
                    AudioController.Play("item_hit");
                    break;

                case F_TYPE.EQUIPMENT:
                    if (_maturityLevel == 0)
                        GetFruitLaunchSpeed(GameMgr.Instance.FruitFlyTime);
                    else
                    {
                        //TODO: Function
                        GetFruitLaunchSpeed(GameMgr.Instance.FruitFlyTime, false);
                        --_maturityLevel;
                    }
                    break;
            }
            Debug.Log("Launch!");
        }
        
    }

    /// <summary>
    /// 
    /// </summary>
    public void DepositOnSack()
    {
        LeanTween.cancel(gameObject);

        //_pTree.DestroyCoconut(this);
        AudioController.Play("fruit_collect");
        _state = FRUIT_ST.ON_SACK;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Collect()
    {
        //TODO: change audio
        AudioController.Play("fruit_collect");
        _state = FRUIT_ST.COLLECTED;
        GameMgr.Instance.CollectFruit(this);
        GameMgr.Instance.AddScore(_currentScore);
        
        //TODO: fix gold amount
        //if (_type == F_TYPE.GOLD_ITEM)
            //GameMgr.Instance.AddGold(_baseGold);

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="animTime"></param>
    public void SetCollectedAnimation(Vector2 pos, float animTime)
    {
        //Animation
        LeanTween.cancel(gameObject);
        LeanTween.move(gameObject, pos, animTime);
        LeanTween.rotateZ(gameObject, 360f, animTime);//.setOnComplete(() =>
        //{
            //_fTree.DestroyFruit(this);
        //});
        _collectedAnimTime = animTime;
        _flyingTime = 0f;
    }

    /// <summary>
    /// Move, rotate and destroy the fruit
    /// </summary>
    public void Dissmiss()
    {
        _state = FRUIT_ST.DISMISSED;
        _flyingTime = 0f;
        LeanTween.cancelAll(gameObject);
        LeanTween.move(gameObject, (Vector2)transform.position + _dissmissAnimationOffset, _dissmissAnimationTime);
        LeanTween.rotateZ(gameObject, 1080f, _dissmissAnimationTime);// setOnComplete(() =>
         //{


         //});
        AudioController.Play("fruit_dissmissed");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fruitTypeIndex"></param>
    public void SetupFruit(int fruitTypeIndex)
    {
        _type = (F_TYPE)fruitTypeIndex;
        if (_img == null)
            _img = GetComponent<Image>();
        if (_type != F_TYPE.GOLD_ITEM && _type != F_TYPE.EQUIPMENT)
            _img.sprite = _fruitSpriteList[fruitTypeIndex];
        switch (_type)
        {
            case F_TYPE.COCO:
                _currentScore = _cocoScore;
                _maturityLevel = 1;
                break;

            case F_TYPE.BANANA:
                _currentScore = _bananaScore;
                _maturityLevel = 2; //TODO: MAGIC NUMBER
                break;

            case F_TYPE.CACAO:
                _currentScore = _cacaoScore;
                _maturityLevel = 1;
                break;

            /*case F_TYPE.GOLD_ITEM:
                _currentScore = _baseGold;
                _maturityLevel = 1;
                _img.sprite = _goldSpriteList[(int)_goldItemType];
                break;

            case F_TYPE.EQUIPMENT:
                _currentScore = _baseScore;
                _maturityLevel =  GetEquipmentMaturity();
                _img.sprite = _goldSpriteList[(int)_goldItemType];
                break;*/
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="goldTypeIndex"></param>
    public void SetupFruitAsGoldItem(string id)
    {
        Debug.Log("___________" + id + "____________________>>>>Fruit set as gold: ");
        //_goldItemType = (G_TYPE)goldTypeIndex;
        if (_img == null)
            _img = GetComponent<Image>();
        _img.sprite = DataMgr.Instance.GetGameItems().Find((itm) => (itm.IdName.CompareTo(id) == 0))._Sprite;
        
        _currentScore = _goldItemScore;
        _maturityLevel = 2;
        _type = F_TYPE.GOLD_ITEM;
        
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="goldTypeIndex"></param>
    public void SetupFruitAsEquipmentItem(string id)
    {
        //_goldItemType = (G_TYPE)goldTypeIndex;
        if (_img == null)
            _img = GetComponent<Image>();
        _img.sprite = DataMgr.Instance.GetGameItems().Find((itm) => (itm.IdName.CompareTo(id) == 0))._Sprite;
        _currentScore = _equipmentItemScore;
        _maturityLevel = 3; 
        _type = F_TYPE.EQUIPMENT;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Sprite GetFruitSprite()
    {
        return _img.sprite;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int GetScore()
    {
        return _currentScore;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int GetGold()
    {
        if (_type == F_TYPE.GOLD_ITEM)
            return _baseGold;
        return 0;
    }

    /// <summary>
    /// Calculates variables to use position ecuation when throwing the fruit to left side oaccording to its landing time(from maxH to floor)
    /// </summary>
    /// <param name="flyTime"></param>
    public void GetFruitLaunchSpeed(float flyTime, bool toLeftSide = true)
    {
        float xSpeed, ySpeed, timeToH, currentFruitHeight;
        float targetMidPos = 0f;
        float targetXPos = 0f;
        currentFruitHeight = GameMgr.Instance.MaxCocoHeight + Random.Range(0f, GameMgr.Instance.MaxCocoOffset);
        //(1) Set virtual gravity given the time from maxHeight to floor
        _virtualGravity = -2f * (currentFruitHeight - GameMgr.Instance.FloorYPos) / (flyTime * flyTime);
        //Debug.Log("FRUIT: " + _type + "  toLEftSide" + toLeftSide);

        //(2) Get init speed. We need first to calculate the time it needs to reach maxHeight, when its y_speed = 0f
        timeToH = Mathf.Sqrt(2f * (transform.position.y - currentFruitHeight) / _virtualGravity);
        //Debug.Log("Time to H: " + timeToH);
        ySpeed = -_virtualGravity * timeToH;

         //check throwing side
        if (toLeftSide)
        {
            //Apply accuracy
            targetMidPos = (GameMgr.Instance.MaxHorizontalCocoPos - GameMgr.Instance.MinHorizontalCocoPos) * 0.5f;
            targetXPos = Mathf.Lerp(Random.Range(GameMgr.Instance.MinHorizontalCocoPos, GameMgr.Instance.MaxHorizontalCocoPos), targetMidPos, GameMgr.Instance._StrikerMonkey.GetAccuracy());
            //xSpeed = (targetXPos - transform.position.x) / (timeToH + flyTime);
        }
        else
        {
            targetMidPos = transform.position.x;
            targetXPos = Mathf.Lerp(Random.Range(GameMgr.Instance.GetCurrentLevel().LeftBotSpawnLimit.position.x, GameMgr.Instance.GetCurrentLevel().RightTopSpawnLimit.position.x), targetMidPos, GameMgr.Instance._StrikerMonkey.GetAccuracy());
            
        }
        xSpeed = (targetXPos - transform.position.x) / (timeToH + flyTime);
        _initSpeed = new Vector2(xSpeed, ySpeed);
        LeanTween.cancel(gameObject);
        LeanTween.rotateZ(gameObject, 1080f, timeToH + flyTime);

        _flyingTime = 0f;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <param name="initSpeed"></param>
    public void LaunchClusterUnitFruit(Vector2 position, Vector2 initPos)
    {
        transform.position = position;
        _initPos = initPos;
        //_initSpeed = initSpeed;
        GetFruitLaunchSpeed(GameMgr.Instance.FruitFlyTime);
        _state = FRUIT_ST.LAUNCHING;
        _fTree = GameMgr.Instance._FruitTree;
        _flyingTime = 0f;
        gameObject.SetActive(true);
       
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <param name="initPos"></param>
    /// <param name="gravity"></param>
    /// <param name="initSpeed"></param>
    /// <param name="delayTime"></param>
    public void LaunchMultiUnitFruit(Vector2 position, Vector2 initPos, float gravity, Vector2 initSpeed, float delayTime)
    {
        transform.position = position;
        _initPos = initPos;
        //_initSpeed = initSpeed;
        GetFruitLaunchSpeed(GameMgr.Instance.FruitFlyTime);
        _state = FRUIT_ST.WAITING_FOR_LAUNCH;
        _fTree = GameMgr.Instance._FruitTree;
        _virtualGravity = gravity;
        _initSpeed = initSpeed;
        _waitCooldown = delayTime;
        _waitingTimer = 0f;
        _flyingTime = 0f;
        gameObject.SetActive(true);  
    }

    public void Pause(bool pause)
    {
        if (pause)
        {
            _lastState = _state;
            _state = FRUIT_ST.IDLE;
            
        }
        else
        {
            _state = _lastState;
        }
    }
	#endregion


	#region Private Methods
    /// <summary>
    /// Calculates variables to use fall position ecuation according to fall time
    /// </summary>
    /// <param name="fallTime"></param>
    private void GetFruitFallSpeed(float fallTime)
    {
        //(1) Set virtual gravity given the time from spawnPosition to floor
        _virtualGravity = -2f * (transform.position.y-GameMgr.Instance.FloorYPos) / (fallTime * fallTime);
        _initSpeed = new Vector2(0f, _virtualGravity*fallTime);
        _initPos = transform.position;
        _flyingTime = 0f;
    }

    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private int GetEquipmentMaturity()
    {
        if (_type != F_TYPE.EQUIPMENT)
            Debug.LogError("This is not an equip item!");

        //TODO: check equpment "quality" to determine max Maturity
        return 3;

    }

    /// <summary>
    /// 
    /// </summary>
    private void ThrowOut()
    {
        float xSpeed, ySpeed, timeToH, currentFruitHeight;

        _state = FRUIT_ST.DISMISSED;
        _initPos = transform.position;
        _flyingTime = 0f;

        currentFruitHeight = transform.position.y + _throwOutHeight;
        //(1) Set virtual gravity given the time from maxHeight to floor
        _virtualGravity = -2f * (currentFruitHeight - GameMgr.Instance.FloorYPos) / (_throwOutFallTime * _throwOutFallTime);
        //Debug.Log("FRUIT: " + _type + "  toLEftSide" + toLeftSide);

        //(2) Get init speed. We need first to calculate the time it needs to reach maxHeight, when its y_speed = 0f
        timeToH = Mathf.Sqrt(2f * (transform.position.y - currentFruitHeight) / _virtualGravity);
        /*Debug.Log("Time to H: " + timeToH);
        ySpeed = -_virtualGravity * timeToH;


        xSpeed = (Random.Range(GameMgr.Instance.GetCurrentLevel().LeftBotSpawnLimit.position.x, GameMgr.Instance.GetCurrentLevel().RightTopSpawnLimit.position.x) - transform.position.x) / (timeToH + _throwOutFallSpeed);
        */
        _initSpeed = _throwOutSpeed;
        LeanTween.cancel(gameObject);
        LeanTween.rotateZ(gameObject, -1440f, timeToH + _throwOutFallTime);

        _flyingTime = 0f;
        
    }
	#endregion


	#region Properties
    public FruitTree FruitTree { get { return _fTree; } set { _fTree = value; } }
    public F_TYPE _Ftype { get { return _type; } private set { _type = value; } }
    public FRUIT_ST _FState { get { return _state; } private set { _state = value; } }
    public Vector2 _InitSpeed { get { return _initSpeed; } set { _initSpeed = value; } }
	#endregion

	#region Private Serialized Fields
    //TODO: unerialie and add score based on fruit type
    [SerializeField]
    private int _baseScore;
    [SerializeField]
    private int _baseGold;
    [SerializeField]
    private F_TYPE _type;//TODO: unserialize both fields
    [SerializeField]
    private G_TYPE _goldItemType;   

    [SerializeField]
    private Transform _minLandingPt, _maxLandingPt;
    [SerializeField]
    private Transform _maxHeightPt;
    [SerializeField]
    private float _maxHeightOffset;

    [SerializeField]
    private Vector2 _dissmissAnimationOffset;
    
    [SerializeField]
    private float _dissmissAnimationTime;

    [SerializeField]
    private List<Sprite> _fruitSpriteList;
    [SerializeField]
    private List<Sprite> _goldSpriteList;
    [SerializeField]
    private Sprite _yellowBanana;
	#endregion

	#region Private Non-serialized Fields
    private FRUIT_ST _state, _lastState;
    

    private FruitTree _fTree;
    private Vector2 _initSpeed;
    private Vector2 _initPos;
    private float _currentMaxHeight;
    private float _flyingTime;
    private float _virtualGravity;
    private float _waitingTimer, _waitCooldown;
    private float _timer; //timer, general purposes

    private Image _img;
    private int _currentScore;
    private int _maturityLevel; //if 0 then goes to left; otherwise it get launched upwards again**

    [SerializeField]
    private Vector2 _throwOutSpeed;
    [SerializeField]
    private float _throwOutHeight;
    [SerializeField]
    private float _throwOutFallTime;

    private float _collectedAnimTime;
	#endregion
}
