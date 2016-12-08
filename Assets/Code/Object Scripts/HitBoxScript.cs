using UnityEngine;
using System.Collections;

public class HitBoxScript : MonoBehaviour {

	public HitBox OwnerHB;
	
	void OnTriggerEnter2D(Collider2D other) 
	{
		Debug.Log("Colliding" + other.gameObject);
		MovingEntity owner = GameManager.Instance().GetEntityByGameObject(OwnerHB.m_Owner);
		MovingEntity me = GameManager.Instance().GetEntityByGameObject(other.gameObject);
		
		if(me != null 
		&& other.gameObject != OwnerHB.m_Owner 
		&& !OwnerHB.GetAffectedObjects().Contains(other.gameObject)
		&& me.m_Team != owner.m_Team)
		{
			me.HandleDamage(OwnerHB);
			OwnerHB.AddAffectedObject(other.gameObject);
		}
	}

	// Use this for initialization
	void Start () {
		if(!GameManager.Instance().DebugMode)
			renderer.enabled = false;
		OwnerHB.Start();
		Debug.Log("HB on Layer: " + this.gameObject.layer);
		Debug.Log("Player on Layer: " + GameManager.Instance().GetPlayers()[0].getLayer());
	}
	
	// Update is called once per frame
	void Update () {
		OwnerHB.Update(Time.deltaTime);
	}
}
