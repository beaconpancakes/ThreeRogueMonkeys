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

public class ShopMenu : MonoBehaviour {

	#region Public Data
    public const int _smallBoxPrice = 35;
    public const int _mediumBoxPrice = 100;
    public const int _largeBoxPrice = 225;
    public const int _extraLargeBoxPrice = 500;
    
    
	#endregion


	#region Behaviour Methods
	// Use this for initialization
	/*void Start () {
		
	}*/
	
	// Update is called once per frame
	//void Update () {
        //if (Input.GetKeyDown(KeyCode.K))
            //InitShop();
	//}
	#endregion

	#region Public Methods
    /// <summary>
    /// 
    /// </summary>
    public void InitShop()
    {
        //(1) Setup Shop item
        GetAvailableShopItems();

        //(2) set spawned box prices
        SetupPrices();

        //(3) Setup itemBought panel
        _itemBoughtImg.gameObject.SetActive(false);
        _itemBoughtText.text = "";

        //(4) Setup gold
        _goldText.text = GameMgr.Instance.Gold.ToString();

        //(4) Buttons and popup
        _okButton.SetActive(false);

    }

    /// <summary>
    /// 
    /// </summary>
    public void Back()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemIndex"></param>
    public void AttemptToBuy(int itemIndex)
    {
        if (_showingBoughtItem)
            return;

        int currentBoxPrice = -1;

        //Get selected box price
        switch (_auxQualityList[itemIndex])
        {
            case DataMgr.BOX_QUALITY.S:
                currentBoxPrice = _smallBoxPrice;
                break;

            case DataMgr.BOX_QUALITY.M:
                currentBoxPrice = _mediumBoxPrice;
                break;

            case DataMgr.BOX_QUALITY.L:
                currentBoxPrice = _largeBoxPrice;
                break;

            case DataMgr.BOX_QUALITY.XL:
                currentBoxPrice = _extraLargeBoxPrice;
                break;
        }
        if (GameMgr.Instance.Gold >= currentBoxPrice)
            BuyItem(itemIndex);
        else
        {
            //TODO: gold feedback
            LeanTween.scale(_goldText.gameObject, Vector3.one*2f, .5f).setEase(LeanTweenType.easeInExpo).setLoopPingPong(1);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void HideBoughtItem()
    {
        _showingBoughtItem = false;
        _okButton.gameObject.SetActive(false);
        _itemBoughtImg.gameObject.SetActive(false);
        _itemBoughtText.text = "";
        //Update gold if it was a gold item
        if (_currentBoughtItem is GoldItem)
        {
            Debug.Log("Current bought item is gold item");
            GameMgr.Instance.AddGold(_currentBoughtItem.Value);
            _goldText.text = GameMgr.Instance.Gold.ToString();
            //TODO: SFX money + tween money counting

        }
    }

   
	#endregion


	#region Private Methods
    /// <summary>
    /// 
    /// </summary>
    private void GetAvailableShopItems()
    {
        bool skip = false;
        int globalLvl = -1;

        //TODO :cler properlu
        List<DataMgr.ItemSizePair> auxList = null;
        List<DataMgr.ItemSizePair> auxList2 = new List<DataMgr.ItemSizePair>();
        List<DataMgr.QualityDistribution> auxDistr = null;
        
        List<EquipmentItem> auxGameItems = null;
        List<ShopItem> auxItemListWithReqs = new List<ShopItem>();

        //TODO: fi magic numbers
        globalLvl = GameMgr.Instance.StageIndex * 9 + GameMgr.Instance.LevelIndex;
        Debug.Log("Global lvl to find in table" + globalLvl);
        if (_availableShopItemList == null)
            _availableShopItemList = new List<ShopItem>();
        else
            _availableShopItemList.Clear();
        if (_auxQualityList == null)
            _auxQualityList = new List<DataMgr.BOX_QUALITY>();
        else
            _auxQualityList.Clear();

        //Get items pool depending on last level completed
        _shopItemList = DataMgr.Instance.GetShopItemList();
        Debug.Log("lists: " + _shopItemList[0].BoxQualityDistr.Count + " / " + _shopItemList[0].ItemSizePairList.Count);

        for (int i = 0; i < _shopItemList.Count && !skip; ++i)
        {
            if (_shopItemList[i].BreakLvl >= globalLvl)
            {
                //entry found
                skip = true;
                Debug.Log("Found entry!");
                auxList = _shopItemList[i].ItemSizePairList;
                auxDistr = _shopItemList[i].BoxQualityDistr;

                //(1) create box size shuffle bag according to given distribution
                foreach (DataMgr.QualityDistribution qD in auxDistr)
                {
                    for (int k = 0; k < Mathf.FloorToInt(qD.Ratio * 100f); ++k)
                        _auxQualityList.Add(qD.Quality);
                    
                }
                Debug.Log("elemtns in shuffle bag: " + _auxQualityList.Count+" which are S: "+_auxQualityList.FindAll((rr) =>(rr == DataMgr.BOX_QUALITY.S)).Count);
                Shuffle(_auxQualityList);
                _auxQualityList = _auxQualityList.GetRange(0, 3); //get three random elements (we have alredy shuffled list)

                foreach (DataMgr.BOX_QUALITY bq in _auxQualityList)
                    Debug.Log("BQ: " + bq);
                //(2) get items baed on 
                auxGameItems = DataMgr.Instance.GetGameItems();


                //TODO: find all items which meets reqs for each size on auzQualityList and get a randome one, iterating over qualityList one by one
                for (int j = 0; j < _auxQualityList.Count; ++j)
                {
                    if (auxItemListWithReqs != null)
                        auxItemListWithReqs.Clear();

                    auxList2.Clear();
                    //add ALL items which meet reqs of p.quality from shoptable entry
                    foreach (DataMgr.ItemSizePair ipp in auxList)
                        Debug.Log("Item in ipp" + ipp.ItemId);
                    auxList2.AddRange(auxList.FindAll((kp) => (kp.MinQuality <= _auxQualityList[j])));
                    Debug.Log("for quality "+_auxQualityList[j].ToString()+" we have "+auxList2.Count+" elements");
                    for (int jj = 0; jj < auxList2.Count; ++jj)
                        Debug.Log("----------->" + auxList2[jj].ItemId);
                    Shuffle(auxList2);
                   // auxItemListWithReqs.AddRange(auxGameItems.FindAll((itm) => (itm.IdName == auxList[j].ItemId) && (_auxQualityList[j]>=auxList[j].MinQuality)));
                    _availableShopItemList.Add(auxGameItems.Find((itm) => (itm.IdName == auxList2[0].ItemId)));
                   // auxItemListWithReqs.Add(
                   // _availableShopItemList.Add(auxGameItems.Find((itm) => (itm.IdName == p.ItemId) && (_auxQualityList.Find((d) => (d >= p.MinQuality)) != null)));
                }
            }
        }

        for (int i = 0; i < _availableShopItemList.Count; ++i)
            Debug.Log("Avaliable: " + _availableShopItemList[i].IdName);


    }

    /// <summary>
    /// 
    /// </summary>
    private void SetupPrices()
    {
        for (int i = 0; i < _auxQualityList.Count; ++i)
        {
            switch (_auxQualityList[i])
            {
                case DataMgr.BOX_QUALITY.S:
                    
                    _itemSpawnPtList[i].GetComponentInChildren<Text>().text = _smallBoxPrice.ToString();
                    break;

                case DataMgr.BOX_QUALITY.M:
                    _itemSpawnPtList[i].GetComponentInChildren<Text>().text = _mediumBoxPrice.ToString();
                    break;

                case DataMgr.BOX_QUALITY.L:
                    _itemSpawnPtList[i].GetComponentInChildren<Text>().text = _largeBoxPrice.ToString();
                    break;

                case DataMgr.BOX_QUALITY.XL:
                    _itemSpawnPtList[i].GetComponentInChildren<Text>().text = _extraLargeBoxPrice.ToString();
                    break;
            }
            _itemSpawnPtList[i].GetComponentInChildren<Button>().image.sprite = _boxSizeSpriteList[(int)_auxQualityList[i]];
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    private void BuyItem(int index)
    {
        //TODO: opening animation

        //This box ins no longer available
        _itemSpawnPtList[index].GetComponentInChildren<Button>().interactable = false;
        _currentBoughtItem = _availableShopItemList[index];
        //Remove gold
        switch (_auxQualityList[index])
        {
            case DataMgr.BOX_QUALITY.S:
                GameMgr.Instance.AddGold(- _smallBoxPrice);
                break;

            case DataMgr.BOX_QUALITY.M:
                GameMgr.Instance.AddGold(-_mediumBoxPrice);
                break;

            case DataMgr.BOX_QUALITY.L:
                GameMgr.Instance.AddGold(-_largeBoxPrice);
                break;

            case DataMgr.BOX_QUALITY.XL:
                GameMgr.Instance.AddGold(-_extraLargeBoxPrice);
                break;
        }
        
        _goldText.text = GameMgr.Instance.Gold.ToString("0");
        //TODO SFX money spent


        //Reveal box item on image frame
        Debug.Log("Showing sprite "+_availableShopItemList[index].IdName);
        _itemBoughtImg.sprite = Resources.Load(_availableShopItemList[index].IdName, typeof(Sprite)) as Sprite;
        _itemBoughtImg.gameObject.SetActive(true);
        SetItemText(_availableShopItemList[index]);
        _okButton.gameObject.SetActive(true);
        _showingBoughtItem = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ei"></param>
    private void SetItemText(ShopItem sItem)
    {
        if (sItem is GoldItem)
        {
            _itemBoughtText.text = "Gold + " + sItem.Value;
        }
        else
        {
            for (int i = 0; i < ((EquipmentItem)sItem).ModTypeList.Count; ++i)
            {
                switch (((EquipmentItem)sItem).ModTypeList[i])
                {
                    case EquipmentItem.MOD_TYPE.ACURRACY:
                        _itemBoughtText.text = "Acurracy + ";
                        break;

                    case EquipmentItem.MOD_TYPE.ADDITIONA_MOD:
                        _itemBoughtText.text = "Additional Mod%%%%%";
                        break;

                    case EquipmentItem.MOD_TYPE.COLLECTOR_SPEED:
                        _itemBoughtText.text = "Collector speed + ";
                        break;

                    case EquipmentItem.MOD_TYPE.FALL_SPEED:
                        _itemBoughtText.text = "Fruit fall speed - ";
                        break;

                    case EquipmentItem.MOD_TYPE.ITEM_FIND_PROB:
                        _itemBoughtText.text = "Gold item spawn chance + ";
                        break;

                    case EquipmentItem.MOD_TYPE.RELOAD_SPEED:
                        _itemBoughtText.text = "Collector sack reload speed + ";
                        break;

                    case EquipmentItem.MOD_TYPE.SACK_SIZE:
                        _itemBoughtText.text = "Sack size = ";
                        break;

                    case EquipmentItem.MOD_TYPE.STRIKER_HIT_SIZE:
                        _itemBoughtText.text = "Striker hit area + ";
                        break;

                    case EquipmentItem.MOD_TYPE.STRIKER_SPEED:
                        _itemBoughtText.text = "Striker speed + ";
                        break;
                }
                _itemBoughtText.text += ((EquipmentItem)sItem).ModValueList[i].ToString("0.00");
                if (i != ((EquipmentItem)sItem).ModTypeList.Count - 1)
                    _itemBoughtText.text += '\n';
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

	#endregion

	#region Private Serialized Fields
    [SerializeField]
    private List<GameObject> _itemSpawnPtList;
    [SerializeField]
    private List<Sprite> _boxSizeSpriteList;
    [SerializeField]
    private Image _itemBoughtImg;
    [SerializeField]
    private GameObject _okButton;
    
    [SerializeField]
    private Text _goldText;
    [SerializeField]
    private Text _itemBoughtText;
	#endregion

	#region Private Non-serialized Fields
    private List<DataMgr.ShopItemTableEntry> _shopItemList;
    private List<ShopItem> _availableShopItemList;
    private List<ShopItem> _currentVendorItems;
    private List<DataMgr.BOX_QUALITY> _auxQualityList;
    private static System.Random rng = new System.Random();

    private bool _showingBoughtItem;        //flag used to prevent buying any other item until accept current one shown
    private ShopItem _currentBoughtItem;
	#endregion
}
