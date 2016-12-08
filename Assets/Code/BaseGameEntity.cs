using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class BaseGameEntity
{
	protected long m_ID = -1;
	protected GameObject m_GameObject;
	protected EntityTypes.entity_type m_EntityType = EntityTypes.entity_type.ET_Unknown;

	public BaseGameEntity ()
	{
			m_ID = EntityManager.Instance().RegisterEntity (this);
	}

	~BaseGameEntity ()
	{
			EntityManager.Instance().RemoveEntity (m_ID);
	}

	public void SetPose (Vector3 new_pos, Quaternion new_rot)
	{
		m_GameObject.transform.position = new_pos;
		m_GameObject.transform.rotation = new_rot;
	}

	public Vector3 GetPosition ()
	{
		return GetGameObject().transform.position;
	}

	public void SetPosition (Vector3 new_pos)
	{ 
		m_GameObject.transform.position = new_pos;
	}
	
	public void SetZIndex(float z)
	{
		Vector3 vec = m_GameObject.transform.position;
		m_GameObject.transform.position = new Vector3(vec.x, vec.y, z);
	}

	public Quaternion GetRotation ()
	{
		return m_GameObject.transform.rotation;
	}

	public void SetRotation (Quaternion new_rot)
	{
		m_GameObject.transform.rotation = new_rot;
	}

	public void SetGameObject (GameObject gameObject)
	{
		m_GameObject = gameObject;
	}

	public bool CreateGameObject (string prefabName)
	{
		m_GameObject = (GameObject)MonoBehaviour.Instantiate(Resources.Load(prefabName), new Vector3(GetPosition().x, GetPosition().y, 0), Quaternion.identity);
	
		return (m_GameObject != null);
	}

	public GameObject GetGameObject ()
	{
		return m_GameObject;
	}
	
	public abstract void Death(GameObject murderer);

	public EntityTypes.entity_type GetEntityType() { return m_EntityType; }
	public void SetEntityType(EntityTypes.entity_type new_type) { m_EntityType = new_type; }
	
	public abstract void Update(float dt);
	public abstract void Start();
	public long GetID() {return m_ID;}
	public void SetID(long ID) {m_ID = ID;}
}
