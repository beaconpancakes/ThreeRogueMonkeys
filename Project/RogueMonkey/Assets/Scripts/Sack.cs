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
public class Sack : MonoBehaviour {

	#region Public Data
    public enum S_STATE { IDLE = 0, ANIMATING, RELOADING, STOP, BROKEN }

    [System.Serializable]
    public class SackFruit
    {
        public Fruit _Fruit;        //Attached Fruit
        public float _Timer;        //current animation time
        public Vector2 _destPos;    //destination position on UI stack

        public SackFruit(Fruit f, Vector2 sackPos)
        {
            _Fruit = f;
            _Timer = 0f;
            _destPos = sackPos;
        }
    }
	#endregion


	#region Behaviour Methods
	// Use this for initialization
	void Start () {
        //InitSack();
	}
	
	// Update is called once per frame
	void Update () {
        switch (_state)
        {
            case S_STATE.ANIMATING:
                foreach (SackFruit sf in _animatingFruitList)
                {
                    sf._Timer += Time.deltaTime;
                    sf._Fruit.transform.position = Vector2.Lerp((Vector2)_stackEnterPt.position, sf._destPos, _fruitStackAC.Evaluate(sf._Timer / _sackFruitAnimationTime));
                    //Reached its targegt osition. Set time to -1 to delete 
                    if (sf._Timer >= _sackFruitAnimationTime)
                        sf._Timer = -1f;
                }
                //Remove all placed on position fruit
                _animatingFruitList.RemoveAll((sf) => sf._Timer == -1f);
                //Check state
                if (_animatingFruitList.Count == 0)
                    _state = S_STATE.IDLE;
                break;

            case S_STATE.RELOADING:
                _reloadTimer += Time.deltaTime;
                if (_reloadTimer >= _reloadPerFruitTime)
                { 
                    _reloadTimer = 0f;


                    _tempFruit = _fruitList[_currentStackIndex];
                    _fruitList.RemoveAt(_currentStackIndex);
                    --_currentStackIndex;
                    _sackFillImg.fillAmount = (float)_fruitList.Count / _maxCount; //Update UI fill
                    //Collect Fruit
                    _tempFruit.Collect();
                    //Checkk Event (tutorial)
                    if (FruitCollectedEvt!=null)
                        FruitCollectedEvt();
                    //Animate fruit  going out from Sack
                    _tempFruit.SetCollectedAnimation(new Vector2(_tempFruit.transform.position.x + _collectedAnimationOffset.x, _tempFruit.transform.position.y + (_stackEnterPt.transform.position.y - _tempFruit.transform.position.y) + _collectedAnimationOffset.y), _collectedAnimTime);
                    
                    //Update Collector speed
                    _speedLoss -= _lossSpeedPerFruit;
                    _Collector.ReduceSpeed(_speedLoss);

                    //Reloading finished
                    if (_fruitList.Count == 0)
                    {
                        _state = S_STATE.IDLE;
                        GameMgr.Instance.ResetCollectedIndex();
                    }
                }  
                break;

            case S_STATE.BROKEN:
                _brokenTimer += Time.deltaTime;
                if(_brokenTimer >= _Collector._StunTime)
                {
                    _state = S_STATE.IDLE;
                }
                else if (_brokenFrCount != 0)
                {
                    _throwFrTimer += Time.deltaTime;
                    if (_throwFrTimer >= _Collector._StunTime / _brokenFrCount)
                    {
                        UnstackAndDissmiss();
                        _throwFrTimer = 0f;
                    }
                }
                break;
        }
        //sack image fill animation
        if (_fillingSack)
        {
            _fillTimer += Time.deltaTime;
            _sackFillImg.fillAmount = _initFillValue + (_targetFillValue - _initFillValue) * _fillAnimCurve.Evaluate(_fillTimer / _fillTime);
            //_alarmSbar.value = Mathf.Lerp(_initBarValue, _targetBarValue, _growCurve.Evaluate(_barTimer / _barGrowTime));
            if (_fillTimer >= _fillTime)
            {
                _fillTimer = 0f;
                _fillingSack = false;
            }
        }
	}
	#endregion

