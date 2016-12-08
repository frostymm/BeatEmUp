using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCEntity : MovingEntity {

	//an instance of the state machine class
	StateMachine<NPCEntity> m_StateMachine;
	MovingEntity m_Target;
	
	public NPCEntity(GameObject go) : base(go)
	{
		m_EntityType = EntityTypes.entity_type.ET_NPC;
		m_StateMachine = new StateMachine<NPCEntity> (this);
		m_StateMachine.SetCurrentState (NPCOwnedStates.Neutral.Instance());
		m_StateMachine.SetGlobalState (NPCOwnedStates.InAir.Instance());
		m_Speed = 3;
		m_Health = 20;
		m_Team = 1;
		
		GameManager.Instance().addNPC(this);
	}
	
	~NPCEntity()
	{
		m_StateMachine = null;
	}
	
	public StateMachine<NPCEntity> GetFSM()
	{
		return m_StateMachine;
	}

	public override void Death(GameObject murderer)
	{
		base.Death(murderer);
		
		MonoBehaviour.Destroy(m_GameObject);
		
		Debug.Log("Kill: " + this);
		GameManager.Instance().DestroyEntity(this);
	}

	private bool m_IsBoss = false;
	public bool IsBoss
	{
		get
		{
			return m_IsBoss;
		}
		set
		{
			m_IsBoss = value;
		}
	}
	
	public MovingEntity GetTarget() { return m_Target; }
	
	public MovingEntity FindClosestPlayer()
	{
		List<MovingEntity> players = GameManager.Instance().GetPlayers();
		MovingEntity currPlayer = null;
		float currDistance = -1;
		
		foreach(MovingEntity player in players)
		{
			float tempDist = Vector2.Distance(player.GetGameObject().transform.position, GetGameObject().transform.position);
			if(currDistance == -1 || tempDist < currDistance)
			{
				currPlayer = player;
				currDistance = tempDist;
			}
		}
		
		return currPlayer;
	}
	
	public override void Start()
	{
		base.Start ();
	}
	
	public override void Update(float dt)
	{
		if(this != null){
			m_StateMachine.Update ();
			base.Update (dt);
		}
		
		m_Target = FindClosestPlayer();
		
		if(GameManager.Instance().DebugMode && Input.GetKeyDown(KeyCode.P))
			m_StateMachine.ChangeState( NPCOwnedStates.Attacking.Instance());
	}
}
