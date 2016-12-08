using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Layer{

	List<GameObject> m_Floors = new List<GameObject>();
	
	public void AddFloor(GameObject go)
	{
		m_Floors.Add(go);
	}
	
	public List<GameObject> GetFloors()
	{
		return m_Floors;
	}
}
