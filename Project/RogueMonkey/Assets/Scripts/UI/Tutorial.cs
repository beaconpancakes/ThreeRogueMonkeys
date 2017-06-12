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
using UnityEngine.SceneManagement;

public class Tutorial : MonoBehaviour {

	#region Public Data
    public enum TUT_STATE { IDLE = 0, INTRO, COLLECTOR_MOV, TESTING_COLLECTOR, STRIKER_MOV, TESTING_STRIKER, SHAKER, RUNNING, SACK_FULL, WAITING_FOR_RELOAD, COLLECT_FRUIT, LAST_RUN, FINISHED, BACK_TO_MENU, ALARM_1, ALARM_2, ALARM_ENDED }

    public static Tutorial Instance;
	#endregion


	#region Behaviour Methods
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
            Instance = this;   
        DontDestroyOnLoad(gameObject);
    }
	// Use this for initialization
	void Start () {
        _state = TUT_STATE.IDLE;
        _initAlphaValue = _collectorHighlightArea.color.a;
	}
	
	// Update is called once per frame
	void Update () {
        switch (_state)
        {

            case TUT_STATE.COLLECTOR_MOV:
                _timer += Time.deltaTime;
                if (_timer >= _highlightFadeTime)
                {
                    _timer = 0f;
                    float aux = _endAlphaValue;
                    _endAlphaValue = _initAlphaValue;
                    _initAlphaValue = aux;
                }
                _collectorHighlightArea.color = new Color(_collectorHighlightArea.color.r, _collectorHighlightArea.color.g, _collectorHighlightArea.color.b, Mathf.Lerp(_initAlphaValue, _endAlphaValue, _timer / _highlightFadeTime));
                break;
            case TUT_STATE.TESTING_COLLECTOR:
                

                _prevCollectorPos = _currentCollectorPos;
                _currentCollectorPos = GameMgr.Instance._CollectorMonkey.transform.position.x;
                _collectorSlideDist += Mathf.Abs(_currentCollectorPos - _prevCollectorPos);
                if (_collectorSlideDist >= _collectorDistToNext)
                    Next();
                break;

            case TUT_STATE.STRIKER_MOV:
                _timer += Time.deltaTime;
                if (_timer >= _highlightFadeTime)
                {
                    _timer = 0f;
                    float aux = _endAlphaValue;
                    _endAlphaValue = _initAlphaValue;
                    _initAlphaValue = aux;
                }
                _strikerHighlightArea.color = new Color(_strikerHighlightArea.color.r, _strikerHighlightArea.color.g, _strikerHighlightArea.color.b, Mathf.Lerp(_initAlphaValue, _endAlphaValue, _timer / _highlightFadeTime));
                break;
            case TUT_STATE.TESTING_STRIKER:
                if (Input.GetMouseButtonDown(0))
                {
                    ++_strikerMovCount;
                    if (_strikerMovCount >= _strikerCountToNext)
                        Next();
                }
                break;
        }
	}
	#endregion

	#region Public Methods
    public void InitTutorial()
    {
        Debug.Log("Init tutorial!");

        if (_sack == null)
            _sack = GameMgr.Instance._CollectorMonkey._Sack;
        _sack.SackFullEvt += new Sack.OnSackFullEvent(SackFullEvtHandler);
        _sack.SackReloadEvt += new Sack.OnSackReloadEvent(SackReloadEvtHandler);
        _sack.FruitCollectedEvt += new Sack.OnFruitCollectedEvent(FruitCollectedEvtHandler);
        GameMgr.Instance.AlarmRaisedEvt += new GameMgr.OnAlarmRaisedEvt(AlarmRaisedEvtHandler);
        _welcomeHelp.SetActive(true);
        GameMgr.Instance.GetCurrentLevel().LevelTime = 60f;
        UIHelper.Instance.SetLvlScreenTime(60f);
        UIHelper.Instance.ShowLevelTime(false);
        _alarmTipShown = false;
        //_mainMenu.TutorialReady();
        _state = TUT_STATE.IDLE;
        //GameMgr.Instance.AlarmIncrease = 5f;
        Next();
    }
    
    public void Next()
    {
        ++_state;
        DoAction(_state);
    }

    /// <summary>
    /// 
    /// </summary>
    public void CloseTutorial()
    {
        UIHelper.Instance.ShowLevelTime(true);
        _sack.SackFullEvt -= new Sack.OnSackFullEvent(SackFullEvtHandler);
        _sack.SackReloadEvt -= new Sack.OnSackReloadEvent(SackReloadEvtHandler);
        _sack.FruitCollectedEvt -= new Sack.OnFruitCollectedEvent(FruitCollectedEvtHandler);
        GameMgr.Instance.AlarmRaisedEvt -= new GameMgr.OnAlarmRaisedEvt(AlarmRaisedEvtHandler);

        SceneManager.UnloadSceneAsync(GameMgr.Instance.GetCurrentLevel().SceneLayoutId);
        SceneManager.LoadScene("Menu"); 
        //_mainMenu.ShowTutorial(false);
    }

    
	#endregion


	#region Private Methods
    private void DoAction(TUT_STATE st)
    {
        switch (st)
        {
            case TUT_STATE.IDLE:

                break;

            case TUT_STATE.INTRO:
                Debug.Log("Intro");
                _welcomeHelp.SetActive(true);
                GameMgr.Instance.Pause(true);
                _blackBground.SetActive(true);
                break;

            case TUT_STATE.COLLECTOR_MOV:
                _welcomeHelp.SetActive(false);
                _collectorHelp.SetActive(true);
                _timer = 0f;
                _collectorHighlightArea.gameObject.SetActive(true);

                break;

            case TUT_STATE.TESTING_COLLECTOR:
                _collectorHighlightArea.gameObject.SetActive(false);
                _blackBground.SetActive(false);
                _collectorHelp.SetActive(false);
                GameMgr.Instance.Pause(false);
                GameMgr.Instance._FruitTree.Pause(true);
                GameMgr.Instance.GetCurrentLevel().Pause(true);
                _strikerMovCount = 0;
                break;
            case TUT_STATE.STRIKER_MOV:
                _strikerHighlightArea.gameObject.SetActive(true);
                _blackBground.SetActive(true);
                GameMgr.Instance.Pause(true);
                _strikerHelp.SetActive(true);
                break;

            case TUT_STATE.TESTING_STRIKER:
                _strikerHighlightArea.gameObject.SetActive(false);
                _blackBground.SetActive(false);
                _strikerHelp.SetActive(false);
                _collectorSlideDist = 0f;
                _currentCollectorPos = GameMgr.Instance._CollectorMonkey.transform.position.x;
                _prevCollectorPos = _currentCollectorPos;

                GameMgr.Instance.Pause(false);
                GameMgr.Instance._FruitTree.Pause(true);
                GameMgr.Instance.GetCurrentLevel().Pause(true);
                break;

            case TUT_STATE.SHAKER:
                _blackBground.SetActive(true);
                GameMgr.Instance.Pause(true);
                _shakerHelp.SetActive(true);
                break;

            case TUT_STATE.RUNNING:
                _collectedCount = 0;
                _blackBground.SetActive(false);
                GameMgr.Instance.Pause(false);
                _shakerHelp.SetActive(false);
                //TODO: run level
                break;

            case TUT_STATE.SACK_FULL:
                _blackBground.SetActive(true);
                _sackHelp.SetActive(true);
                GameMgr.Instance.Pause(true);
                //TODO: pause game
                break;

            case TUT_STATE.WAITING_FOR_RELOAD:
                _blackBground.SetActive(false);
                GameMgr.Instance.Pause(false);
                _sackHelp.SetActive(false);
                break;

            case TUT_STATE.COLLECT_FRUIT:
                GameMgr.Instance.Pause(true);
                _blackBground.SetActive(true);
                _lastRunHelp.SetActive(true);
                _collectedCount = 0;
                break;

            case TUT_STATE.LAST_RUN:
                GameMgr.Instance.Pause(false);
                _blackBground.SetActive(false);
                _lastRunHelp.SetActive(false);              
                break;

            case TUT_STATE.FINISHED:
                _blackBground.SetActive(true);
                _lastRunHelp.SetActive(false);
                _finishedHelp.SetActive(true);
                GameMgr.Instance.Pause(true);
                break;

            case TUT_STATE.BACK_TO_MENU:
                _blackBground.SetActive(false);
                _finishedHelp.SetActive(false);
                CloseTutorial();
                break;

            /*case TUT_STATE.ALARM_1:
                _alarmHelp_1.SetActive(true);
                _alarmHelp_2.SetActive(true);
                break;*/

            case TUT_STATE.ALARM_2:
                _alarmHelp_1.SetActive(false);
                _alarmHelp_2.SetActive(true);
                break;

            case TUT_STATE.ALARM_ENDED:
                _alarmHelp_2.SetActive(false);
                GameMgr.Instance.Pause(false);
                _blackBground.SetActive(false);
                _state = _lastState;
                Debug.Log("Recovering tutorial state: " + _state);
                break;
        }
    }

    //Event handler
    private void SackFullEvtHandler()
    {
        Debug.Log("Event recieved! - sack full");
        if (_state == TUT_STATE.RUNNING)
            Next(); 

    }

    private void SackReloadEvtHandler()
    {
        Debug.Log("Event recieved - sack reload");
        if (_state == TUT_STATE.WAITING_FOR_RELOAD)
            Next();
    }

    /// <summary>
    /// 
    /// </summary>
    private void FruitCollectedEvtHandler()
    {
        Debug.Log("Event recieved - fruit collected");
        if (_state == TUT_STATE.RUNNING)
        {
            ++_collectedCount;
            if (_collectedCount >= _collectedFruitToFinish)
            {
                _state = TUT_STATE.LAST_RUN;
                Next();
            }
        }
        if (_state == TUT_STATE.LAST_RUN)
        {
            ++_collectedCount;
            if (_collectedCount >= _collectedFruitToFinish)
                Next();
        }
    }

    private void AlarmRaisedEvtHandler()
    {
        if (_alarmTipShown)
        {
            /*if (GameMgr.Instance.CurrentAlarmLevel >= GameMgr.Instance.MaxAlarmLevel)
            {
                _state = TUT_STATE.IDLE;
            }*/
            return;
        }
            

        //if (_state == TUT_STATE.RUNNING || _state == TUT_STATE.LAST_RUN )
        {
            _lastState = _state;
            _alarmTipShown = true;
           _state = TUT_STATE.ALARM_1;
            GameMgr.Instance.Pause(true);
            _alarmHelp_1.gameObject.SetActive(true);
            _blackBground.SetActive(true);
        }
    }
	#endregion


	#region Properties

	#endregion

	#region Private Serialized Fields
    [SerializeField]
    private MainMenu _mainMenu;
    [SerializeField]
    private GameObject _welcomeHelp,_collectorHelp, _strikerHelp, _shakerHelp, _alarmHelp_1, _alarmHelp_2, _runHelp, _sackHelp, _lastRunHelp, _finishedHelp;
    [SerializeField]
    private float _collectorDistToNext;  //min distance needed to go to next step
    [SerializeField]
    private int _strikerCountToNext;
    [SerializeField]
    private int _collectedFruitToFinish;
    [SerializeField]
    private GameObject _blackBground;
    [SerializeField]
    private Image _collectorHighlightArea, _strikerHighlightArea;
    [SerializeField]
    private float _highlightFadeTime;
    [SerializeField]
    private float _endAlphaValue;
	#endregion

	#region Private Non-serialized Fields
    private TUT_STATE _state, _lastState;

    private Sack _sack;
    private int _strikerMovCount;
    private int _collectedCount;
    private float _collectorSlideDist, _currentCollectorPos, _prevCollectorPos;
    private float _timer;
    private float _initAlphaValue;
    private bool _alarmTipShown;
	#endregion
}
