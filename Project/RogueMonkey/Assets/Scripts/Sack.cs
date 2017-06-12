/************************************************************************/
/* @Author: Author Name
 * @Date: Date
 * @Brief: BBBrief
 * @Description: DDDescription
 * ***********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sack : MonoBehaviour {

	#region Public Data
    public enum S_STATE { IDLE = 0, ANIMATING, RELOADING, STOP }

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


                    _tempFruit = _fruitStack.Pop();
                    _tempFruit.Collect();
                    if (FruitCollectedEvt!=null)
                        FruitCollectedEvt();
                    _tempFruit.SetCollectedAnimation(new Vector2(_tempFruit.transform.position.x + _collectedAnimationOffset.x, _tempFruit.transform.position.y + (_stackEnterPt.transform.position.y - _tempFruit.transform.position.y) + _collectedAnimationOffset.y), _collectedAnimTime);
                    
                    //Update speed
                    _speedLoss -= _lossSpeedPerFruit;
                    _Collector.ReduceSpeed(_speedLoss);

                    //Animation
                    /*LeanTween.cancelAll(_tempFruit);
                    LeanTween.move(_tempFruit.gameObject, new Vector2(_tempFruit.transform.position.x + _collectedAnimationOffset.x, ), _collectedAnimTime);
                    LeanTween.rotateZ(_tempFruit.gameObject, 360f, _collectedAnimTime).setOnComplete(() =>
                    {
                        _tempFruit.gameObject.SetActive(false);
                    });*/

                    //Reloading finished
                    if (_fruitStack.Count == 0)
                        _state = S_STATE.IDLE;
                }

                    

                    
                break;
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
        
        if (_fruitStack == null)
            _fruitStack = new Stack<Fruit>();
        else
            _fruitStack.Clear();

        _maxCount = _currentCapacity;
        Debug.Log("Max count is : " + _maxCount);
        _speedLoss = 0f;
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
        if (_fruitStack == null)
            _fruitStack = new Stack<Fruit>();
        else
        {
            foreach (Fruit f in _fruitStack)
                f.FruitTree.DestroyFruit(f);
            _fruitStack.Clear();
        }

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
        if (_state == S_STATE.RELOADING || _fruitStack.Count == _maxCount)
        {
            if (SackFullEvt!=null)
                SackFullEvt();
            return false;
        }
        else
        {
            PushToSack(f);
            return true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Reload()
    {
        if (_state == S_STATE.RELOADING ||_fruitStack.Count == 0 ||_state == S_STATE.STOP)
            return;

        //Event call for any subscribed client(ie Tutorial)
        if (SackReloadEvt != null)
            SackReloadEvt();

        _state = S_STATE.RELOADING;
        _reloadPerFruitTime = _currentReloadTime / _fruitStack.Count;
        _reloadTimer = _reloadPerFruitTime; //firs fruit pop is instant
        _Collector.Reload(/*_reloadTime*/);
    }

    #endregion


    #region Private Methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="f"></param>
    private void PushToSack(Fruit f)
    {
        Debug.Log("_________pushing to sack");
        f.DepositOnSack();
        f.transform.SetParent(transform);
        _fruitStack.Push(f);
        UpdateUISack(f);

        //Update speed
        _speedLoss += _lossSpeedPerFruit;
        _Collector.ReduceSpeed(_speedLoss);
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateUISack(Fruit f)
    {
        _animatingFruitList.Add(new SackFruit(f, (Vector2)_stackEndPt.position + Vector2.up * (_fruitStack.Count * _fruitStackHeightOffset)));
        if (_state != S_STATE.ANIMATING)
            _state = S_STATE.ANIMATING;
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
    private float _sackFruitAnimationTime;
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
	#endregion

	#region Private Non-serialized Fields
    private S_STATE _state;
    private Stack<Fruit> _fruitStack;
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
	#endregion
}
