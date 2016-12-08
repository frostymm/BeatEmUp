using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization;

class EntityManager
{
	private static EntityManager m_Instance = null;
	private static ObjectIDGenerator m_Generator = new ObjectIDGenerator ();
	private Dictionary<long, BaseGameEntity> m_EntityMap = new Dictionary<long, BaseGameEntity> ();
	
	public static EntityManager Instance ()
	{
			if (m_Instance == null) {
					m_Instance = new EntityManager ();
			}
		
			return m_Instance;
	}
	
	public bool GetEntityFromID (long id, out BaseGameEntity gameEntity)
	{
			if (m_EntityMap.TryGetValue (id, out gameEntity)) 
			{
					return true;
			}
			return false;
	}
	
	public long RegisterEntity (BaseGameEntity gameEntity)
	{
			bool firstTime;
			long newID = gameEntity.GetID ();
		
			if (newID == -1) {
					newID = m_Generator.GetId (gameEntity, out firstTime);
			}
		
			m_EntityMap [newID] = gameEntity;
		
			return newID;
	}
	
	public bool RemoveEntity (long id)
	{
			return m_EntityMap.Remove (id);
	}
}
