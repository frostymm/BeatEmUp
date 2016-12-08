using UnityEngine;
using System.Collections.Generic;

public class GameManager
{

	public GameObject m_Background;
	private GameObject m_GameObject;
	
	private static GameManager m_Instance = null;
	List<MovingEntity> m_Players = new List<MovingEntity>();
	List<NPCEntity> m_NPCs = new List<NPCEntity>();
	Dictionary<GameObject, MovingEntity> m_Entities = new Dictionary<GameObject, MovingEntity>();
	Dictionary<int, Layer> m_Layers = new Dictionary<int, Layer>();
	private bool m_DebugMode = false;
	private string m_GameOverOutcome;
	private bool m_IsGameover = false;

	private Camera m_Cam = Camera.main;
	private bool m_CamIsLocked = false;
	private float m_CamIsLockedAt;
	private Vector2 m_CamFollow = new Vector2 (2f, 3.3f);
	private float m_CamEdge = 9f;
	
	private Vector3 m_PlayerSpawn = new Vector3(0, 0, -2);

	public static GameManager Instance()
	{
		if (m_Instance == null)
		{
			m_Instance = new GameManager();
		}
		
		return m_Instance;
	}

	public List<NPCEntity> GetNPCs()
	{
		return m_NPCs;
	}
	
	public void addNPC(NPCEntity NE)
	{
		m_NPCs.Add(NE);
		m_Entities.Add(NE.GetGameObject(), NE);
	}
	
	public void removeNPC(NPCEntity NE)
	{
		m_NPCs.Remove(NE);
		m_Entities.Remove(NE.GetGameObject());
	}

	public void addPlayer(PlayerEntity PE)
	{
		m_Players.Add(PE);
		m_Entities.Add(PE.GetGameObject(), PE);
	}
	
	public Layer GetLayer(int i)
	{
		if(!m_Layers.ContainsKey(i))
			m_Layers.Add(i, new Layer());
		
		return m_Layers[i];
	}
	
	public MovingEntity GetEntityByGameObject(GameObject go)
	{
		if(m_Entities.ContainsKey(go))
			return m_Entities[go];
		else
			return null;
	}
	
	public bool DebugMode
	{ 
		get{ return m_DebugMode; } 
		set{ m_DebugMode = value; } 
	}
	
	public bool IsGameover
	{ 
		get{ return m_IsGameover; } 
		set{ m_IsGameover = value; } 
	}
	
	public bool CamIsLocked
	{
		get{ return m_CamIsLocked; }
		set{ m_CamIsLocked = value; }
	}

	public void LockCam (float pos)
	{
		m_CamIsLockedAt = pos;
		m_CamIsLocked = true;
	}

	public Camera GetCam()
	{ 
		if(m_Cam != null)
			return m_Cam; 
		else
			return Camera.main;
	}

	public Vector2 GetCamFollowDistance(){ return m_CamFollow; }
	public float GetCamEdgeDistance() { return m_CamEdge; }

	public List<MovingEntity> GetPlayers() { return m_Players; }

	public GameObject getBackground(){ return m_Background; }
	public void setBackground(GameObject bg){ m_Background = bg; }
	
	public void SetGameObject(GameObject go){m_GameObject = go;}
	
	public void SetPlayerSpawn(Vector3 pos)
	{
		m_PlayerSpawn = pos;
	}
	
	public Vector3 GetPlayerSpawn()
	{
		return m_PlayerSpawn;
	}
	
	public string GetGameOverOutcome(){ return m_GameOverOutcome; }

	public void GameOver(string outcome)
	{
		m_GameOverOutcome = outcome;
		m_GameObject.AddComponent("GameOverScript");
	}
	
	public void RestartGame()
	{
		Debug.Log ("Restarting Level");
		
		m_IsGameover = false;
		
		//Clear references
		m_Entities.Clear();
		m_Layers.Clear();
		m_NPCs.Clear();
		m_Players.Clear();
		
		//reload scene
		Application.LoadLevel(Application.loadedLevelName);
	}
	
	public void DestroyEntity(MovingEntity me)
	{
		if(me.GetEntityType() == EntityTypes.entity_type.ET_NPC)
			removeNPC(me as NPCEntity);
		me = null;
	}
	
	public void UpdateCamera()
	{
		float speed = 0.018f;
		float yOffset = 3.2f;

		if(GetCam() != null)
		{
			foreach(MovingEntity pe in m_Players)
			{
				float distx = Mathf.Abs(pe.GetPosition().x - GetCam().transform.position.x);
				float disty = Mathf.Abs(pe.GetPosition().y - GetCam().transform.position.y);
				if(distx > GetCamFollowDistance().x || disty > GetCamFollowDistance().y)
				{
					Vector2 movement = Vector2.Lerp(GetCam().transform.position, pe.GetPosition() + Vector3.up * yOffset, speed);

					if(!CamIsLocked)
						GetCam().transform.position = new Vector3(movement.x, movement.y, GetCam().transform.position.z);
					else
					{
						if(movement.x > m_CamIsLockedAt)
							movement.x = m_CamIsLockedAt;
						GetCam().transform.position = new Vector3(movement.x, movement.y, GetCam().transform.position.z);
					}
				}
			}
		}
	}

	// Use this for initialization
	public void Start () {
	}
	
	// Update is called once per frame
	public void Update () {
		if(Input.GetKeyDown(KeyCode.BackQuote))
			DebugMode = !DebugMode;
			
		if(DebugMode && Input.GetKeyDown(KeyCode.Escape))
			GameOver("win");

		if(DebugMode && Input.GetKeyDown(KeyCode.Q))
			CamIsLocked = !CamIsLocked;
		
		if(!IsGameover)
			UpdateCamera ();
	}
}
