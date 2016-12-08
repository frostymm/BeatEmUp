using UnityEngine;
using System.Collections;

public class MainGameScript : MonoBehaviour {

	public GameObject background;
	public Transform playerSpawn;

	// Use this for initialization
	void Start () {
		GameManager.Instance().setBackground(background);
		GameManager.Instance().SetPlayerSpawn(playerSpawn.position);
		GameManager.Instance().Start();
		GameManager.Instance().SetGameObject(gameObject);
		
	}
	
	// Update is called once per frame
	void Update () {
		GameManager.Instance().Update();
	}
}
