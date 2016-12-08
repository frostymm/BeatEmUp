using UnityEngine;
using System.Collections;

public class FloorScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GameManager.Instance().GetLayer(gameObject.layer).AddFloor(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
