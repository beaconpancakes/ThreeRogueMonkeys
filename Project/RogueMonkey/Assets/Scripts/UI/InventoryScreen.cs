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
using SunCubeStudio.Localization;

public class InventoryScreen : MonoBehaviour {

	#region Public Data

	#endregion


	#region Behaviour Methods
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //if (Input.GetKeyDown(KeyCode.I))
            //InitScreen();
	}
	#endregion

	#region Public Methods
    /// <summary>
    /// 
    /// </summary>
    public void InitScreen()
    {
        //Items should be already loaded

        //(1) Set inventory item miniatures
        _inventoryItems = DataMgr.Instance.GetInventoryItems();
        //Sort inventory items getting equipped ones first
        _inventoryItems.Sort(delegate(EquipmentItem x, EquipmentItem y)
        {
            if (x.Equipped && y.Equipped)
                return 0;
            else if (x.Equipped)
                return -1;
            else
                return 1;
        });
        for (int i = 0; i < _inventoryItems.Count; ++i)
            Debug.Log(_inventoryItems[i].IdName+" equipped: "+_inventoryItems[i].Equipped);
        //Debug.Log("Firt equipped: " + _inventoryItems[0].Equipped);
        //Debug.Log("Retrieved inventory items: " + _inventoryItems.Count);

        foreach (Image slot in _monkeysSlotist)
        {
            slot.gameObject.SetActive(false);
            slot.GetComponentInChildren<Text>().text = "";

        }
        
        for (int i = 0; i < _inventoryItems.Count; ++i)
        {
            Debug.Log("Attemting to load: " + _inventoryItems[i].IdName);
            //TODO: set sprite field on data mgr on load it directly from shop ._Sprite
            _inventoryItemButtonList[i].image.sprite = _inventoryItems[i]._Sprite;
            //_inventoryItemButtonList[i].image.sprite = Resources.Load(_inventoryItems[i].IdSpriteName, typeof(Sprite)) as Sprite;
            _inventoryItemButtonList[i].gameObject.SetActive(true);

            //Frame feedback
            if (_inventoryItems[i].Equipped)
            {
                //Frame FB

                //Disable New label
                _inventoryItemButtonList[i].transform.GetChild(0).gameObject.SetActive(false);
                //Set item
                SetItem(_inventoryItems[i]);
            }
            else
            {
                //Frame FB
                _inventoryItemButtonList[i].transform.GetChild(0).gameObject.SetActive(_inventoryItems[i].New);
                if (_inventoryItems[i].Count > 1)
                {
                    _inventoryItemButtonList[i].transform.GetChild(1).GetComponent<Text>().text = _inventoryItems[i].Count.ToString();
                    _inventoryItemButtonList[i].transform.GetChild(1).gameObject.SetActive(true);
                }
                else
                    _inventoryItemButtonList[i].transform.GetChild(1).gameObject.SetActive(false);
            }

        }
        for (int i = _inventoryItems.Count; i < _inventoryItemButtonList.Count; ++i)
            _inventoryItemButtonList[i].gameObject.SetActive(false);

        _sellButton.SetActive(false);
        _sellPopUp.SetActive(false);
        _goldImg.SetActive(false);
        _sellValueLabel.SetActive(false);
        _sellValueText.text = "";
        _selectedItemText.text = "";
        _playerGoldText.text = GameMgr.Instance.Gold.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    public void SelectInventoryItemIndex(int index)
    {
        Debug.Log("Selected inventory item index: " + index + "with type: "+_inventoryItems[index].SlotType);
        if (_showingSellPopup)
            return;

        _currentSelectedItem = _inventoryItems[index];
        ShowItem(_currentSelectedItem);
        HighlightMonkeySlot(_currentSelectedItem.SlotType);
        _equipUnequipButton.gameObject.SetActive(true);
        if (_currentSelectedItem.Equipped)
            _equipUnequipButton.GetComponentInChildren<Text>().text = LocalizationService.Instance.GetTextByKey("loc_remove");
        else
            _equipUnequipButton.GetComponentInChildren<Text>().text = LocalizationService.Instance.GetTextByKey("loc_equip");

        _goldImg.SetActive(true);
        _sellValueLabel.SetActive(true);
        _sellValueText.text = _currentSelectedItem.Value.ToString();
        _sellButton.SetActive(true);
    }

    /// <summary>
    /// 
    /// </summary>
    public void EquipUnequipCurrentItem()
    {
        if (_currentSelectedItem.Equipped)
            UnequipCurrentItem();
        else
            EquipCurrentItem();
    }
    

    /// <summary>
    /// 
    /// </summary>
    /// <param name="show"></param>
    public void ShowSellPopUp(bool show)
    {
        _sellPopUp.SetActive(show);
        _showingSellPopup = show;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SellCurrentItem()
    {
        --_currentSelectedItem.Count;
        int index = _inventoryItems.FindIndex((itm) => (itm == _currentSelectedItem));
        //Remove if no remaining items
        if (_currentSelectedItem.Count == 0)
        {
            if (_currentSelectedItem.Equipped)
                UnequipCurrentItem();

            _inventoryItems.Remove(_currentSelectedItem);

            //Update list
            for (int i = index; i < _inventoryItems.Count; ++i)
            {
                _inventoryItemButtonList[i].image.sprite = _inventoryItems[i]._Sprite;
                _inventoryItemButtonList[i].transform.GetChild(0).gameObject.SetActive(_inventoryItems[i].New);
                _inventoryItemButtonList[i].transform.GetChild(1).GetComponent<Text>().text = _inventoryItems[i].Count.ToString();
                _inventoryItemButtonList[i].gameObject.SetActive(true);

            }
            for (int i = _inventoryItems.Count; i < _inventoryItemButtonList.Count; ++i)
                _inventoryItemButtonList[i].gameObject.SetActive(false);    
        }
        else
        {
            
            _inventoryItemButtonList[index].transform.GetChild(1).GetComponent<Text>().text = _currentSelectedItem.Count.ToString();
        }
        
       

        //Add item gold value
        GameMgr.Instance.AddGold(_currentSelectedItem.Value);
        _playerGoldText.text = GameMgr.Instance.Gold.ToString();
        Debug.Log("Now gold is: " + _currentSelectedItem.Value);
        
        _goldImg.SetActive(false);
        
        _sellValueLabel.SetActive(false);
        _sellValueText.text = "";
        ShowSellPopUp(false);

        if (_currentSelectedItem.Count == 0)
        {
            _currentSelectedItem = null;
            _selectedItemImg.gameObject.SetActive(false);
            _selectedItemText.text = "";
            _equipUnequipButton.gameObject.SetActive(false);
            _sellButton.gameObject.SetActive(false);
        }
        
    }

    /// <summary>
    /// 
    /// </summary>
    public void Back()
    {
        gameObject.SetActive(false);
        //Once "New" mssage is shown, reset New value
        foreach (EquipmentItem eI in _inventoryItems)
            eI.New = false;
        DataMgr.Instance.SaveInventoryItems();
    }
    #endregion


    #region Private Methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="eI"></param>
    private void ShowItem(EquipmentItem eI)
    {
        Debug.Log("Show " + eI.IdName);
        _selectedItemImg.sprite = eI._Sprite;
        if (!_selectedItemImg.gameObject.activeSelf)
            _selectedItemImg.gameObject.SetActive(true);
        ShowItemText(eI.ModTypeList, eI.ModValueList);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mT"></param>
    /// <param name="modValue"></param>
    private void ShowItemText(List<EquipmentItem.MOD_TYPE> mtList, List<float> modValueList)
    {
        string displayedText = "";

        _selectedItemText.text = "";
        for (int i = 0; i < mtList.Count; ++i)
        {
            Debug.Log("MOD: " + i+ mtList[i]);
            switch (mtList[i])
            {
                case EquipmentItem.MOD_TYPE.ACURRACY:
                    displayedText = LocalizationService.Instance.GetTextByKey("loc_accuracy") + " + ";
                    break;

                case EquipmentItem.MOD_TYPE.ADDITIONA_MOD:
                    displayedText = "Additional Mod%%%%%";
                    break;

                case EquipmentItem.MOD_TYPE.COLLECTOR_SPEED:
                    displayedText = LocalizationService.Instance.GetTextByKey("loc_speedCol") + " + ";
                    break;

                case EquipmentItem.MOD_TYPE.FALL_SPEED:
                    displayedText = LocalizationService.Instance.GetTextByKey("loc_speedFruitFall") + " + ";
                    break;

                case EquipmentItem.MOD_TYPE.GOLD_FIND_PROB:
                    displayedText = LocalizationService.Instance.GetTextByKey("loc_goldChance") + " + ";
                    break;
                case EquipmentItem.MOD_TYPE.ITEM_FIND_PROB:
                    displayedText = LocalizationService.Instance.GetTextByKey("loc_equipChance") + " + ";
                    break;

                case EquipmentItem.MOD_TYPE.RELOAD_SPEED:
                    displayedText = LocalizationService.Instance.GetTextByKey("loc_reloadSpeed") + " + ";
                    break;

                case EquipmentItem.MOD_TYPE.SACK_SIZE:
                    displayedText = LocalizationService.Instance.GetTextByKey("loc_sackSize") + " + ";
                    break;

                case EquipmentItem.MOD_TYPE.STRIKER_HIT_SIZE:
                    displayedText = LocalizationService.Instance.GetTextByKey("loc_strikerArea") + " + ";
                    break;

                case EquipmentItem.MOD_TYPE.STRIKER_SPEED:
                    displayedText = LocalizationService.Instance.GetTextByKey("loc_strikerSpeed") + " + ";
                    break;
            }
            //Format, percent/integer
            if (mtList[i] != EquipmentItem.MOD_TYPE.SACK_SIZE)
                displayedText += (modValueList[i]*100f).ToString("00") + "%";
            else
                displayedText += modValueList[i].ToString("0");

            _selectedItemText.text += displayedText;

            if (i != mtList.Count-1)
                _selectedItemText.text += '\n';
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void EquipCurrentItem()
    {
        if (_showingSellPopup)
            return;

        EquipmentItem equippedItm = null;
        //Check if there's any equipped item in desired slot
        switch (_currentSelectedItem.SlotType)
        {
            case EquipmentItem.SLOT_TYPE.SHAKER_A:
                equippedItm = GameMgr.Instance.ShakerSlotA;
                break;

            case EquipmentItem.SLOT_TYPE.SHAKER_B:
                equippedItm = GameMgr.Instance.ShakerSlotB;
                break;

            case EquipmentItem.SLOT_TYPE.COLLECTOR_A:
                equippedItm = GameMgr.Instance.CollectorSlotA;
                break;

            case EquipmentItem.SLOT_TYPE.COLLECTOR_B:
                equippedItm = GameMgr.Instance.CollectorSlotB;
                break;

            case EquipmentItem.SLOT_TYPE.STRIKER_A:
                equippedItm = GameMgr.Instance.StrikerSlotA;
                break;

            case EquipmentItem.SLOT_TYPE.STRIKER_B:
                equippedItm = GameMgr.Instance.StrikerSlotB;
                break;
        }
        if (equippedItm != null)
        {
            equippedItm.Equipped = false;
            //TODO: change feedback
            //_inventoryItems.FindIndex(equipped);

        }
        _currentSelectedItem.Equipped = true;
        _equipUnequipButton.GetComponentInChildren<Text>().text = LocalizationService.Instance.GetTextByKey("loc_remove");
        //TODO: change feedback
        SetItem(_currentSelectedItem);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    private void UnequipCurrentItem()
    {

        switch (_currentSelectedItem.SlotType)
        {
            case EquipmentItem.SLOT_TYPE.SHAKER_A:
                GameMgr.Instance.ShakerSlotA = null;
                //GameMgr.Instance._FruitTree.SlotA = null;
                break;

            case EquipmentItem.SLOT_TYPE.SHAKER_B:
                GameMgr.Instance.ShakerSlotB = null;
                //GameMgr.Instance._FruitTree.SlotB = null;
                break;

            case EquipmentItem.SLOT_TYPE.COLLECTOR_A:
                GameMgr.Instance.CollectorSlotA = null;
                //GameMgr.Instance._CollectorMonkey.SlotA = null;
                break;

            case EquipmentItem.SLOT_TYPE.COLLECTOR_B:
                GameMgr.Instance.CollectorSlotB = null;
                break;

            case EquipmentItem.SLOT_TYPE.STRIKER_A:
                GameMgr.Instance.StrikerSlotA = null;
                break;

            case EquipmentItem.SLOT_TYPE.STRIKER_B:
                GameMgr.Instance.StrikerSlotB = null;
                break;
        }

        _currentSelectedItem.Equipped = false;
        _equipUnequipButton.GetComponentInChildren<Text>().text = LocalizationService.Instance.GetTextByKey("loc_equip");
        _monkeysSlotist[(int)_currentSelectedItem.SlotType].gameObject.SetActive(false);
        //TODO: change feedback
        ClearSlotText(_currentSelectedItem.SlotType);

    }

    /// <summary>
    /// Highligh monkey item slot associated with selected item
    /// </summary>
    /// <param name="sT"></param>
    private void HighlightMonkeySlot(EquipmentItem.SLOT_TYPE sT)
    {
        /*int slotIndex = -1;
        switch (sT)
        {
            case EquipmentItem.SLOT_TYPE.SHAKER_A:
                slotIndex = 0;
                break;

            case EquipmentItem.SLOT_TYPE.SHAKER_B:
                slotIndex = 1;
                break;

            case EquipmentItem.SLOT_TYPE.COLLECTOR_A:
                slotIndex = 2;
                break;

            case EquipmentItem.SLOT_TYPE.COLLECTOR_B:
                slotIndex = 3;
                break;

            case EquipmentItem.SLOT_TYPE.STRIKER_A:
                slotIndex = 4;
                break;

            case EquipmentItem.SLOT_TYPE.STRIKER_B:
                slotIndex = 5;
                break;
        }*/
        for (int i = 0; i < _monkeysSlotist.Count; ++i)
            _monkeysSlotist[i].GetComponent<Outline>().enabled = (i == (int)sT);
    }

    /// <summary>
    /// Update equipped item UI and set it to target monkey
    /// </summary>
    /// <param name="eI"></param>
    private void SetItem(EquipmentItem eI)
    {
        int slotIndex = -1;

        switch (eI.SlotType)
        {
            case EquipmentItem.SLOT_TYPE.SHAKER_A:
               //slotIndex = 0;
                GameMgr.Instance.ShakerSlotA = eI;
                //GameMgr.Instance._FruitTree.SlotA = eI;
                
                break;

            case EquipmentItem.SLOT_TYPE.SHAKER_B:
                //slotIndex = 1;
                //GameMgr.Instance._FruitTree.SlotB = eI;
                GameMgr.Instance.ShakerSlotB = eI;
                break;

            case EquipmentItem.SLOT_TYPE.COLLECTOR_A:
                //slotIndex = 2;
                //GameMgr.Instance._CollectorMonkey.SlotA = eI;
                GameMgr.Instance.CollectorSlotA = eI;
                break;

            case EquipmentItem.SLOT_TYPE.COLLECTOR_B:
                //slotIndex = 3;
                //GameMgr.Instance._CollectorMonkey.SlotB = eI;
                GameMgr.Instance.CollectorSlotB = eI;
                break;

            case EquipmentItem.SLOT_TYPE.STRIKER_A:
                //slotIndex = 4;
                //GameMgr.Instance._StrikerMonkey.SlotA = eI;
                GameMgr.Instance.StrikerSlotA = eI;
                break;

            case EquipmentItem.SLOT_TYPE.STRIKER_B:
                //slotIndex = 5;
                GameMgr.Instance.StrikerSlotB = eI;
                break;
        }
        _monkeysSlotist[(int)eI.SlotType].sprite = Resources.Load(eI.IdSpriteName, typeof(Sprite)) as Sprite;
        _monkeysSlotist[(int)eI.SlotType].gameObject.SetActive(true);
        SetSlotText(eI);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mT"></param>
    /// <param name="modVal"></param>
    /// <param name="index"></param>
    private void SetSlotText(EquipmentItem eI)
    {
        string displayedText = "";

        _monkeysSlotist[(int)eI.SlotType].GetComponentInChildren<Text>().text = "";
        for (int i = 0; i < eI.ModTypeList.Count; ++i)
        {
            switch (eI.ModTypeList[i])
            {
                case EquipmentItem.MOD_TYPE.ACURRACY:
                    displayedText = LocalizationService.Instance.GetTextByKey("loc_accuracy") + " + ";
                    break;

                case EquipmentItem.MOD_TYPE.ADDITIONA_MOD:
                    displayedText = "Additional Mod%%%%%";
                    break;

                case EquipmentItem.MOD_TYPE.COLLECTOR_SPEED:
                    displayedText = _selectedItemText.text = LocalizationService.Instance.GetTextByKey("loc_speedCol") + " + ";
                    break;

                case EquipmentItem.MOD_TYPE.FALL_SPEED:
                    displayedText = LocalizationService.Instance.GetTextByKey("loc_speedFruitFall") + " + ";
                    break;

                case EquipmentItem.MOD_TYPE.GOLD_FIND_PROB:
                    displayedText = LocalizationService.Instance.GetTextByKey("loc_goldChance") + " + ";
                    break;
                case EquipmentItem.MOD_TYPE.ITEM_FIND_PROB:
                    displayedText = LocalizationService.Instance.GetTextByKey("loc_equipChance") + " + ";
                    break;

                case EquipmentItem.MOD_TYPE.RELOAD_SPEED:
                    displayedText = LocalizationService.Instance.GetTextByKey("loc_reloadSpeed") + " + ";
                    break;

                case EquipmentItem.MOD_TYPE.SACK_SIZE:
                    displayedText = LocalizationService.Instance.GetTextByKey("loc_sackSize") + " + ";
                    break;

                case EquipmentItem.MOD_TYPE.STRIKER_HIT_SIZE:
                    displayedText = LocalizationService.Instance.GetTextByKey("loc_strikerArea") + " + ";
                    break;

                case EquipmentItem.MOD_TYPE.STRIKER_SPEED:
                    displayedText = LocalizationService.Instance.GetTextByKey("loc_strikerSpeed") + " + ";
                    break;
            }

            if (eI.ModTypeList[i] != EquipmentItem.MOD_TYPE.SACK_SIZE)
                displayedText += (eI.ModValueList[i]*100f).ToString("00") + "%";
            else
                displayedText += eI.ModValueList[i].ToString("0");

            _monkeysSlotist[(int)eI.SlotType].GetComponentInChildren<Text>().text += displayedText;
            if (i != eI.ModTypeList.Count-1)
                _monkeysSlotist[(int)eI.SlotType].GetComponentInChildren<Text>().text +='\n';
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mT"></param>
    private void ClearSlotText(EquipmentItem.SLOT_TYPE sT)
    {
        _monkeysSlotist[(int)sT].GetComponentInChildren<Text>().text = "";
    }

	#endregion


	#region Properties

	#endregion

	#region Private Serialized Fields
    [SerializeField]
    private List<Button> _inventoryItemButtonList;  //buttons with inventory item miniatures

    [SerializeField]
    private Image _selectedItemImg;
    [SerializeField]
    private Text _selectedItemText;
    [SerializeField]
    private List<Image> _monkeysSlotist;   //lis with shaker, collecotr and striker frame list

    [SerializeField]
    private Button _equipUnequipButton;
    [SerializeField]
    private GameObject _sellPopUp;
    [SerializeField]
    private GameObject _sellButton;

    [SerializeField]
    private GameObject _goldImg;
    [SerializeField]
    private GameObject _sellValueLabel;
    [SerializeField]
    private Text _sellValueText;

    [SerializeField]
    private Text _playerGoldText;
	#endregion

	#region Private Non-serialized Fields
    private List<EquipmentItem> _inventoryItems;
    private EquipmentItem _currentSelectedItem;

    private bool _showingSellPopup;
	#endregion
}
