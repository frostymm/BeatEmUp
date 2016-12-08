using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Collections.Generic;

public class HitBox
{
	public float m_Damage = 0;
	public float m_HitLag = 0;
	public float m_KnockBack = 0;
	public float m_Trajectory = 0;
	public float m_LifeTime = -1;
	
	public Vector3 m_Position = new Vector3(0, 0, 0);
	public float m_Size = 1;
	
	public GameObject m_Owner;
	public GameObject m_GameObject;
	
	public GameObject m_AttachedObject;
	public bool m_Attached = false;
	
	public List<GameObject> m_AffectedObjects = new List<GameObject>();
	
	public delegate void SelfDestroyer(object sender, System.EventArgs ea);
	public event SelfDestroyer DestroyMe;
	public void OnDestroyMe()
	{
		SelfDestroyer temp = DestroyMe;
		if (temp != null)
		{
			temp(this, new System.EventArgs());
		}
	}
	
	public void AddAffectedObject(GameObject go)
	{
		m_AffectedObjects.Add(go);
	}
	
	public List<GameObject> GetAffectedObjects()
	{
		return m_AffectedObjects;
	}
	
	public void SetFollowObject(GameObject go)
	{
		m_AttachedObject = go;
		m_Attached = true;

		//m_GameObject.transform.IsChildOf(m_AttachedObject.transform);
	}
	
	public void SetPosition(Vector3 pos)
	{
		m_Position = pos;
		m_GameObject.transform.position = pos;
	}
	
	public HitBox(Vector3 position, float size, GameObject owner,
	float damage, float hitlag, float knockback, float traject, float lifetime)
	{
		m_Position = position;
		m_Size = size;
		m_Owner = owner;
		m_Damage = damage;
		m_HitLag = hitlag;
		m_KnockBack = knockback;
		m_Trajectory = traject;
		m_LifeTime = lifetime;
		
		m_GameObject = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/HitBox"), m_Position, Quaternion.identity);
		
		HitBoxScript hbScript = (HitBoxScript)m_GameObject.GetComponent(typeof(HitBoxScript));
		hbScript.OwnerHB = this;
		
		m_GameObject.layer = m_Owner.layer;
	}

	public HitBox(GameObject AttachedToObject, float size, GameObject owner,
	              float damage, float hitLag, float knockBack, float traject, float lifeTime)
	{
		m_Size = size;
		m_Owner = owner;
		m_Damage = damage;
		m_HitLag = hitLag;
		m_KnockBack = knockBack;
		m_Trajectory = traject;
		m_LifeTime = lifeTime;
		m_AttachedObject = AttachedToObject;
		m_Attached = true;
		
		m_GameObject = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/HitBox"), AttachedToObject.transform.position, Quaternion.identity);
		//m_GameObject.transform.parent = AttachedToObject.transform;
		
		HitBoxScript hbScript = (HitBoxScript)m_GameObject.GetComponent(typeof(HitBoxScript));
		hbScript.OwnerHB = this;
		
		m_GameObject.layer = m_Owner.layer;
	}

	public HitBox(GameObject AttachedToObject, float size, GameObject owner,
	              float damage, float hitLag, float knockBack, float traject)
	{
		m_Size = size;
		m_Owner = owner;
		m_Damage = damage;
		m_HitLag = hitLag;
		m_KnockBack = knockBack;
		m_Trajectory = traject;
		m_AttachedObject = AttachedToObject;
		m_Attached = true;
		
		m_GameObject = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/HitBox"), AttachedToObject.transform.position, Quaternion.identity);
		//m_GameObject.transform.IsChildOf(AttachedToObject.transform);

		HitBoxScript hbScript = (HitBoxScript)m_GameObject.GetComponent(typeof(HitBoxScript));
		hbScript.OwnerHB = this;
		
		m_GameObject.layer = m_Owner.layer;
	}
	
	~HitBox()
	{
		//Debug.Log("Destroying HitBox!");
	}
	
	public void Start()
	{
		m_GameObject.transform.localScale = new Vector3(m_Size, m_Size);
	}
	
	public void Update(float dt)
	{	
		if(m_LifeTime != -1)
		{
			if(m_Attached)
			{
				m_GameObject.transform.position = m_AttachedObject.transform.position;
			}

			if(m_LifeTime < 0)
			{
				MonoBehaviour.Destroy(m_GameObject);
				OnDestroyMe();
			}
			
			m_LifeTime -= dt;
		}
	}
}
