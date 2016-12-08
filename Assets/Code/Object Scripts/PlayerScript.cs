using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour {

	private PlayerEntity m_PlayerEntity = null;

	public Animator anim;
	public Transform[] GroundLines;

	public GameObject body;
	public GameObject blueFist;
	public GameObject redFist;

	// Use this for initialization
	void Start () {
		m_PlayerEntity = new PlayerEntity (gameObject);
		m_PlayerEntity.SetGroundRayPositions(GroundLines);
		m_PlayerEntity.SetAnimator(anim);

		GameObject[] bodyParts = new GameObject[3];
		bodyParts[(int)body_parts.BlueFist] = blueFist;
		bodyParts[(int)body_parts.Body] = body;
		bodyParts[(int)body_parts.RedFist] = redFist;
		m_PlayerEntity.m_BodyParts = bodyParts;

		m_PlayerEntity.Start ();
	}
	
	// Update is called once per frame
	void Update () {
		m_PlayerEntity.Update (Time.deltaTime);
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		m_PlayerEntity.SetCollision (collision);
	}

	void OnCollisionExit2D(Collision2D collision)
	{
		m_PlayerEntity.SetCollision (null);
	}
}
