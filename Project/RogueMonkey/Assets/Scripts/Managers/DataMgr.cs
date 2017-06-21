﻿/************************************************************************/
/* @Author: Author Name
 * @Date: Date
 * @Brief: BBBrief
 * @Description: DDDescription
 * ***********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;
using SunCubeStudio.Localization;

public class DataMgr : MonoBehaviour {


	#region Public Data
    public enum SHOP_ITEM_TYPE { EMPTY = 0, GOLD_ITEM, EQUIPMENT }
    public enum BOX_QUALITY { S = 0, M, L, XL }

    [System.Serializable]
    public class QualityDistribution
    {
        public QualityDistribution(BOX_QUALITY bQ, float r)
        {
            Quality = bQ;
            Ratio = r;
        }
        public BOX_QUALITY Quality;
        public float Ratio; // c [0,1]
    }

    [System.Serializable]
    public class ItemSizePair
    {
        public ItemSizePair(string id, BOX_QUALITY bq)
        {
            ItemId = id;
            MinQuality = bq;
        }
        public string ItemId;
        public BOX_QUALITY MinQuality;
    }

    [System.Serializable]
    public class ShopItemTableEntry
    {
        //Ctor
        public ShopItemTableEntry(int bLvl, List<QualityDistribution> boxDistr, List<ItemSizePair> itemP)
        {
            BreakLvl = bLvl;
            BoxQualityDistr = boxDistr;
            ItemSizePairList = itemP;
        }

        public int BreakLvl;        //determines max lvl restriction for this entry
        public List<QualityDistribution> BoxQualityDistr;
        public List<ItemSizePair> ItemSizePairList;
    }


    public static DataMgr Instance;
	#endregion


	#region Behaviour Methods
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        LoadData();
    }
    /*void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            
        }
    }*/
	#endregion

	#region Public Methods

    /// <summary>
    /// 
    /// </summary>
    public void LoadItemData()
    {
        string slotTypeS, modTypeS;
        string auxId, auxSpriteId;
        int auxValue = -1, auxCount = -1;
        GoldItem auxGoldItm = null;
        EquipmentItem auxEquipItm = null;
        List<EquipmentItem.MOD_TYPE> auxMtList = new List<EquipmentItem.MOD_TYPE>();
        List<float> auxModValList = new List<float>();
        EquipmentItem.SLOT_TYPE auxSt = EquipmentItem.SLOT_TYPE.COLLECTOR_A;

        XmlReader reader = XmlReader.Create(new StringReader(_itemDataTA.text));
        //XmlDocument doc = new XmlDocument();
       // doc.LoadXml(_textDoc.text);

        if (_gameItemList == null)
            _gameItemList = new List<ShopItem>();
        else
            _gameItemList.Clear();

        _readingGoldItem = false;
        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    if (reader.Name.CompareTo("gold_item_type") == 0)
                        _readingGoldItem = true;
                    else if (reader.Name.CompareTo("equipment_item_data") == 0)
                        _readingGoldItem = false;
                    else
                    {
                        //Debug.Log("Value: " + reader.Name);
                        if (reader.Name.CompareTo("item") == 0)
                        {
                            if (_readingGoldItem)
                            {
                                auxSpriteId = reader.GetAttribute("spriteId");
                                auxGoldItm = new GoldItem(reader.GetAttribute("id"), auxSpriteId, XmlConvert.ToInt32(reader.GetAttribute("value")));
                                Debug.Log("Loading sprite id- - - -" + auxSpriteId);
                                auxGoldItm._Sprite = Resources.Load(auxSpriteId, typeof(Sprite)) as Sprite;
                                _gameItemList.Add(auxGoldItm);
                            }
                            else
                            {
                                auxId = reader.GetAttribute("id");
                                auxSpriteId = reader.GetAttribute("spriteId");
                                auxValue = XmlConvert.ToInt32(reader.GetAttribute("value"));
                                //auxCount = XmlConvert.ToInt32(reader.GetAttribute("count"));

                                slotTypeS = reader.GetAttribute("slotType");
                                if (slotTypeS.CompareTo("COLLECTOR_A") == 0)
                                    auxSt = EquipmentItem.SLOT_TYPE.COLLECTOR_A;
                                else if (slotTypeS.CompareTo("COLLECTOR_B") == 0)
                                    auxSt = EquipmentItem.SLOT_TYPE.COLLECTOR_B;
                                else if (slotTypeS.CompareTo("STRIKER_A") == 0)
                                    auxSt = EquipmentItem.SLOT_TYPE.STRIKER_A;
                                else if (slotTypeS.CompareTo("STRIKER_B") == 0)
                                    auxSt = EquipmentItem.SLOT_TYPE.STRIKER_B;
                                else if (slotTypeS.CompareTo("SHAKER_A") == 0)
                                    auxSt = EquipmentItem.SLOT_TYPE.SHAKER_A;
                                else if (slotTypeS.CompareTo("SHAKER_B") == 0)
                                    auxSt = EquipmentItem.SLOT_TYPE.SHAKER_B;
                                else
                                    Debug.LogError("Error parsing slotType " + auxSt);

                                //Read mod list
                                auxMtList = new List<EquipmentItem.MOD_TYPE>();
                                auxModValList = new List<float>();
                                while (reader.Read())
                                {
                                    if (reader.IsStartElement() && reader.Name.CompareTo("mod") == 0)
                                    {
                                        modTypeS = reader.GetAttribute("modType");
                                        //Debug.Log("R E A D I N G: " + modTypeS);
                                        if (modTypeS.CompareTo("RELOAD_SPEED") == 0)
                                            auxMtList.Add(EquipmentItem.MOD_TYPE.RELOAD_SPEED);
                                        else if (modTypeS.CompareTo("SACK_SIZE") == 0)
                                            auxMtList.Add(EquipmentItem.MOD_TYPE.SACK_SIZE);
                                        else if (modTypeS.CompareTo("COLLECTOR_SPEED") == 0)
                                            auxMtList.Add(EquipmentItem.MOD_TYPE.COLLECTOR_SPEED);
                                        else if (modTypeS.CompareTo("STRIKER_HIT_SIZE") == 0)
                                            auxMtList.Add(EquipmentItem.MOD_TYPE.STRIKER_HIT_SIZE);
                                        else if (modTypeS.CompareTo("STRIKER_SPEED") == 0)
                                            auxMtList.Add(EquipmentItem.MOD_TYPE.STRIKER_SPEED);
                                        else if (modTypeS.CompareTo("ACCURACY") == 0)
                                            auxMtList.Add(EquipmentItem.MOD_TYPE.ACURRACY);
                                        else if (modTypeS.CompareTo("FALL_SPEED") == 0)
                                            auxMtList.Add(EquipmentItem.MOD_TYPE.FALL_SPEED);
                                        else if (modTypeS.CompareTo("GOLD_FIND_PROB") == 0)
                                            auxMtList.Add(EquipmentItem.MOD_TYPE.GOLD_FIND_PROB);
                                        else if (modTypeS.CompareTo("ITEM_FIND_PROB") == 0)
                                            auxMtList.Add(EquipmentItem.MOD_TYPE.ITEM_FIND_PROB);
                                        else
                                            Debug.LogError("Error parsing slotType " + modTypeS);

                                        auxModValList.Add(float.Parse(reader.GetAttribute("modVal")));

                                    }
                                    else if (!reader.IsStartElement() && reader.Name.CompareTo("item") == 0)
                                    {
                                        //Debug.Log("Item read ended, break reading mod List");
                                        break;
                                    }
                                }


                                auxEquipItm = new EquipmentItem(auxId, auxSpriteId, auxValue, auxSt, auxMtList, auxModValList/*,auxCount*/);
                                //auxSt, auxMt, float.Parse(reader.GetAttribute("modVal")), 0);
                                //Load Sprite reference
                                auxEquipItm._Sprite = Resources.Load(auxSpriteId, typeof(Sprite)) as Sprite;
                                _gameItemList.Add(auxEquipItm);
                            }
                        }
                    } 
                    break;

                case XmlNodeType.EndElement:
                    Debug.Log("End element Value: " + reader.Name);
                    break;
            }
        }
        Debug.Log("Current itemlist count:" + _gameItemList.Count);
    }

    /// <summary>
    /// 
    /// </summary>
    public void LoadShopData()
    {
        XmlReader reader = XmlReader.Create(new StringReader(_shopDataTA.text));
        int tempLvl = -1;
        string qualityS;
        BOX_QUALITY bQuality = BOX_QUALITY.S;

        //list init
        if (_tempDistr == null)
            _tempDistr = new List<QualityDistribution>();
        else
            _tempDistr.Clear();
        
        if (_tempSizePait == null)
            _tempSizePait = new List<ItemSizePair>();
        else
            _tempSizePait.Clear();

        if (_shopItemList == null)
            _shopItemList = new List<ShopItemTableEntry>();
        else
            _shopItemList.Clear();


        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    if (reader.Name.CompareTo("data_entry") == 0)
                    {
                        tempLvl = XmlConvert.ToInt32(reader.GetAttribute("breakLvl"));
                    }
                    else if (reader.Name.CompareTo("box_prob_list") == 0)
                    {
                        while (reader.Read() && reader.IsStartElement()/*NodeType != XmlNodeType.EndElement && reader.NodeType != XmlNodeType.Whitespace*/)
                        {
                            Debug.Log("nodeType " + reader.NodeType.ToString());
                            qualityS = reader.GetAttribute("type");
                            if (qualityS.CompareTo("S") == 0)
                                bQuality = BOX_QUALITY.S;
                            else if (qualityS.CompareTo("M") == 0)
                                bQuality = BOX_QUALITY.M;
                            else if (qualityS.CompareTo("L") == 0)
                                bQuality = BOX_QUALITY.L;
                            else if (qualityS.CompareTo("XL") == 0)
                                bQuality = BOX_QUALITY.XL;
                            else
                                Debug.LogError("Error reading quality value");

                            _tempDistr.Add(new QualityDistribution(bQuality, float.Parse(reader.GetAttribute("prob"))));
                        }
                    }
                    else if (reader.Name.CompareTo("item_pool") == 0)
                    {
                        while (reader.Read() && reader.IsStartElement()/*reader.NodeType != XmlNodeType.EndElement && reader.NodeType != XmlNodeType.Whitespace*/)
                        {
                            Debug.Log(".  .  .");
                            qualityS = reader.GetAttribute("minSize");
                            if (qualityS.CompareTo("S") == 0)
                                bQuality = BOX_QUALITY.S;
                            else if (qualityS.CompareTo("M") == 0)
                                bQuality = BOX_QUALITY.M;
                            else if (qualityS.CompareTo("L") == 0)
                                bQuality = BOX_QUALITY.L;
                            else if (qualityS.CompareTo("XL") == 0)
                                bQuality = BOX_QUALITY.XL;
                            else
                                Debug.LogError("Error reading quality value");
                            _tempSizePait.Add(new ItemSizePair(reader.GetAttribute("id"), bQuality));
                        }
                    }
                    break;

                case XmlNodeType.EndElement:
                    if (reader.Name.CompareTo("data_entry") == 0)
                    {
                        Debug.Log("Adding lists..... " + _tempDistr.Count + " / " + _tempSizePait.Count);
                        //Add Entry tp Table and clear temp vars
                        _shopItemList.Add(new ShopItemTableEntry(tempLvl, _tempDistr, _tempSizePait));

                    }
                    else
                        Debug.Log("End element:    " + reader.Name);
                    
                    break;
            }
        }
        Debug.Log("Shop items loaded: " + _shopItemList.Count);
    }

    /*
    public void CreateInventoryItemsFile()
    {
        XmlWriter writer = XmlWriter.Create(Application.persistentDataPath + _dataRelativePath);

        writer.WriteStartDocument();
        //writer.WriteStartElement("item_data");
        //writer.WriteEndElement();
        writer.Close();

    }*/

    /// <summary>
    /// 
    /// </summary>
    public void LoadInventoryItems()
    {
        XmlReader reader = null;
        Debug.Log("Path is: " + Application.persistentDataPath + _invFileName);
        if (File.Exists(Application.persistentDataPath + _invFileName))
        {
            reader = XmlReader.Create(Application.persistentDataPath + _invFileName);// XmlReader.Create(new StringReader(_inventoryDataTA.text));
        }
        else
        {
            /*private void createSaveData()
            {
            TextAsset textAsset = Resources.Load("Saved_Data/SavedData") as TextAsset;
            XmlDocument xmldoc = new XmlDocument ();
            xmldoc.LoadXml(textAsset.text);
            xmldoc.Save(Application.persistentDataPath + "\SavedData.xml");
            }*/
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(_inventoryDataTA.text);
            xmldoc.Save(Application.persistentDataPath + _invFileName);
            reader = XmlReader.Create(Application.persistentDataPath + _invFileName);
            /*
            XmlWriter writer = XmlWriter.Create(Application.persistentDataPath + _invFileName);

            writer.WriteStartDocument();
            writer.WriteStartElement("item_data");
            writer.WriteEndElement();
            writer.Close();
            reader = XmlReader.Create(Application.persistentDataPath + _invFileName);
             * */
        }
  
        //if no items saved, create empty file
        /*if (reader == null)
        {
            XmlWriter writer = XmlWriter.Create(Application.persistentDataPath + _dataRelativePath);

            writer.WriteStartDocument();
            writer.Close();
        }*/
        EquipmentItem aux;
        if (_inventoryItems == null)
            _inventoryItems = new List<EquipmentItem>();
        else
            _inventoryItems.Clear();

        while (reader != null && reader.Read() && reader.IsStartElement())
        {
            //switch (reader.NodeType)
            //{
            //  case XmlNodeType.Element:
            if (reader.Name.CompareTo("item") == 0)
            {
                aux = _gameItemList.Find((itm) => (itm.IdName == reader.GetAttribute("id"))) as EquipmentItem;
                //Debug.Log("inventory item found on data" + aux.IdName);
                aux.Equipped = reader.GetAttribute("equipped").CompareTo("true") == 0;
                aux.New = reader.GetAttribute("new").CompareTo("true") == 0;
                aux.Count = XmlConvert.ToInt32(reader.GetAttribute("count"));
                _inventoryItems.Add(aux);
            }
            //      break;
            //}
        }
        Debug.Log("Loaded " + _inventoryItems.Count + " inventory items");
        
    }

    /// <summary>
    /// 
    /// </summary>
    public void SaveInventoryItems()
    {
        Debug.Log("_________Saving inveotyr items");
        XmlWriter writer = XmlWriter.Create(Application.persistentDataPath + _invFileName);

        writer.WriteStartDocument();
        writer.WriteStartElement("item_data");
        for (int i=0; i<_inventoryItems.Count; ++i)
        {
            writer.WriteStartElement("item");
            writer.WriteAttributeString("id",_inventoryItems[i].IdName);
            writer.WriteAttributeString("equipped", _inventoryItems[i].Equipped.ToString());
            writer.WriteAttributeString("new", _inventoryItems[i].New.ToString());
            writer.WriteAttributeString("count", _inventoryItems[i].Count.ToString());
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
        writer.WriteEndDocument();
        writer.Close();
    }


    /// <summary>
    /// 
    /// </summary>
    public void LoadLevelData()
    {
        Debug.Log("_________________Load level data");
        XmlReader reader = null;
        if (File.Exists(Application.persistentDataPath  + _levelFileName))
        {
            reader = XmlReader.Create(Application.persistentDataPath + _levelFileName);// XmlReader.Create(new StringReader(_inventoryDataTA.text));
        }
        else
        {
            Debug.Log("Creating new coz no file exists");
            /////
            XmlDocument xmldoc = new XmlDocument ();
            xmldoc.LoadXml(_levelDataTA.text);
            xmldoc.Save(Application.persistentDataPath + _levelFileName);
            reader = XmlReader.Create(Application.persistentDataPath + _levelFileName);
            ////
            /*XmlWriter writer = XmlWriter.Create(Application.persistentDataPath + _levelFileName);

            writer.WriteStartDocument();
            writer.WriteStartElement("level_data");
            writer.WriteEndElement();
            writer.Close();
            reader = XmlReader.Create(Application.persistentDataPath + _levelFileName);*/
        }
        int stageIndex = -1;
        string auxSt = "";
        //if no items saved, create empty file
        /*if (reader == null)
        {
            XmlWriter writer = XmlWriter.Create(Application.persistentDataPath + _dataRelativePath);

            writer.WriteStartDocument();
            writer.Close();
        }*/


        while (reader != null && reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    if (reader.Name.CompareTo("stage") == 0)
                    {
                        stageIndex =  XmlConvert.ToInt32(reader.GetAttribute("id"));
                        auxSt = reader.GetAttribute("state");
                        if (auxSt.CompareTo("completed") == 0)
                            GameMgr.Instance._StageList[stageIndex].State = Stage.STAGE_STATE.COMPLETED;
                        else if (auxSt.CompareTo("unlocked") == 0)
                            GameMgr.Instance._StageList[stageIndex].State = Stage.STAGE_STATE.UNLOCKED;
                        else if (auxSt.CompareTo("locked") == 0)
                            GameMgr.Instance._StageList[stageIndex].State = Stage.STAGE_STATE.LOCKED;
                        else if (auxSt.CompareTo("unavailable") == 0)
                            GameMgr.Instance._StageList[stageIndex].State = Stage.STAGE_STATE.UNAVAILABLE;
                        else
                            Debug.LogError("wrong level state data" + reader.Name);
                        
                        while (reader != null && reader.Read())
                        {
                            if (reader.IsStartElement() && reader.Name.CompareTo("level") == 0)
                            {
                                //Debug.Log("Loading lvl " + stageIndex + " / " + reader.GetAttribute("id"));
                                //Score
                                GameMgr.Instance._StageList[stageIndex].GetLevelList()[XmlConvert.ToInt32((reader.GetAttribute("id")))].MaxScore = XmlConvert.ToInt32((reader.GetAttribute("score")));
                                //State
                                auxSt = reader.GetAttribute("state");
                                if (auxSt.CompareTo("failed") == 0)
                                    GameMgr.Instance._StageList[stageIndex].GetLevelList()[XmlConvert.ToInt32((reader.GetAttribute("id")))].AvailabilitySt = Level.AVAILABILITY_STATE.FAILED;
                                else if (auxSt.CompareTo("completed") == 0)
                                    GameMgr.Instance._StageList[stageIndex].GetLevelList()[XmlConvert.ToInt32((reader.GetAttribute("id")))].AvailabilitySt = Level.AVAILABILITY_STATE.COMPLETED;
                                else if (auxSt.CompareTo("unlocked") == 0)
                                    GameMgr.Instance._StageList[stageIndex].GetLevelList()[XmlConvert.ToInt32((reader.GetAttribute("id")))].AvailabilitySt = Level.AVAILABILITY_STATE.UNLOCKED;
                                else if (auxSt.CompareTo("locked") == 0)
                                    GameMgr.Instance._StageList[stageIndex].GetLevelList()[XmlConvert.ToInt32((reader.GetAttribute("id")))].AvailabilitySt = Level.AVAILABILITY_STATE.LOCKED;
                                else if (auxSt.CompareTo("unavailable") == 0)
                                    GameMgr.Instance._StageList[stageIndex].GetLevelList()[XmlConvert.ToInt32((reader.GetAttribute("id")))].AvailabilitySt = Level.AVAILABILITY_STATE.UNAVAILABLE;
                                else
                                    Debug.LogError("wrong level state data" + reader.Name);

                                //Rank
                                auxSt = reader.GetAttribute("rank");
                                if (auxSt.CompareTo("S") == 0)
                                    GameMgr.Instance._StageList[stageIndex].GetLevelList()[XmlConvert.ToInt32((reader.GetAttribute("id")))].Rank = Level.RANK.S;
                                else if (auxSt.CompareTo("A") == 0)
                                    GameMgr.Instance._StageList[stageIndex].GetLevelList()[XmlConvert.ToInt32((reader.GetAttribute("id")))].Rank = Level.RANK.A;
                                else if (auxSt.CompareTo("B") == 0)
                                    GameMgr.Instance._StageList[stageIndex].GetLevelList()[XmlConvert.ToInt32((reader.GetAttribute("id")))].Rank = Level.RANK.B;
                                else if (auxSt.CompareTo("C") == 0)
                                    GameMgr.Instance._StageList[stageIndex].GetLevelList()[XmlConvert.ToInt32((reader.GetAttribute("id")))].Rank = Level.RANK.C;
                                else if (auxSt.CompareTo("D") == 0)
                                    GameMgr.Instance._StageList[stageIndex].GetLevelList()[XmlConvert.ToInt32((reader.GetAttribute("id")))].Rank = Level.RANK.D;
                                else if (auxSt.CompareTo("E") == 0)
                                    GameMgr.Instance._StageList[stageIndex].GetLevelList()[XmlConvert.ToInt32((reader.GetAttribute("id")))].Rank = Level.RANK.E;
                                else if (auxSt.CompareTo("F") == 0)
                                    GameMgr.Instance._StageList[stageIndex].GetLevelList()[XmlConvert.ToInt32((reader.GetAttribute("id")))].Rank = Level.RANK.F;
                                else
                                    Debug.LogError("wrong level rank data" + reader.Name);
                            }
                            else if (reader.NodeType == XmlNodeType.EndElement && reader.Name.CompareTo("stage")==0)
                                break;
                        }
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /*public void SaveLevelData()
    {
        XmlWriter writer = XmlWriter.Create(Application.persistentDataPath + _levelFileName);
        Level.AVAILABILITY_STATE auxSt;

        writer.WriteStartDocument();
        writer.WriteStartElement("level_data");
        for (int i = 0; i < GameMgr.Instance._StageList.Count; ++i)
        {
            writer.WriteStartElement("stage");
            writer.WriteAttributeString("id", i.ToString());
            for (int j = 0; i < GameMgr.Instance._StageList[i].GetLevelList().Count; ++j)
            {
                writer.WriteStartElement("level");
                writer.WriteAttributeString("id", j.ToString());
                writer.WriteAttributeString("score", GameMgr.Instance._StageList[i].GetLevelList()[j].GetMaxScore().ToString());
                auxSt = GameMgr.Instance._StageList[i].GetLevelList()[j].GetState();
                switch (auxSt)
                {
                    case Level.AVAILABILITY_STATE.COMPLETED:
                        writer.WriteAttributeString("state", "completed");
                        break;

                    case Level.AVAILABILITY_STATE.UNLOCKED:
                        writer.WriteAttributeString("state", "unlocked");
                        break;

                    case Level.AVAILABILITY_STATE.LOCKED:
                        writer.WriteAttributeString("state", "locked");
                        break;

                    case Level.AVAILABILITY_STATE.UNAVAILABLE:
                        writer.WriteAttributeString("state", "unavailable");
                        break;
                }
                writer.WriteEndElement(); //</level>
            }
        }
        writer.WriteEndElement(); 
    }*/

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stageIndex"></param>
    /// <param name="lvlIndex"></param>
    public void SaveLevelData(int stageIndex, int lvlIndex)
    {
        Debug.Log("Save level data________" +stageIndex+" / "+lvlIndex);
        XmlDocument doc = new XmlDocument();
        doc.Load(Application.persistentDataPath + _levelFileName);

        XmlNode root = doc.DocumentElement;
        Debug.Log("R00t: " + root.Name);
        //XmlElement levelDataNode = (XmlElement)root.SelectSingleNode("level_data");
        //Debug.Log("Leveldatanode: " + levelDataNode);
        //Current stage
        XmlNodeList stages = ((XmlElement)root).GetElementsByTagName("stage");
        Debug.Log("stages: " + stages);

        //go to stage-level aand rewrite attribute data
        foreach (XmlAttribute attr in stages[stageIndex].ChildNodes[lvlIndex].Attributes)
        {
            Debug.Log("Score &&& State attr "+attr.Name);
            //Score
            if (attr.Name.Equals("score"))
            {
                Debug.Log("push score");
                attr.Value = GameMgr.Instance._StageList[stageIndex].GetLevelList()[lvlIndex].MaxScore.ToString();
            }
            //State
            else if (attr.Name.Equals("state"))
            {
                if (GameMgr.Instance._StageList[stageIndex].GetLevelList()[lvlIndex].GetAvState() == Level.AVAILABILITY_STATE.FAILED)
                    attr.Value = "failed";
                else
                    attr.Value = "completed";

            }
            //Rank
            else if (attr.Name.Equals("rank"))
            {
                switch (GameMgr.Instance._StageList[stageIndex].GetLevelList()[lvlIndex].Rank)
                {
                    case Level.RANK.S:
                        attr.Value = "S";
                        break;
                    case Level.RANK.A:
                        attr.Value = "A";
                        break;
                    case Level.RANK.B:
                        attr.Value = "B";
                        break;
                    case Level.RANK.C:
                        attr.Value = "C";
                        break;
                    case Level.RANK.D:
                        attr.Value = "D";
                        break;
                    case Level.RANK.E:
                        attr.Value = "E";
                        break;
                    case Level.RANK.F:
                        attr.Value = "F";
                        break;
                }
            }
        }
        //unlock next level
        if (lvlIndex < GameMgr.Instance._StageList[stageIndex].GetLevelList().Count - 1)
        {
            //go to stage-level adn rewrite attribute data
            foreach (XmlAttribute attr in stages[stageIndex].ChildNodes[lvlIndex + 1].Attributes)
            {
                if (attr.Name.Equals("state"))
                    attr.Value = "unlocked";
            }
        }
        //stage completed
        else if (stageIndex < stages.Count-1)
        {
            //unlock next stage
            foreach (XmlAttribute attr in stages[stageIndex+1].Attributes)
            {
                if (attr.Name.Equals("state"))
                        attr.Value = "unlocked";
            }
            //switch from unavaiable to unlocked
            if (stageIndex < stages.Count - 2)
            {
                foreach (XmlAttribute attr in stages[stageIndex + 2].Attributes)
                {
                    if (attr.Name.Equals("state"))
                        attr.Value = "locked";
                }
            }
            //unlock levels
            for (int i=0; i< stages[stageIndex+1].ChildNodes.Count; ++i)
            {
                foreach (XmlAttribute attr in stages[stageIndex+1].ChildNodes[i].Attributes)
                {
                    if (attr.Name.Equals("state"))
                    {
                        if (i==0)
                            attr.Value = "unlocked";
                        else
                            attr.Value = "locked";
                    }
                }
            }

        }
        doc.Save(Application.persistentDataPath + _levelFileName);


        /*foreach (XmlElement stage in levelDataNode)
        {
            if  (XmlConvert.ToInt32(stage.GetAttribute("id")) == stageIndex)// == GameMgr.Instance.CurrentPlayer.Id)
            {
                for (int i= 0; i< stage.ChildNodes.Count;++i)
                //foreach (XmlNode level in stage)
                {
                    if  (XmlConvert.ToInt32(stage.geGetAttribute("id")) == lvlIndex)// == GameMgr.Instance.CurrentPlayer.Id)
                    {
                        foreach (XmlAttribute attr in level.Attributes)
                        {
                            if (attr.Name.Equals("DifficultyReached"))
                            {
         
                                
                               
                                

                                break;
                            }
                        }
                        break;
                    }
                }
                break;
            
        }*/
    }

    public void SaveGold()
    {
        Debug.Log("Saving gold__________");
        PlayerPrefs.SetInt("Gold", DataMgr.Instance.Gold);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SaveData()
    {
        PlayerPrefs.SetInt("Gold", DataMgr.Instance.Gold);
        PlayerPrefs.SetString("Lang", LocalizationService.Instance.Localization);
        PlayerPrefs.SetInt("Vib", DataMgr.Instance.Vibration);
        SaveInventoryItems();
        SaveLevelData(GameMgr.Instance.StageIndex, GameMgr.Instance.LevelIndex);
        //TODO: levels score data
    }

    /// <summary>
    /// 
    /// </summary>
    public void LoadData()
    {
        DataMgr.Instance.Gold = PlayerPrefs.GetInt("Gold");
        Debug.Log("Loading language..." + PlayerPrefs.GetString("Lang"));
        //LocalizationService.Instance.Localization = PlayerPrefs.GetString("Lang");
        if (PlayerPrefs.GetString("Lang") =="")
            LocalizationService.Instance.Localization = "English";
        else
            LocalizationService.Instance.Localization = PlayerPrefs.GetString("Lang");

        DataMgr.Instance.Vibration = PlayerPrefs.GetInt("Vib");
        LoadItemData();
        LoadInventoryItems();
        LoadLevelData();
        LoadShopData();
    }
    /// <summary>
    /// 
    /// </summary>
    public void RemoveData()
    {
        Debug.Log("Delete");
        if ( File.Exists(Application.persistentDataPath  + _invFileName))
            File.Delete(Application.persistentDataPath + _invFileName);
        if (File.Exists(Application.persistentDataPath + _levelFileName))
            File.Delete(Application.persistentDataPath + _levelFileName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stage"></param>
    /// <param name="lvl"></param>
    /// <returns></returns>
    public List<ShopItemTableEntry> GetShopItemList()
    {
        return _shopItemList;

    }

    public List<ShopItem> GetGameItems()
    {
        return _gameItemList;
    }

    public List<EquipmentItem> GetInventoryItems()
    {
        return _inventoryItems;
    }

    


    /// <summary>
    /// 
    /// </summary>
    /// <param name="eI"></param>
    /// <param name="saveToFile"></param>
    public void AddInventoryItem(EquipmentItem eI, bool saveToFile = true)
    {
        _inventoryItems.Add(eI);
        if (saveToFile)
            SaveInventoryItems();
    }
	#endregion


	#region Private Methods

	#endregion


	#region Properties
    public bool DataLoaded { get { return _dataLoaded; } private set { _dataLoaded = true; } }
    public int Gold { get { return _gold; } set { _gold = value; } }
    public int Vibration { get { return _vibration; } set { _vibration = value; } }
	#endregion

	#region Private Serialized Fields

	#endregion

	#region Private Non-serialized Fields
    //TODO: read from file
    [SerializeField]
    private List<ShopItem> _gameItemList;
    
    [SerializeField]
    private List<ShopItemTableEntry> _shopItemList;

    [SerializeField]
    private List<EquipmentItem> _inventoryItems;

    [SerializeField]
    private TextAsset _itemDataTA;  //XML file containing game items data (as Text Asset)
    [SerializeField]
    private TextAsset _shopDataTA;  //XML file containing shop data : items displayed pool, prob list..
    [SerializeField]
    private TextAsset _inventoryDataTA; //TODO: load and write from persistent data path
    [SerializeField]
    private TextAsset _levelDataTA;

    [SerializeField]
    private string _levelFileName;
    [SerializeField]
    private string _invFileName;

    private bool _readingGoldItem;
    private List<ShopItem> _availableShopItemList;
    private List<QualityDistribution> _tempDistr;
    private List<ItemSizePair> _tempSizePait;

    private int _gold, _vibration;

    private bool _dataLoaded;
	#endregion
}
