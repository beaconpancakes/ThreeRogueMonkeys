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
public class TapMonkey : MonoBehaviour {

	#region Public Data
    public enum MONKEY_STATE { IDLE = 0, MOVING_TO_POS, HIT_ANIMATION, RECOVERING_TO_FLOOR, FLEE }
	#endregion


	#region Behaviour Methods

	// Use this for initialization
	void Start () {
		_img = GetComponent<Image>();
        if (_img == null)
            Debug.LogError("No img!");
        _gameMgr = GameMgr.Instance;
        if (!_gameMgr)
            Debug.LogError("No GameMgr found!");
        _currentJumpSpeed = _jumpSpeed;
        _currentRecoverSpeed = _recoverSpeed;
	}
	

	// Update is called once per frame
	void Update () {
		//Monkey Animation
        switch (_state)
        {
            case MONKEY_STATE.IDLE:
                _frameTimer += Time.deltaTime;
                if (_frameTimer >= _gameMgr.FrameTime)
                {
                    _frameTimer = 0f;
                    _frameIndex = (_frameIndex + 1) % _idleSpList.Count;
                }
                _img.sprite = _idleSpList[_frameIndex];
                break;

            case MONKEY_STATE.MOVING_TO_POS:
                //Animation
                _frameTimer += Time.deltaTime;
                if (_frameTimer >= _gameMgr.FrameTime)
                {
                    _frameTimer = 0f;
                    _frameIndex = (_frameIndex + 1) % _idleSpList.Count;
                }
                _img.sprite = _idleSpList[_frameIndex];

                _nextPos =  (Vector2)transform.position +  (_targetPos - (Vector2)transform.position).normalized * Time.deltaTime * _currentJumpSpeed;

                if (_nextPos.y > _targetPos.y)
                {
                    transform.position = _targetPos;
                    PerformHit();
                }
                else
                    transform.position = _nextPos;
                break;

            case MONKEY_STATE.HIT_ANIMATION:
                _frameTimer += Time.deltaTime;
                _hitTimer += Time.deltaTime;
                if (_frameTimer >= _gameMgr.FrameTime)
                {
                    _frameTimer = 0f;
                    _frameIndex = (_frameIndex + 1) % _hitSpList.Count;
                    _img.sprite = _hitSpList[_frameIndex];
                }
                if (_hitTimer >=_hitTime)
                    RecoverToFloor();

                break;

            case MONKEY_STATE.RECOVERING_TO_FLOOR:
                _timer += Time.deltaTime;
                _nextPos = (Vector2)transform.position - _currentRecoverSpeed * Vector2.up * Time.deltaTime;

                if (_nextPos.y <= _gameMgr.FloorYPos)
                {
                    transform.position = new Vector2(transform.position.x, _gameMgr.FloorYPos);
                    SetIdle();
                }
                else
                    transform.position = _nextPos;
                break;

            case MONKEY_STATE.FLEE:
                //Sprite
                _frameTimer += Time.deltaTime;
                if (_frameTimer >= _gameMgr.FrameTime)
                {
                    _frameTimer = 0f;
                    _frameIndex = (_frameIndex + 1) % _fleeSpList.Count;
                    _img.sprite = _fleeSpList[_frameIndex];
                }

                transform.position += Vector3.right * _fleeSpeed * Time.deltaTime;
                break;
        }

        if (_showingHitFeedback)
        {
            _hitFeedbackTimer += Time.deltaTime;
            if (_hitFeedbackTimer >= _hitFeedbackImgTime)
            {
                _showingHitFeedback = false;
                _hitFeedbackImg.gameObject.SetActive(false);
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
        _currentCollisionRadius = _collisionRadius;
        _currentRecoverSpeed = _recoverSpeed;
        _currentJumpSpeed = _jumpSpeed;
        _currentAccuracy = _accuracy;

        if (_slotA != null)
            LoadSlotItem(_slotA);
        if (_slotB != null)
            LoadSlotItem(_slotB);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    public void MoveToHit(Vector2 position)
    {
        if (_state != MONKEY_STATE.IDLE)
            return;

        _state = MONKEY_STATE.MOVING_TO_POS;
        _targetPos = position;
        //transform.position = position;
        _frameTimer = 0f;
        _frameIndex = 0;
        _img.sprite = _movSpList[0];
    }

    /// <summary>
    /// 
    /// </summary>
    public void PerformHit()
    {
        _state = MONKEY_STATE.HIT_ANIMATION;
        //transform.position = position;
        _frameTimer = 0f;
        _frameIndex = 0;
        _hitTimer = 0f;
        _img.sprite = _movSpList[0];
        _hitFeedbackImg.transform.position = transform.position + _hitOffset;
        _hitFeedbackImg.transform.localScale = (float)(_currentCollisionRadius / _collisionRadius) * Vector3.one;
        _hitFeedbackImg.gameObject.SetActive(true);
        _hitFeedbackTimer = 0f;
        _showingHitFeedback = true;

        _hit2DArray = Physics2D.CircleCastAll(transform.position, _currentCollisionRadius, Vector2.up, 0f, LayerMask.GetMask("Fruit"));
        Debug.DrawLine(transform.position, transform.position + Vector3.right * _collisionRadius, Color.red);
        //Debug.DrawRay(GameCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.up * 20f, Color.red, 5f);
        foreach (RaycastHit2D hit2D in _hit2DArray)
        {
            Debug.Log("H I T !");
            if (hit2D != null && hit2D.collider != null)
            {
                //Set monkey hitting coco on position + throw coco
                hit2D.collider.GetComponent<Fruit>().Launch();
            }
        }
        
    }

    /// <summary>
    /// 
    /// </summary>
    public void Flee()
    {
        //TODO
        Debug.Log("Flee! - striker");
        _frameTimer = 0f;
        _state = MONKEY_STATE.FLEE;
        _img.sprite = _fleeSpList[0];
    }

    /// <summary>
    /// 
    /// </summary>
    public void Reset()
    {
        _state = MONKEY_STATE.IDLE;
        _frameTimer = 0f;
        _frameIndex = 0;
        _img.sprite = _idleSpList[0];
        _hitFeedbackImg.gameObject.SetActive(false);
        _hitFeedbackTimer = 0f;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public float GetAccuracy()
    {
        return _currentAccuracy;
    }
	#endregion


	#region Private Methods
    /// <summary>
    /// 
    /// </summary>
    private void RecoverToFloor()
    {
        _state = MONKEY_STATE.RECOVERING_TO_FLOOR;
        _frameIndex = 0;
        _frameTimer = 0f;
        _img.sprite = _recoveringSpList[0];
        //_initPos = transform.position;
        //Calculate time to get pos ecuation
        _recoveringTime = Mathf.Sqrt((-2f * (transform.position.y - GameMgr.Instance.FloorYPos)) / _recoveringGravity);
        _timer = 0f;
        Debug.Log("Recovering time is: " + _recoveringTime);
     }

    /// <summary>
    /// 
    /// </summary>
    private void SetIdle()
    {
        _state = MONKEY_STATE.IDLE;
        _frameIndex = 0;
        _frameTimer = 0f;
        _img.sprite = _idleSpList[0];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eI"></param>
    private void LoadSlotItem(EquipmentItem eI)
    {
        for (int i = 0; i < eI.ModTypeList.Count; ++i)
        //foreach (EquipmentItem.MOD_TYPE mt in eI.ModTypeList)
        {
            switch (eI.ModTypeList[i])
            {
                case EquipmentItem.MOD_TYPE.STRIKER_HIT_SIZE:
                    _currentCollisionRadius += _collisionRadius * eI.ModValueList[i];
                    break;

                case EquipmentItem.MOD_TYPE.STRIKER_SPEED:
                    _currentJumpSpeed += _jumpSpeed *  eI.ModValueList[i];
                    _currentRecoverSpeed += _recoverSpeed * eI.ModValueList[i];
                    break;

                case EquipmentItem.MOD_TYPE.ACURRACY:
                    //TODO
                    _currentAccuracy += _accuracy * eI.ModValueList[i];
                    break;
            }
        }
    }
	#endregion


	#region Properties
    public EquipmentItem SlotA { get { return _slotA; } set { _slotA = value; } }
    public EquipmentItem SlotB { get { return _slotB; } set { _slotB = value; } }
	#endregion

	#region Private Serialized Fields
    [SerializeField]
    private List<Sprite> _idleSpList;
    [SerializeField]
    private List<Sprite> _movSpList;
    [SerializeField]
    private List<Sprite> _hitSpList;
    [SerializeField]
    private List<Sprite> _recoveringSpList;
    [SerializeField]
    private List<GameObject> _shadowSpriteList; //sprites used as trail****
    [SerializeField]
    private List<Sprite> _fleeSpList;
    [SerializeField]
    private float _recoveringGravity;
    [SerializeField]
    private float _jumpSpeed;
    [SerializeField]
    private float _recoverSpeed;
    [SerializeField]
    private float _hitTime;
    [SerializeField]
    private float _minArrivalOffset;
    //TODO: precision
    [SerializeField]
    private float _collisionRadius;
    [SerializeField]
    private float _accuracy;
    [SerializeField]
    private float _fleeSpeed;
    [SerializeField]
    private Image _hitFeedbackImg;
    [SerializeField]
    private float _hitFeedbackImgTime;
    [SerializeField]
    private Vector3 _hitOffset;//offset from striker center to  hit position
	#endregion

	#region Private Non-serialized Fields
    private MONKEY_STATE _state;

    //Items
    private EquipmentItem _slotA, _slotB;

    private GameMgr _gameMgr;
    private Image _img; 
    private int _frameIndex;
    private float _frameTimer;
    private float _hitTimer;

    private float _currentJumpSpeed, _currentRecoverSpeed;
    private float _currentCollisionRadius;
    private float _currentAccuracy;

    private Vector2 _initPos;
    private float _recoveringTime;
    private float _timer;
    private Vector2 _targetPos;
    private RaycastHit2D[] _hit2DArray;

    private Vector2 _nextPos;

    private float _hitFeedbackTimer;
    private bool _showingHitFeedback;
	#endregion
}
