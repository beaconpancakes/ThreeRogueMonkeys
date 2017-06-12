/************************************************************************/
/* @Author: Author Name
 * @Date: Date
 * @Brief: BBBrief
 * @Description: DDDescription
 * ***********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stage : MonoBehaviour {

	#region Public Data
    public enum STAGE_STATE { UNLOCKED, LOCKED, COMPLETED, UNAVAILABLE }
	#endregion


	#region Behaviour Methods
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	#endregion

	#region Public Methods
    public int GetStageIndex()
    {
        return _stageIndex;
    }

    public string GetStageName()
    {
        return _stageName;
    }
    public List<Level> GetLevelList()
    {
        return _levelList;
    }

    
	#endregion


	#region Private Methods

	#endregion


	#region Properties

	#endregion

	#region Private Serialized Fields
    [SerializeField]
    private int _stageIndex;
    [SerializeField]
    private string _stageName;
    [SerializeField]
    private List<Level> _levelList;
	#endregion

	#region Private Non-serialized Fields

	#endregion
}