	#region Public Methods
    public delegate void OnSackFullEvent();
    public delegate void OnSackReloadEvent();
    public delegate void OnFruitCollectedEvent();

    public event OnSackFullEvent SackFullEvt;
    public event OnSackReloadEvent SackReloadEvt;
    public event OnFruitCollectedEvent FruitCollectedEvt;
    /// <summary>
    /// 
    /// </summary>
    public void InitSack()
    {
        if (_animatingFruitList == null)
            _animatingFruitList = new List<SackFruit>(_currentCapacity);
        else
            _animatingFruitList.Clear();
        
        if (_fruitList == null)
            _fruitList = new List<Fruit>();
        else
            _fruitList.Clear();
        _currentStackIndex = -1;

        _maxCount = _currentCapacity;
        Debug.Log("Max count is : " + _maxCount);
        _speedLoss = 0f;
        _sackFillImg.fillAmount = 0f;
        _fillingSack = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Reset()
    {
        Debug.Log("Reset Sack_______________________________________");
        if (_animatingFruitList == null)
            _animatingFruitList = new List<SackFruit>(_initUICapacity);
        else if (_animatingFruitList.Count > 0)
        {
            /*for (int i=0; i< _animatingFruitList.Count; ++i)
            //foreach (SackFruit sf in _animatingFruitList)
            {
                Destroy(_animatingFruitList[i]._Fruit);
                //_animatingFruitList.Remove(sf);
            }
            //_animatingFruitList.Remo*/
            _animatingFruitList.Clear();
        }
        if (_fruitList == null)
            _fruitList = new List<Fruit>();
        else
        {
            foreach (Fruit f in _fruitList)
                f.FruitTree.DestroyFruit(f);
            _fruitList.Clear();
        }
        _currentStackIndex = 0;

        _maxCount = _currentCapacity;
        _speedLoss = 0f;
        _state = S_STATE.IDLE;
    }

    public void Stop()
    {
        Debug.Log("Stopppppp SSSSack");
        _state = S_STATE.STOP;
    }
    /// <summary>
    /// 
    /// </summary>
    public void ResetMods()
    {
        _currentCapacity = _defaultCapacity;
        _currentReloadTime = _reloadTime;
    }

    /// <summary>
    /// Try to push a fruit inside sack
    /// </summary>
    /// <param name="f"></param>
    /// <returns>if operation succeeded</returns>
    public bool TryToPushToSack(Fruit f)
    {
        Debug.Log("Try to push");
        if (_state != S_STATE.RELOADING && _state != S_STATE.BROKEN)
        {
            if (f._Ftype != Fruit.F_TYPE.SACK_BREAKER && _fruitList.Count != _maxCount)
            {
                PushToSack(f);
                return true;
            }
            else
            {
                if (f._Ftype == Fruit.F_TYPE.SACK_BREAKER)
                    _Collector.Stun();
                return false;
            }
        }
        else
        {
            if (SackFullEvt != null)
                SackFullEvt();
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Reload()
    {
        if (_state == S_STATE.RELOADING ||_fruitList.Count == 0 ||_state == S_STATE.STOP)
            return;

        //Event call for any subscribed client(ie Tutorial)
        if (SackReloadEvt != null)
            SackReloadEvt();

        _state = S_STATE.RELOADING;
        _reloadPerFruitTime = _currentReloadTime / _fruitList.Count;
        _reloadTimer = _reloadPerFruitTime; //firs fruit pop is instant
        _Collector.Reload(/*_reloadTime*/);
    }

    /// <summary>
    /// 
    /// </summary>
    public void DismissAll()
    {
        foreach (Fruit f in _fruitList)
        {
            f.transform.position = _Collector.transform.position;
            f.Dissmiss();
        }
        _fruitList.Clear();     
    }

    /// <summary>
    /// Sack spins 720º during _stunTime Dismiss() fruits every _stunTime/_fruitCount, 
    /// first 1 at t=0; (n-1) each _stunTime/_fruitCount; Unparent fruit from sack, 
    /// move to world sack spot, remove from stack/animation    
    /// </summary>
    public void SackBroken()
    {
        LeanTween.rotateZ(_Collector._CollectorSack, 720f, _Collector._StunTime);
        //Dismiss first 1 autoatically
        _brokenFrCount = _fruitList.Count;
        
        if (_fruitList.Count != 0)
            UnstackAndDissmiss();

        _sackFillImg.fillAmount = (float)_fruitList.Count / _maxCount; //Update UI fill
        
        _state = S_STATE.BROKEN;
        _brokenTimer = 0f;
        _throwFrTimer = 0f;
        if (_brokenFrCount !=0)
            _throwFrTime = _Collector._StunTime / _brokenFrCount;
    }

    /// <summary>
    /// Push to Sack an egg under Fruit fr
    /// All fruits from fr to top seeks up.
    /// If Sack is full, Dismiss top fruit
    /// </summary>
    /// <param name="fr"></param>
    public void PushEgg(Fruit fr)
    {
        int frStackIndex = -1;
        Fruit tempEgg = null;
        Fruit pushedFr = null;
        Fruit chicken = null;

        frStackIndex = _fruitList.FindIndex((f) => (f == fr));
        if (frStackIndex == -1)
            Debug.Log("Chicken not found to spawn an egg");
        else
        {
            Debug.Log("Pushing egg from " + frStackIndex);
            Fruit temp = null;

            //(1) Save Chicken ref
            chicken = fr;
            Debug.Log("MAx count and count is :" + _maxCount + " / " + _fruitList.Count);
            //(3) If List full,last top will get dismissed, else, duplicate last to seek iteration
            if (_fruitList.Count == _maxCount)
            {
                pushedFr = _fruitList[_maxCount - 1];
                _fruitList.RemoveAt(_maxCount - 1);
            }
            /*else
                _fruitList.Add(_fruitList[_currentStackIndex]);*/

            //(2) Insert Egg at position
            tempEgg = GameMgr.Instance._FruitTree.GetEggFruit(fr.EggSpawnQuality);
            Debug.Log("Spawned egg with quality: " + tempEgg.EggSpawnQuality);
            tempEgg.transform.position = _stackEndPt.position + Vector3.up * (frStackIndex * _fruitStackHeightOffset);
            tempEgg.transform.parent = transform;
            tempEgg.gameObject.SetActive(true);
            _fruitList.Insert(frStackIndex, tempEgg);
            Debug.Log("fruit list count after insert: " + _fruitList.Count);
            for (int i = 0; i < _fruitList.Count; ++i)
                Debug.Log("---------------->>>> " + _fruitList[i]._Ftype);
            //_animatingFruitList.Insert(frStackIndex, new SackFruit(tempEgg, (Vector2)_stackEndPt.position + Vector2.up * (frStackIndex * _fruitStackHeightOffset)));

            for (int i = frStackIndex+1; i < _fruitList.Count; ++i)
            {
                //if (i!=_maxCount-1)
                    LeanTween.move(_fruitList[i].gameObject, _fruitList[i].transform.position + Vector3.up * _fruitStackHeightOffset, _eggSpawnTime).setEase(_eggSpawnAC);
            }
            //(4) Seek up elements in top of inserted egg

            //LeanTween.move(_fruitList[frStackIndex].gameObject, _fruitList[frStackIndex].transform.position + Vector3.up * _fruitStackHeightOffset, _eggSpawnTime).setEase(_eggSpawnAC);
            
            //(5) Dissmis element if needed
            if (pushedFr != null)
            {
                LeanTween.move(pushedFr.gameObject, pushedFr.transform.position + Vector3.up * _fruitStackHeightOffset, _eggSpawnTime*0.4f).setEase(_eggSpawnAC).setOnComplete(() =>
                {
                    Debug.Log("Di s s misss Chicken");
                    pushedFr.transform.parent = GameMgr.Instance._FruitTree.FruitPoolRoot;   //TODO: egg pool parent
                    pushedFr.transform.position = _Collector._CollectorSack.transform.position;
                    pushedFr.Dissmiss();
                });
                
            }else
                ++_currentStackIndex;
        }
    }
    #endregion


    #region Private Methods
    /// <summary>
    /// 
    /// </summary>
    private void UnstackAndDissmiss()
    {
        Fruit temp = null;

        //temp = _fruitList.Pop();
        temp = _fruitList[_currentStackIndex];
        _fruitList.RemoveAt(_currentStackIndex);
        --_currentStackIndex;
        temp.transform.parent = GameMgr.Instance._FruitTree.FruitPoolRoot;   //TODO: check if must returned to pool or gets auto attached on destroy
        temp.transform.position = _Collector._CollectorSack.transform.position;
        temp.Dissmiss();
        _sackFillImg.fillAmount = (float)_fruitList.Count / _maxCount; //Update UI fill

    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="f"></param>
    private void PushToSack(Fruit f)
    {
        Debug.Log("_________pushing to sack");
        f.DepositOnSack();
        f.transform.SetParent(transform);
        _fruitList.Add(f);
        ++_currentStackIndex;
        UpdateUISack(f);

        //Update speed
        _speedLoss += _lossSpeedPerFruit;
        _Collector.ReduceSpeed(_speedLoss);

        AudioController.Play("aud_fr_catch_0");
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateUISack(Fruit f)
    {
        _animatingFruitList.Add(new SackFruit(f, (Vector2)_stackEndPt.position + Vector2.up * (_fruitList.Count * _fruitStackHeightOffset)));
        if (_state != S_STATE.ANIMATING)
            _state = S_STATE.ANIMATING;
        _initFillValue = _sackFillImg.fillAmount;
        _targetFillValue = (float)_fruitList.Count / _maxCount;
        _fillingSack = true;
        
    }
	#endregion


	#region Properties
    public CollectorMonkey _Collector;
    public float ReloadTime { get { return _reloadTime; } set { _reloadTime = value; } }
    public float CurrentReloadTime { get { return _currentReloadTime; } set { _currentReloadTime = value; } }
    public int Capacity { get { return _defaultCapacity; } set { _defaultCapacity = value; } }
    public int CurrentCapacity { get { return _currentCapacity; } set { _currentCapacity = value; } } 
	#endregion

	#region Private Serialized Fields
    //UI
    [SerializeField]
    private AnimationCurve _fruitStackAC;
    [SerializeField]
    private AnimationCurve _eggSpawnAC;
    [SerializeField]
    private float _sackFruitAnimationTime;
    [SerializeField]
    private float _eggSpawnTime;
    [SerializeField]
    private Transform _stackEndPt;
    [SerializeField]
    private Transform _stackEnterPt;
    [SerializeField]
    private int _initUICapacity;

    [SerializeField]
    private float _lossSpeedPerFruit;
    [SerializeField]
    private float _rotModifierSpeed;        //increaes/decreases fruit rot behaviour
    [SerializeField]
    private float _reloadTime;
    [SerializeField]
    private int _defaultCapacity;

    [SerializeField]
    private FruitTree _fTree;

    [SerializeField]
    private Image _sackFillImg;
    [SerializeField]
    private float _fillTime;
    [SerializeField]
    private AnimationCurve _fillAnimCurve;
	#endregion

	#region Private Non-serialized Fields
    private S_STATE _state;
    private List<Fruit> _fruitList;
    private int _maxCount;

    //UI
    [SerializeField]
    private float _fruitStackHeightOffset;  //y offset between stored fruits
    [SerializeField]
    private Vector2 _collectedAnimationOffset;
    [SerializeField]
    private float _collectedAnimTime;

    private List<SackFruit> _animatingFruitList;   //store all references of fruit entering in the sack's stack and its associated animation time
    private int _animatingFruitCount;

    private float _speedLoss;

    private Fruit _tempFruit;
    private float _currentReloadTime;
    private float _reloadTimer;
    private float _reloadPerFruitTime;

    private int _currentCapacity;

    private bool _fillingSack;
    private float _initFillValue, _targetFillValue;
    private float _fillTimer;

    private int _brokenFrCount;
    private float _brokenTimer;
    private float _throwFrTime, _throwFrTimer;

    private int _currentStackIndex; //top of list
    #endregion
}
