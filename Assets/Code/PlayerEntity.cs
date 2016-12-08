using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerEntity : MovingEntity
{
	//an instance of the state machine class
	StateMachine<PlayerEntity> m_StateMachine;
	
	//Player Specific Stats
	private int m_EnemiesKilled = 0;
	private float Score = 0f;
	private int m_Lives = 3;

	public PlayerEntity(GameObject go) : base(go)
	{
		m_EntityType = EntityTypes.entity_type.ET_Playable;
		m_StateMachine = new StateMachine<PlayerEntity> (this);
		m_StateMachine.SetCurrentState (PlayerOwnedStates.Neutral.Instance());
		m_StateMachine.SetGlobalState (PlayerOwnedStates.InAir.Instance());
		m_Speed = 4;
		m_Team = 0;

		GameManager.Instance().addPlayer(this);
	}

	~PlayerEntity()
	{
		m_StateMachine = null;
	}

	public StateMachine<PlayerEntity> GetFSM()
	{
		return m_StateMachine;
	}

	private List<string> m_InputBuffer = new List<string>();
	private int m_InputBufferLength = 2;
	public void AddInput( string input )
	{
		if(m_InputBuffer.Count <= m_InputBufferLength)
			m_InputBuffer.Add (input); 
	}
	public string GetInput(){ return m_InputBuffer[0]; }
	public void RemoveInput(){ m_InputBuffer.RemoveAt(0); }
	private bool m_InputAvailable = false;
	public bool InputAvailable
	{
		get
		{
			if(m_InputBuffer.Count > 0)
				return true;
			else
				return false;
		}
		set
		{
			m_InputAvailable = value;
		}
	}
	private float m_BufferInputTimer = 0;
	private float m_BufferInputWait = 0.1f;
	
	public override void Death(GameObject murderer)
	{
		base.Death(murderer);
		
		Debug.Log("Kill: " + this);
		m_Lives--;
		
		if(m_Lives < 0)
		{
			GameManager.Instance().GameOver("Diley");
			GameManager.Instance().IsGameover = true;
			m_GameObject.renderer.enabled = false;
		}
		else
			Respawn();
	}
	
	public void Respawn()
	{
		m_Health = 100;
		SetPosition(GameManager.Instance().GetPlayerSpawn());
	}

	public override void Start()
	{
		base.Start ();
	}

	public override void Update(float dt)
	{
		if (!GameManager.Instance().IsGameover)
		{
			//Debug.Log("InState: " + GetFSM().CurrentState());

			if(Time.time > m_BufferInputTimer
				&& Input.GetButtonDown("Punch"))
			{
				m_BufferInputTimer = Time.time + m_BufferInputWait;
				AddInput("Punch");
			}

			m_StateMachine.Update ();
			base.Update (dt);
		}
	}
}
