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

public class Guard : MonoBehaviour {

	#region Public Data
    public enum GUARD_STATE { IDLE = 0, WARNED, WAKING_UP, ASLEEP }
	#endregion


	#region Behaviour Methods
	// Use this for initialization
	void Start () {
        _img = GetComponent<Image>();
        if (_img == null)
            Debug.LogError("No img found!");
	}
	
	// Update is called once per frame
	void Update () {
        switch (_state)
        {
            case GUARD_STATE.IDLE:
                _frameTimer += Time.deltaTime;
                if (_frameTimer >= _frameTime)
                {
                    _frameTimer = 0f;
                    _currentFrameIndex = (_currentFrameIndex + 1) % _idleSpList.Count;
                }
                break;

            case GUARD_STATE.WARNED:
                _frameTimer += Time.deltaTime;
                break;

            case GUARD_STATE.WAKING_UP:
                _frameTimer += Time.deltaTime;
                if (_frameTimer >= _frameTime)
                {
                    _frameTimer = 0f;
                    if (_currentFrameIndex < _wakeUpSpList.Count - 2)
                    {
                        ++_currentFrameIndex;
                        _img.sprite = _wakeUpSpList[_currentFrameIndex];
                    }
                }

                _timer += Time.deltaTime;
                if (_timer >= _wakeUpTime)
                {
                    GameMgr.Instance.GuardWokenUp();
                    _state = GUARD_STATE.ASLEEP;
                    _frameTimer = 0f;
                    _timer = 0f;
                }
                break;

            case GUARD_STATE.ASLEEP:
                _frameTimer += Time.deltaTime;
                break;
        }
	}
	#endregion

	#region Public Methods
    public bool CheckAlarm(float noseHorizontalPt)
    {
        if (noseHorizontalPt > transform.position.x)
        {
            if (noseHorizontalPt - transform.position.x <= _alarmRadius)
            {
                Alarm();
                return true;
            }
            return false;
        }
        else
        {
            if ((transform.position.x - noseHorizontalPt) <= _alarmRadius)
            {
                Alarm();
                return true;
            }
            return false;
        }
    }

    public void SetupGuard()
    {
        _state = GUARD_STATE.IDLE;
        _timer = 0f;
        _frameTimer = 0f;
    }
	#endregion


	#region Private Methods
    private void Alarm()
    {
        ShowAlarmFeedback();
        if (GameMgr.Instance.RaiseAlarm(_alarmLvlIncrement))
            WakeUp();
    }

    private void ShowAlarmFeedback()
    {

    }

    private void WakeUp()
    {
        _state = GUARD_STATE.WAKING_UP;
        _frameTimer = 0f;
        _currentFrameIndex = 0;
    }
	#endregion


	#region Properties

	#endregion

	#region Private Serialized Fields
    [SerializeField]
    private float _alarmLvlIncrement;       //amount incremented when a fruit is missed while falling from tree

    //Animation
    [SerializeField]
    private List<Sprite> _idleSpList;
    [SerializeField]
    private List<Sprite> _wakeUpSpList;
    [SerializeField]
    private List<Sprite> _asleepSpList;
    [SerializeField]
    private float _frameTime;
    [SerializeField]
    private float _alarmRadius; //to check if a fallen fruit alarms the guard

    [SerializeField]
    private float _wakeUpTime;


	#endregion

	#region Private Non-serialized Fields
    private GUARD_STATE _state;

    private Image _img;
    private float _frameTimer, _timer;
    private int _currentFrameIndex;
	#endregion
}
