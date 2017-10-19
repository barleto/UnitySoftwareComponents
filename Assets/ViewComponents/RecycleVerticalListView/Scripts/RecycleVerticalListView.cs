using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;

[System.Serializable]
[RequireComponent(typeof(ScrollRect))]
public class RecycleVerticalListView : MonoBehaviour {

	//default settings:
	[Header("Default settings")]
	public RectTransform ContentRecTransform;

	[Header("Scroll settings")]
	public bool AlwaysScroll = true;

	//delegates:
	[Header("Delegate")]
	[InterfaceOutlet(typeof(IRecycleVerticalListViewDelegate))]
	public UnityEngine.Object component;
	public IRecycleVerticalListViewDelegate listViewDelegate;

	int ListLength = 0;
	ScrollRect scrollRect;
	GameObject CellPrefab = null;
	float DistanceBetweenElements = 10;
	float numberOfCellsInViewPort = 0;
	float totalNumberOfCells = 0;
	float cellPrefabHeigth = 0;
	float scrollRectNeededHeigth = 0;

	//Refresh settings
	[Header("Refresh settings - not required")]
	public bool userefresh = false;
	public bool RotateRefreshImage = false;
	public float TopOffsetForRefreshing = 50;
	public Image refreshImage;
	public UnityEvent OnRefresh;

	[HideInInspector]
	public bool refreshLock = false;

	private Queue<GameObject> unusedCells = new Queue<GameObject> ();
	private List<ViwedCellHelper> usedCells = new List<ViwedCellHelper> ();


	void Awake(){
		if (component != null) {
			listViewDelegate = (IRecycleVerticalListViewDelegate)component;
		} else {
			Debug.LogError ("VerticalListView must have a delegate to work!");
		}
		scrollRect = GetComponent<ScrollRect> ();
	}

	void Start () {
		StartCoroutine(initializeList ());
	}

	public void UpdateList(){
		StartCoroutine(initializeList ());
	}

	private IEnumerator initializeList(){
		yield return new WaitForEndOfFrame ();
		ContentRecTransform.gameObject.SetActive (true);

		if (listViewDelegate != null) {
			ListLength = (listViewDelegate.GetNumOfCells());
			if(ListLength<0){
				Debug.LogError ("List length can not be less than 0!");
				yield break;
			}
		}

		if (listViewDelegate != null) {
			CellPrefab = (listViewDelegate.GetCellPrefab());
			ValidateCellPrefab (CellPrefab);
		}

		if (listViewDelegate != null) {
			DistanceBetweenElements = (listViewDelegate.GetCellDistance());
		}

		scrollRectNeededHeigth = ListLength * (DistanceBetweenElements + cellPrefabHeigth) - DistanceBetweenElements;

		CreateList ();

		refreshLock = true;
	}

	void ValidateCellPrefab (GameObject cellPrefab)
	{
		RectTransform cellPrefabRectTransform = cellPrefab.transform as RectTransform;
		if (cellPrefabRectTransform.anchorMax != new Vector2 (1, 1) || cellPrefabRectTransform.anchorMin != new Vector2 (0, 1)) {
			Debug.LogError ("CellPrefab Anchors must be Stretch Top!");
		}
		GetCellHeightFromPrefab ();
	}
		
	float GetScrollNormalizedPosition(){
		return Mathf.Clamp01 (scrollRect.verticalNormalizedPosition);
	}

	float GetViewPortNormalizedSize(){
		return (transform as RectTransform).rect.height/scrollRectNeededHeigth;
	}
		
	RangeInt GetIndexRangeOfViwableCells(){
		float viewPortHeigth = (transform as RectTransform).rect.height;
		float totalCellHeigth = cellPrefabHeigth + DistanceBetweenElements;
		float scrolled = ContentRecTransform.offsetMax.y;
		int start = (int)Mathf.Floor(scrolled/totalCellHeigth);
		if (start < 0) {
			start = 0;
		}
		int length = (int)Mathf.Ceil(viewPortHeigth/totalCellHeigth);
		if (start+length >= ListLength) {
			length = ListLength - start;
		}
		return new RangeInt(start,length);
	}

	void Update () {
		if(userefresh){
			if(!refreshLock && ContentRecTransform.offsetMax.y < 0){
				float percentage = ContentRecTransform.offsetMax.magnitude / TopOffsetForRefreshing;
				if (ContentRecTransform.offsetMax.magnitude < TopOffsetForRefreshing) {
					Color transparentColor = refreshImage.color;
					transparentColor.a = percentage;
					refreshImage.color = transparentColor;
					if(RotateRefreshImage){
						refreshImage.transform.rotation = new Quaternion(0,0,0,0);
						refreshImage.transform.Rotate (0,0,-360*percentage);
					}

				} else {
					Color transparentColor = refreshImage.color;
					transparentColor.a = 1.0f;
					refreshImage.color = transparentColor;
					refreshLock = true;
					OnRefresh.Invoke ();
				}
			}else{
				if(ContentRecTransform.offsetMax.magnitude==0){
					refreshLock = false;
				}
			}
		}
	}

