using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RecycleVerticalListViewExampleController :  MonoBehaviour, IRecycleVerticalListViewDelegate{

	public GameObject CellPrefab;
	public int size = 140;

	public int GetNumOfCells(){
		return size;
	}

	public GameObject GetCellPrefab(){
		return CellPrefab;
	}

	public void PopulateEachCell(GameObject cell, int index){
		var comp = cell.GetComponentInChildren<Text> ();
		comp.text = (index+1).ToString();
	}

	public float GetCellDistance(){
		return 10f;
	}


}
