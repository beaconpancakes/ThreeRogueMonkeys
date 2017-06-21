/************************************************************************/
/* @Author: Author Name
 * @Date: Date
 * @Brief: BBBrief
 * @Description: DDDescription
 * ***********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitTree : MonoBehaviour {

	#region Public Data
    public enum PT_STATE { IDLE = 0, SPAWNING, ENDED }
	#endregion


	#region Behaviour Methods
	// Use this for initialization
	void Start () {
        //InitPalmTree();
	}
	
	// Update is called once per frame
	void Update () {
        switch (_state)
        {
            case PT_STATE.IDLE:

                break;

            case PT_STATE.SPAWNING:
                _timer += Time.deltaTime;
                if (_timer >= _currentSpawnTime)
                {
                    SpawnFruit();
                    _timer = 0f;
                }

                _treeAudioTimer += Time.deltaTime;
                if (_treeAudioTimer >= _currentAudioTime)
                {
                    _currentAudioTime = Random.Range(_minTreeAudioTime, _maxTreeAudioTime) + AudioController.Play("shake_1").clipLength;
                    _treeAudioTimer = 0f;
                }
                break;

            case PT_STATE.ENDED:

                break;
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
        _currentFruitFallTime = _fallFruitTime;
        _currentGoldSpawnChance = _goldSpawnChance;
        _currentEquipmentSpawnChance = _equipmentSpawnChance;
        _currentFruitFlyTime = _fruitFlyTime;
        if (_slotA != null)
            LoadSlotItem(_slotA);
        if (_slotB != null)
            LoadSlotItem(_slotB);

        GenerateFruitPool(_levelFruitSpawnTypeList, _currentGoldSpawnChance, _currentEquipmentSpawnChance);
    }

    /// <summary>
    /// 
    /// </summary>
    public void InitPalmTree()
    {
        //init coconut pool
        if (_fruitList != null)
            _fruitList.Clear();
        /*_fruitList = new List<Fruit>();
        for (int i = 0; i < _fruitInitCountPool; ++i)
        {
            _fruitList.Add((Fruit)Instantiate(_fruitPrefab, _fruitRoot).GetComponent<Fruit>());
            _fruitList[i].gameObject.SetActive(false);
        }*/
        _currentPoolIndex = 0;
        _currentClusterPoolIndex = 0;
        _currentMultifruitPoolIndex = 0;
        _state = PT_STATE.IDLE;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="spawnTime"></param>
    /*public void StartPalmTree(float spawnTime)
    {
        _currentSpawnTime = spawnTime;
        _timer = 0f;
        _state = PT_STATE.SPAWNING;
        _currentPoolIndex = 0;
    }*/

    

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fruitSpawnTypeList"></param>
    public void SetFruitPool(List<Level.FruitSpawn> fruitSpawnTypeList)
    {
        _levelFruitSpawnTypeList = fruitSpawnTypeList;
    }
    public void SetGoldPool(List<Level.ItemSpawn> probList)
    {
        _goldItemPoolType = probList;
    }
    public void SetEquipmentPool(List<Level.ItemSpawn> probList)
    {
        _equipmentItemPoolType = probList;
    }

    public void SetGoldSpawnChance(float ratio)
    {
        _goldSpawnChance = ratio;
    }

    public void SetEquipmentSpawnChance(float ratio)
    {
        _equipmentSpawnChance = ratio;
    }
    /// <summary>
    /// Create fruit poolusing ratio spawn values with shuffle bags
    /// </summary>
    /// <param name="fruitSpawnTypeList"></param>
    /// <param name="listSize">shuffle bag size: defines randomness</param>
    public void GenerateFruitPool(List<Level.FruitSpawn> fruitSpawnTypeList, float goldSpawnChance, float equipmentSpawnChance, int listSize = 1000)
    {
        float sum = 0;
        float nonValuableProb = 0f;
        Fruit temp = null;
        float rndom = -1;
        List<string> _auxIdList = null; 


        //(1) Check total sum = 1f
        foreach (Level.FruitSpawn fs in fruitSpawnTypeList)
            sum += fs.SpawnRatio;
        if (sum != 1f)
            Debug.LogError("Fruit prob distribution wrong!");

        //(2)Create shuffle bag based on chances and setup each one
        if (_fruitList != null)
            _fruitList.Clear();
        else
            _fruitList = new List<Fruit>();

        //Calculate non equipment or gold item spawn chance
        foreach (Level.FruitSpawn fs in fruitSpawnTypeList)
            nonValuableProb += fs.SpawnRatio;
        nonValuableProb -= (goldSpawnChance + equipmentSpawnChance);
        //Add non valuable fruits to pool
        foreach (Level.FruitSpawn fs in fruitSpawnTypeList)
        {
            for (int i = 0; i < Mathf.FloorToInt(nonValuableProb *fs.SpawnRatio * listSize); ++i)
            {
                temp = (Fruit)Instantiate(_fruitPrefab, _fruitRoot).GetComponent<Fruit>();
                /*if (fs.FruitTypeIndex == (int)Fruit.F_TYPE.GOLD_ITEM)
                    temp.SetupFruitAsGoldItem((int)_goldItemsTypePool[i]);*/
                temp.SetupFruit(fs.FruitTypeIndex);
                temp.gameObject.SetActive(false);
                _fruitList.Add(temp);
            }
        }
        //Add gold items
        //foreach (Level.FruitSpawn fs in fruitSpawnTypeList)
        //{
        Debug.Log("Generating " + Mathf.FloorToInt(goldSpawnChance * listSize) + " gold items");
        _auxIdList = GetPoolIdList(_goldItemPoolType, goldSpawnChance, listSize);
        for (int i = 0; i < _auxIdList.Count; ++i)
        {

            temp = (Fruit)Instantiate(_fruitPrefab, _fruitRoot).GetComponent<Fruit>();
            //TODO: golditempool type setup

            temp.SetupFruitAsGoldItem(_auxIdList[i]);
            temp.gameObject.SetActive(false);
            _fruitList.Add(temp);
        }
        //}

        //Add equipment items
        //foreach (Level.FruitSpawn fs in fruitSpawnTypeList)
        //{
        Debug.Log("Generating " + Mathf.FloorToInt(goldSpawnChance * listSize) + " equip items");
        _auxIdList = GetPoolIdList(_equipmentItemPoolType,equipmentSpawnChance, listSize);
        for (int i = 0; i < _auxIdList.Count; ++i)
        {
            temp = (Fruit)Instantiate(_fruitPrefab, _fruitRoot).GetComponent<Fruit>();
            temp.SetupFruitAsEquipmentItem(_auxIdList[i]);
            temp.gameObject.SetActive(false);
            _fruitList.Add(temp);
        }
        //}

        //(3)Check if we need cluster pool instantiating
        if (fruitSpawnTypeList.Find((fr) => (fr.FruitTypeIndex == (int)Fruit.F_TYPE.CLUSTER_SEED)) != null)
        {
            if (_clusterFruitPool != null)
                _clusterFruitPool.Clear();
            else
                _clusterFruitPool = new List<Fruit>();

            for (int i = 0; i < _maxClusterPoolCount; ++i)
            {
                temp = (Fruit)Instantiate(_fruitPrefab, _fruitRoot).GetComponent<Fruit>();
                temp.SetupFruit((int)Fruit.F_TYPE.CLUSTER_UNIT);
                temp.gameObject.SetActive(false);
                _clusterFruitPool.Add(temp);
            }
        }
        //(4)Check if we need multifruit pool instantiating
        if (fruitSpawnTypeList.Find((fr) => (fr.FruitTypeIndex == (int)Fruit.F_TYPE.MULTI_SEED)) != null)
        {
            if (_multiFruitPool != null)
                _multiFruitPool.Clear();
            else
                _multiFruitPool = new List<Fruit>();

            for (int i = 0; i < _maxMultifruitPoolCount; ++i)
            {
                temp = (Fruit)Instantiate(_fruitPrefab, _fruitRoot).GetComponent<Fruit>();
                temp.SetupFruit((int)Fruit.F_TYPE.MULTI_UNIT);
                temp.gameObject.SetActive(false);
                _multiFruitPool.Add(temp);
            }
        }

        Shuffle(_fruitList);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="spawnRatio"></param>
    /// <param name="fallSpeed"></param>
    public void SetFruitStats(float spawnRatio, float fallSpeed, float flyTime, float goldItemProb, float equipmentItemProb)
    {
        _currentSpawnTime = spawnRatio;
        _fallFruitTime = fallSpeed;
        _fruitFlyTime = flyTime;
        _goldSpawnChance = goldItemProb;
        _equipmentSpawnChance = equipmentItemProb;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fallSpeed"></param>
    public void SetFruitFallSpeed(float fallSpeed)
    {
        _currentFruitFallTime = fallSpeed;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="goldItemSpawnTypeList"></param>
    /// <param name="listSize"></param>
    /*public void SetGoldItemsPool(List<Level.ItemSpawn> goldItemSpawnTypeList, int listSize = 50)
    {
        float sum = 0f;
        Fruit temp = null;

        //(1) Check total sum = 1f
        foreach (Level.ItemSpawn gs in goldItemSpawnTypeList)
            sum += gs.SpawnRatio;
        if (sum != 1f && sum != 0f)
            Debug.LogError("Gold item prob distribution wrong!" +sum);

        //(2)Create shuffle bag based on type chance
        if (_goldItemsTypePool != null)
            _goldItemsTypePool.Clear();
        else
            _goldItemsTypePool = new List<Fruit.G_TYPE>();

        foreach (Level.ItemSpawn gs in goldItemSpawnTypeList)
        {
            for (int i = 0; i < Mathf.FloorToInt(gs.SpawnRatio * listSize); ++i)
            {
                _goldItemsTypePool.Add((Fruit.G_TYPE)gs.GoldTypeIndex);
            }
        }

        Shuffle(_goldItemsTypePool);
    }*/

    /// <summary>
    /// 
    /// </summary>
    public void SetIdle()
    {
        _state = PT_STATE.IDLE;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="pause"></param>
    public void Pause(bool pause)
    {
        if (pause && _state != PT_STATE.IDLE)
        {
            _lastState = _state;
            _state = PT_STATE.IDLE;

            for (int i = 0; i < _currentPoolIndex; ++i)
                _fruitList[i].Pause(true);
            for (int i = 0; i < _currentMultifruitPoolIndex; ++i)
                _multiFruitPool[i].Pause(true);
            for (int i = 0; i < _currentClusterPoolIndex; ++i)
                _clusterFruitPool[i].Pause(true);
        }
        else if (!pause && _state == PT_STATE.IDLE)
        {
            _state = _lastState;

            for (int i = 0; i < _currentPoolIndex; ++i)
                _fruitList[i].Pause(false);
            for (int i = 0; i < _currentMultifruitPoolIndex; ++i)
                _multiFruitPool[i].Pause(false);
            for (int i = 0; i < _currentClusterPoolIndex; ++i)
                _clusterFruitPool[i].Pause(false);
        }
        
    }
    /// <summary>
    /// 
    /// </summary>
    public void StartSpawn()
    {
        //_currentSpawnTime = _baseSpawnTime;
        _timer = 0f;
        _state = PT_STATE.SPAWNING;
        _currentPoolIndex = 0;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Stop(bool destroyFlyingFruits = true)
    {
        if (destroyFlyingFruits)
        {
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Fruit"))
            {
                if (go.GetComponent<Fruit>()._FState == Fruit.FRUIT_ST.FALLING_FROM_TREE)
                    go.SetActive(false); 
            }
        }
        _state = PT_STATE.ENDED;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Fruit GetClusterUnit()
    {
        if (_currentClusterPoolIndex >= _clusterFruitPool.Count)
            Debug.LogError("Index out of range!");

        Fruit returnFr = null;
        returnFr =_clusterFruitPool[_currentClusterPoolIndex];
        ++_currentClusterPoolIndex;
        return returnFr;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Fruit GetMultiUnit()
    {
        if (_currentMultifruitPoolIndex >= _multiFruitPool.Count)
            Debug.LogError("Index out of range!");


        
        Fruit returnFr = null;
        returnFr = _multiFruitPool[_currentMultifruitPoolIndex];
        ++_currentMultifruitPoolIndex;
        Debug.Log("Get multi index_________________________: " + _currentMultifruitPoolIndex);
        return returnFr;
        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unit"></param>
    public void DestroyClusterUnit(Fruit unit)
    {
        
        int fruitIndex = _clusterFruitPool.FindIndex(((c) => c == unit));
        Debug.Log("DEstroy cluster unit: " + fruitIndex);
        //seek left and place at the end the current unit
        for (int i = fruitIndex; i < _clusterFruitPool.Count - 1; ++i)
            _clusterFruitPool[i] = _clusterFruitPool[i + 1];

        _clusterFruitPool[_clusterFruitPool.Count - 1] = unit;
        GameMgr.Instance.FruitMissed(unit.transform.position.x);
        unit.gameObject.SetActive(false);
        --_currentClusterPoolIndex;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unit"></param>
    public void DestroyMultiUnit(Fruit unit)
    {
        
        int fruitIndex = _multiFruitPool.FindIndex(((c) => c == unit));
        //Debug.Log("Destroy multi unit" + fruitIndex);
        //seek left and place at the end the current unit
        for (int i = fruitIndex; i < _multiFruitPool.Count - 1; ++i)
            _multiFruitPool[i] = _multiFruitPool[i + 1];

        _multiFruitPool[_multiFruitPool.Count - 1] = unit;
        GameMgr.Instance.FruitMissed(unit.transform.position.x);
        unit.gameObject.SetActive(false);
        --_currentMultifruitPoolIndex;
        Debug.Log("Index after destroying multi unit::___________________::" + _currentMultifruitPoolIndex);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fr"></param>
    public void DestroyFruit(Fruit fr, bool fruitMissed = false)
    {
        Debug.Log("Destroy " + fr._Ftype);
        int fruitIndex = -1;

        if (fr._Ftype == Fruit.F_TYPE.CLUSTER_UNIT)
            DestroyClusterUnit(fr);
        else if (fr._Ftype == Fruit.F_TYPE.MULTI_UNIT)
            DestroyMultiUnit(fr);
        else
        {
            Debug.Log("Attepmting to destroy: "+fr._Ftype);
            fruitIndex = _fruitList.FindIndex(((c) => c == fr));
            Debug.Log("Fruit index found: " + fruitIndex);
            //seek left and place at the end the current disabled coco
            for (int i = fruitIndex; i < _fruitList.Count - 1; ++i)
                _fruitList[i] = _fruitList[i + 1];

            _fruitList[_fruitList.Count - 1] = fr;
            if (fruitMissed)
                GameMgr.Instance.FruitMissed(fr.transform.position.x);
            fr.gameObject.SetActive(false);
            --_currentPoolIndex;
        }
    }

    /// <summary>
    ///     
    /// </summary>
    public void Reset()
    {
        _state = PT_STATE.IDLE;
        _currentAudioTime = Random.Range(_minTreeAudioTime, _maxTreeAudioTime);
        _treeAudioTimer = 0f;
    }
    #endregion


    #region Private Methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemSpawnList"></param>
    /// <param name="spawnChance"></param>
    /// <param name="listSize"></param>
    /// <returns></returns>
    private List<string> GetPoolIdList(List<Level.ItemSpawn> itemSpawnList, float spawnChance, int listSize)
    {
        List<string> _retList = new List<string>();
        for (int i = 0; i < itemSpawnList.Count; ++i)
        {

            for (int j = 0; j < Mathf.FloorToInt(itemSpawnList[i].SpawnRatio * spawnChance * listSize); ++j)
                _retList.Add(itemSpawnList[i].Id);
        }
        Shuffle(_retList);
        return _retList;
    }

    /// <summary>
    /// 
    /// </summary>
    private void SpawnFruit()
    {
        if (_currentPoolIndex >= _fruitList.Count)
            Debug.LogError("Index out of range!");
        else
        {
            _fruitList[_currentPoolIndex].gameObject.SetActive(true);
            _fruitList[_currentPoolIndex].transform.position = new Vector3(Random.Range(_spawnMinXLimit.position.x, _spawnMaxXLimit.position.x),
                                                                            Random.Range(_spawnMinYLimit.position.y, _spawnMaxYLimit.position.y), transform.position.z);
            _fruitList[_currentPoolIndex].FruitTree = this;
            _fruitList[_currentPoolIndex].StartFruit();
            
        }

        /*if (_fruitList[_currentPoolIndex]._Ftype == Fruit.F_TYPE.CLUSTER_UNIT)
            ++_currentClusterPoolIndex;     
        else if (_fruitList[_currentPoolIndex]._Ftype == Fruit.F_TYPE.MULTI_UNIT)
            ++_currentMultifruitPoolIndex;
        else*/
        ++_currentPoolIndex;
            //_currentPoolIndex = (_currentPoolIndex + 1) % _fruitList.Count;
    }

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
                case EquipmentItem.MOD_TYPE.ITEM_FIND_PROB:
                    _currentEquipmentSpawnChance += _equipmentSpawnChance * eI.ModValueList[i];
                    //TODO: collider size + sprite?
                    //fixed sacks?-> no
                    break;
                case EquipmentItem.MOD_TYPE.GOLD_FIND_PROB:
                    _currentEquipmentSpawnChance += _equipmentSpawnChance * eI.ModValueList[i];
                    break;

                case EquipmentItem.MOD_TYPE.FALL_SPEED:
                    _currentFruitFallTime += _fallFruitTime * eI.ModValueList[i];
                    //_currentSpeed = _baseSpeed * (1f + eI.ModValue);
                    //_currentRawSpeed = _currentSpeed;
                    break;
            }
        }
    }

    /// <summary>
    /// Fisher-Yates shuffle
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    private void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
	#endregion


	#region Properties
    //Shaker Monkey
    public float SapwnRatioModifier { get { return _spawnRatioModifier; } set { _spawnRatioModifier = value; } }
    public float BurstChanceOnSpawn { get { return _burstChanceOnSpawn; } set { _burstChanceOnSpawn = value; } }
    public int BurtsAmount { get { return _burtsAmount; } set { _burtsAmount = value; } }
    public float ItemSpawnChance { get { return _itemSpawnChance; } set { _itemSpawnChance = value; } }
    public EquipmentItem SlotA { get { return _slotA; } set { _slotA = value; } }
    public EquipmentItem SlotB { get { return _slotB; } set { _slotB = value; } }
    public float CurrentFallSpeed { get { return _currentFruitFallTime; } set { _currentFruitFallTime = value; } }
    public float CurrentFruitFlyTime { get { return _currentFruitFlyTime; } set { _currentFruitFlyTime = value; } }
	#endregion

	#region Private Serialized Fields
    [SerializeField]
    private GameObject _fruitPrefab;
    [SerializeField]
    private Transform _fruitRoot;
    [SerializeField]
    private Transform _spawnMinXLimit, _spawnMaxXLimit, _spawnMinYLimit, _spawnMaxYLimit;
    [SerializeField]
    private int _fruitInitCountPool;
    private int _fruitMaxPool;
    private int _fruitIncrement;
	
    //Shaker monkey stats
    [SerializeField]
    private float _spawnRatioModifier;
    [SerializeField]
    private float _burstChanceOnSpawn;      //chance to spawn a fruit burst when spawning a fruit
    [SerializeField]
    private int _burtsAmount;
    [SerializeField]
    private float _itemSpawnChance;     //chance to spawn an item instead a fruit


    [SerializeField]
    private int _maxClusterPoolCount, _maxMultifruitPoolCount;

    [SerializeField]
    private float _minTreeAudioTime, _maxTreeAudioTime;

    //[SerializeField]
    private List<Level.ItemSpawn> _goldItemPoolType, _equipmentItemPoolType;
    #endregion

	#region Private Non-serialized Fields
    private PT_STATE _state, _lastState;
    private float _currentSpawnTime, _baseSpawnTime;
    private List<Fruit> _fruitList;
    private List<Fruit> _clusterFruitPool; //pool used for instantiating cluster clones for cluster fruits on launch
    private List<Fruit> _multiFruitPool;    //pool used for instantiating multifruit clones for multifruit type on launch
    private List<Fruit.G_TYPE> _goldItemsTypePool;  //poll which stores different golditems type; used on fruitsetup if type==goldiItem
    private float _timer;

    private int _currentPoolIndex;
    private int _currentClusterPoolIndex, _currentMultifruitPoolIndex;

    private float _fallFruitTime;   //fall speed set by level stats
private float _fruitFlyTime;
    private float _currentFruitFallTime;   //final speed calculated after applying item mod
private float _currentFruitFlyTime;
    private float _goldSpawnChance, _equipmentSpawnChance;
    private float _currentGoldSpawnChance, _currentEquipmentSpawnChance;

    private EquipmentItem _slotA, _slotB;
    private static System.Random rng = new System.Random();

    private float _currentAudioTime, _treeAudioTimer;

    private List<Level.FruitSpawn> _levelFruitSpawnTypeList, _currentFruitSpawnTypeList;
	#endregion
}