	void GetCellHeightFromPrefab ()
	{
		cellPrefabHeigth = (-(CellPrefab.transform as RectTransform).offsetMin.y) + (CellPrefab.transform as RectTransform).offsetMax.y;
	}

	void CreateList(){

		if(!CellPrefab){
			Debug.LogWarning ("No Cell Prefab set.");
			return;
		}

		numberOfCellsInViewPort = Mathf.Ceil((transform as RectTransform).rect.height/(DistanceBetweenElements + cellPrefabHeigth));
		if(numberOfCellsInViewPort > ListLength){
			numberOfCellsInViewPort = ListLength;
		}
		totalNumberOfCells = numberOfCellsInViewPort + 1;
//		if(unusedCells.Count+usedCells.Count < totalNumberOfCells ){
			for(int i =0;i<totalNumberOfCells;i++){
				GameObject newCell = Instantiate (CellPrefab);
				newCell.transform.SetParent (ContentRecTransform,false);
				newCell.name = CellPrefab.name+"[unused]";
				newCell.SetActive (false);
				unusedCells.Enqueue (newCell);
			}
//		}
		SetContentRectHeight (scrollRectNeededHeigth);


		for(int index = 0; index<numberOfCellsInViewPort && index < ListLength;index++){
			var item = GetReusabelCell ();
			usedCells.Add (new ViwedCellHelper(item.gameObject,index));
			item.name = CellPrefab.name;
			if (listViewDelegate != null) {
				listViewDelegate.PopulateEachCell (item.gameObject,index);
			}
			SetListItemRectPosByIndex (item,index);
		}

	}

	RectTransform GetReusabelCell ()
	{
		GameObject newCell = unusedCells.Dequeue ();
		newCell.SetActive (true);
		RectTransform item = newCell.transform as RectTransform;
		return item;
	}

	void TestForRectAnchorPositions(RectTransform rt){
		if(rt.anchorMin.x != 0 || rt.anchorMin.y != 1 || rt.anchorMax.x!= 1 || rt.anchorMax.y!=1){
			throw new System.Exception ("All ListItems inside an ListView must have anchor position as Stretch-Top!");
		}
	}

	void SetListItemRectPosByIndex(RectTransform listItem, int index){
		float posY = (cellPrefabHeigth + DistanceBetweenElements) * index;
		listItem.anchorMin = new Vector2 (0, 1);
		listItem.anchorMax = new Vector2 (1, 1);
		listItem.offsetMax = - new Vector2 (0,posY);
		listItem.offsetMin = - new Vector2 (0,posY+cellPrefabHeigth);

	}

	void SetContentRectHeight(float neededHeigth){
		ContentRecTransform.anchorMax = new Vector2(1,1);
		ContentRecTransform.anchorMin = new Vector2(0,1);
		ContentRecTransform.offsetMax = new Vector2(0,0);
		if(neededHeigth < (gameObject.transform as RectTransform).rect.height){
			ContentRecTransform.offsetMin = new Vector2(0,-(gameObject.transform as RectTransform).rect.height);
		}else{
			ContentRecTransform.offsetMin = new Vector2(0,-neededHeigth);
		}
		ScrollRect scrollRect = gameObject.GetComponent<ScrollRect> (); 
		if (ContentRecTransform.sizeDelta.y <= (scrollRect.transform as RectTransform).rect.height && !AlwaysScroll) {
			scrollRect.vertical = false;
		}
	}

	bool IsCellInViewPort(int index){
		return usedCells.Where (obj => obj.index == index).SingleOrDefault () != null;
	}

	public void UpdateViewableCells(Vector2 pos){
		RangeInt range = GetIndexRangeOfViwableCells ();

		//deactivate hidden cells
		List<ViwedCellHelper> removeIndexes = new List<ViwedCellHelper>();
		foreach(var viwedCellHelper in usedCells){
			if (viwedCellHelper.index < range.start || viwedCellHelper.index > range.end) {
				removeIndexes.Add (viwedCellHelper);
			}
		}

		foreach (var viwedCellHelper in removeIndexes) {
			usedCells.Remove (viwedCellHelper);
			viwedCellHelper.cell.SetActive (false);
			unusedCells.Enqueue (viwedCellHelper.cell);
			viwedCellHelper.cell = null;
		}

		//activate and populate new cells that appear
		for(int index = range.start;index<=range.end && index < ListLength;index++){
			if(!IsCellInViewPort(index) && index >= 0){
				var item = GetReusabelCell ();
				usedCells.Add (new ViwedCellHelper(item.gameObject,index));
				item.name = CellPrefab.name;
				if (listViewDelegate != null) {
					listViewDelegate.PopulateEachCell (item.gameObject,index);
				}
				SetListItemRectPosByIndex (item,index);
			}
		}

	}

	public class ViwedCellHelper{
		public GameObject cell;
		public int index;

		public ViwedCellHelper(GameObject cell, int index){
			this.cell = cell;
			this.index = index;
		}

	}

}

public interface IRecycleVerticalListViewDelegate{
	int GetNumOfCells ();

	GameObject GetCellPrefab();

	void PopulateEachCell(GameObject cell, int index);

	float GetCellDistance();
}
	