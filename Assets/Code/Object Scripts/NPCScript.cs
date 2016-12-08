using UnityEngine;
using System.Collections;

public class NPCScript : MonoBehaviour {

	private NPCEntity m_NPCEntity = null;

	public Animator anim;
	public Transform[] GroundLines;

	public GameObject body;
	public GameObject blueFist;
	public GameObject redFist;
	
	// Use this for initialization
	void Start () {
		m_NPCEntity = new NPCEntity (gameObject);
		m_NPCEntity.SetGroundRayPositions(GroundLines);

		m_NPCEntity.SetAnimator(anim);
		
		GameObject[] bodyParts = new GameObject[3];
		bodyParts[(int)body_parts.BlueFist] = blueFist;
		bodyParts[(int)body_parts.Body] = body;
		bodyParts[(int)body_parts.RedFist] = redFist;
		m_NPCEntity.m_BodyParts = bodyParts;

		m_NPCEntity.Start();
	}
	
	// Update is called once per frame
	void Update () {
		m_NPCEntity.Update (Time.deltaTime);
	}
	
	void OnCollisionEnter2D(Collision2D collision)
	{
		m_NPCEntity.SetCollision (collision);
		
		//Debug.Log (collision.gameObject);
	}
	
	void OnCollisionExit2D(Collision2D collision)
	{
		m_NPCEntity.SetCollision (null);
	}

	public void Flip()
	{
		gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x * -1, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
	}
}
