﻿/************************************************************************/
/* @Author: Author Name
 * @Date: Date
 * @Brief: BBBrief
 * @Description: DDDescription
 * ***********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectorMonkey : MonoBehaviour {

	#region Public Data
    public enum C_STATE { IDLE = 0, MOVING, RELOADING, FLEE }
	#endregion


	#region Behaviour Methods
	// Use this for initialization
	void Start () {
        _state = C_STATE.IDLE;
        _gameMgr = GameMgr.Instance;
        _currentSpeed = _baseSpeed;
        _sack._Collector = this;
        _img = GetComponent<Image>();
        if (!_img)
            Debug.LogError("No image found!");
	}
	
	// Update is called once per frame
	void Update () {
        switch (_state)
        {
            case C_STATE.IDLE:

                break;

            case C_STATE.MOVING:
                if (_targetPosTouch.phase == TouchPhase.Canceled || _targetPosTouch.phase == TouchPhase.Ended)
                {
                    Debug.LogError("Wrong phase");
                }else
                {
                    //Move left
                    if (_gameMgr.GameCamera.ScreenToWorldPoint(_targetPosTouch.position).x < transform.position.x)
                    {
                        //Calculate next pos to avoid go over it
                        _nextXpos = transform.position.x - _currentSpeed * Time.deltaTime;
                        if (_nextXpos < _gameMgr.GameCamera.ScreenToWorldPoint(_targetPosTouch.position).x)
                        {
                            transform.position = new Vector3(_gameMgr.GameCamera.ScreenToWorldPoint(_targetPosTouch.position).x, _gameMgr.FloorYPos);
                            Stop();
                        }
                        else
                            transform.position = new Vector3(transform.position.x - _currentSpeed * Time.deltaTime, _gameMgr.FloorYPos);
                    }
                    else//Right
                    {
                        _nextXpos = transform.position.x + _currentSpeed * Time.deltaTime;
                        if (_nextXpos > _gameMgr.GameCamera.ScreenToWorldPoint(_targetPosTouch.position).x)
                        {
                            transform.position = new Vector3(_gameMgr.GameCamera.ScreenToWorldPoint(_targetPosTouch.position).x, _gameMgr.FloorYPos);
                            Stop();
                        }
                        else
                            transform.position = new Vector3(transform.position.x + _currentSpeed * Time.deltaTime, _gameMgr.FloorYPos);
                    }
                }
                break;

            case C_STATE.RELOADING:
                //Time
                _rTimer += Time.deltaTime;
                if (_rTimer >= _reloadTime)
                {
                    _state = C_STATE.IDLE;
                    _frameTimer = 0f;
                    _frameIndex = 0;
                    _img.sprite = _idleSpList[0];
                }
                else
                {
                    //Sprite
                    _frameTimer += Time.deltaTime;
                    if (_frameTimer >= _gameMgr.FrameTime)
                    {
                        _frameTimer = 0f;
                        _frameIndex = (_frameIndex + 1) % _reloadSpList.Count;
                        _img.sprite = _reloadSpList[_frameIndex];
                    }                    
                }
                break;

            case C_STATE.FLEE:
                //Sprite
                _frameTimer += Time.deltaTime;
                if (_frameTimer >= _gameMgr.FrameTime)
                {
                    _frameTimer = 0f;
                    _frameIndex = (_frameIndex + 1) % _fleeSpList.Count;
                    _img.sprite = _fleeSpList[_frameIndex];
                }
                transform.position -= Vector3.right * _fleeSpeed * Time.deltaTime;
                break;
        }
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="col"></param>
    void OnTriggerEnter2D(Collider2D col)
    {
        if (_gameMgr.GetCurrentLevel().GetLevelState() == Level.L_STATE.FINISHED)
            return;

        if (col.CompareTag("Fruit"))
        {
            if (!_sack.TryToPushToSack(col.GetComponent<Fruit>()))
            {
                col.GetComponent<Fruit>().Dissmiss();
            }

        }
    }
	#endregion

	#region Public Methods

    /// <summary>
    /// Apply equipped items mods
    /// </summary>
    public void LoadItemsStats()
    {
        //reset to default values
        _reloadTime = _sack.ReloadTime;
        _currentReloadTime = _reloadTime;
        _currentRawSpeed = _baseSpeed;
        _sack.ResetMods();
        _currentCapacity = _sack.Capacity;
        Debug.Log("RELOADING before loading items " + _currentReloadTime);
        if (_slotA != null)
            LoadSlotItem(_slotA);
        if (_slotB != null)
            LoadSlotItem(_slotB);
        _sack.CurrentCapacity = _currentCapacity;
        _sack.CurrentReloadTime = _currentReloadTime;
        _sack.InitSack();
        Debug.Log("RELOADING after loading items " + _currentReloadTime);
        
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="speedLoss"></param>
    public void ReduceSpeed(float speedLoss)
    {
        _currentSpeed = _currentRawSpeed - speedLoss;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Stop()
    {
        _state = C_STATE.IDLE;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetTouchPos"></param>
    public void Move(Touch targetTouchPos)
    {
        if (_state == C_STATE.RELOADING)
            return;

        _state = C_STATE.MOVING;
        _targetPosTouch = targetTouchPos;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Reload(/*float reloadTime*/)
    {
        _state = C_STATE.RELOADING;
        _frameTimer = 0f;
        _frameIndex = 0;
        //_reloadTime = reloadTime;
    }

    public void Flee()
    {
        //TODO
        Debug.Log("Flee! - collector");
        _frameTimer = 0f;
        _state = C_STATE.FLEE;
        _frameIndex = 0;
        _img.sprite = _fleeSpList[0];
    }

    public void Reset()
    {
        _currentRawSpeed = _baseSpeed;
        _state = C_STATE.IDLE;
        _frameTimer = 0f;
        _frameIndex = 0;
        _img.sprite = _idleSpList[0];
        _sack.Reset();
        
        
    }
	#endregion


	#region Private Methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="eI"></param>
    private void LoadSlotItem(EquipmentItem eI)
    {
        for (int i=0; i< eI.ModTypeList.Count; ++i)
        //foreach (EquipmentItem.MOD_TYPE mt in eI.ModTypeList)
        {
            switch (eI.ModTypeList[i])
            {
                case EquipmentItem.MOD_TYPE.SACK_SIZE:
                    _currentCapacity += Mathf.RoundToInt(eI.ModValueList[i]);
                    //TODO: collider size + sprite?
                    //fixed sacks?-> no
                    break;

                case EquipmentItem.MOD_TYPE.RELOAD_SPEED:
                    _currentReloadTime -= _reloadTime * eI.ModValueList[i];
                    //_sack.CurrentReloadTime -= _sack.ReloadTime * eI.ModValueList[i];
                    break;

                case EquipmentItem.MOD_TYPE.COLLECTOR_SPEED:
                    _currentSpeed += _baseSpeed * eI.ModValueList[i];
                    //_currentSpeed = _baseSpeed * (1f + eI.ModValueList[i]);
                    _currentRawSpeed = _currentSpeed;
                    break;
            }
        }
    }
	#endregion


	#region Properties
    public float ReloadTime { get { return _reloadTime; } set { _reloadTime = value; } }
    public Sack _Sack { get { return _sack; } set { _sack = value; } }
    public EquipmentItem SlotA { get { return _slotA; } set { _slotA = value; } }
    public EquipmentItem SlotB { get { return _slotB; } set { _slotB = value; } }

	#endregion

	#region Private Serialized Fields
    [SerializeField]
    private Sack _sack;
    [SerializeField]
    private float _baseSpeed;

    //Animation
    [SerializeField]
    private List<Sprite> _reloadSpList;
    [SerializeField]
    private List<Sprite> _idleSpList;
    [SerializeField]
    private List<Sprite> _fleeSpList;

    [SerializeField]
    private float _fleeSpeed;
	#endregion

	#region Private Non-serialized Fields
    private C_STATE _state;
    private GameMgr _gameMgr;
    private Image _img;
    private Touch _targetPosTouch;      //the sliding thumb the collector follows
    private float _currentSpeed;
    private float _currentRawSpeed;     //current sppeed with equipped items. Used to calculate _currentSpeed when applying SpeedLoss
    private float _nextXpos;
    

    //Reload
    private float _frameTimer;
    private int _frameIndex;

    private float _rTimer;
    private float _reloadTime;
    private float _currentReloadTime;
    private int _currentCapacity;
    //Items
    private EquipmentItem _slotA, _slotB;
	#endregion
}
