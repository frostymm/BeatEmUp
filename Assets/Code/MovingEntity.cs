using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovingEntity : BaseGameEntity 
{
	protected BoxCollider2D m_CollisionBox;
	protected Collision2D m_Collision;
	protected int m_Layer = 8;
	public RaycastHit2D[] m_RayHit;
	public Transform[] m_RayPositions;
	private bool m_CanHitGround = false;
	private float m_FloorOffset = 0.78f;
	public int m_Team = 0;

	public GameObject[] m_BodyParts;
	
	//Stats
	protected float m_Health = 100;
	protected float m_Speed;
	protected float m_Poise = 0;
	protected float m_JumpingForce = 600;

	public MovingEntity(GameObject go)
	{
		m_GameObject = go;
	}

	~MovingEntity()
	{
		Debug.Log("Destroying MovingEntity!");
	}

	private Animator m_Animator;
	public void SetAnimator( Animator anim ){ m_Animator = anim; }
	public Animator GetAnimator(){ return m_Animator; }

	private int m_DirectionFacing = 1;
	public void SetDirectionFacing(float dir)
	{
		Debug.Log("SetDirection: " + this);
		int direction = Mathf.RoundToInt (dir);

		if(direction != 0 && direction != m_DirectionFacing)
		{
			Debug.Log("SettingDirection: " + this);
			m_DirectionFacing = direction;
			//m_GameObject.transform.localScale = Vector3.Scale(new Vector3(direction, 1, 1), m_GameObject.transform.localScale);
			m_GameObject.transform.Rotate(new Vector3(0, 180, 0));
			//GetAnimator().Play("FlipDirection");
		}


	}
	public int GetDirectionFacing(){ return m_DirectionFacing; }
	
	public GameObject GetFloorStanding()
	{
		List<RaycastHit2D> rays;
		if(AnyRaysColliding(out rays))
		{
			foreach(RaycastHit2D r in rays)
			{
				if(r.collider.gameObject.tag == "Ground")
					return r.collider.gameObject;
			}
		}
				
		return null;
	}
	
	public bool AnyRaysColliding()
	{
		if(m_RayHit == null)
			return false;
	
		foreach(RaycastHit2D r in m_RayHit)
		{
			if(r)
			{
				return true;
			}
		}
		return false;
	}
	
	public bool AnyRaysColliding(out List<RaycastHit2D> rays)
	{
		rays = new List<RaycastHit2D>();

		if(m_RayHit == null)
			return false;
		
		foreach(RaycastHit2D r in m_RayHit)
		{
			if(r)
			{
				rays.Add(r);
			}
		}
		
		if(rays.Count > 0)
			return true;
		
		return false;
	}
	
	public bool AllRaysColliding()
	{
		foreach(RaycastHit2D r in m_RayHit)
		{
			if(!r)
			{
				return false;
			}
		}
		return true;
	}
	
	public bool AllRaysColliding(out RaycastHit2D ray)
	{
		ray = new RaycastHit2D();
		
		foreach(RaycastHit2D r in m_RayHit)
		{
			if(!r)
			{
				ray = r;
				return false;
			}
		}
		return true;
	}
	
	public void RayCollisionsCheck()
	{
		float sensorDistance = 0.05f;
		
		if(m_RayPositions.Length > 0)
		{
			if(m_RayHit == null)
				m_RayHit  = new RaycastHit2D[m_RayPositions.Length];
			
			for(int i = 0; i < m_RayHit.Length; i++)
			{
				if(m_RayHit[i] = Physics2D.Linecast((Vector2)m_RayPositions[i].position, (Vector2)m_RayPositions[i].position + -Vector2.up * sensorDistance, 1 << m_Layer))
				{
					Debug.DrawLine((Vector2)m_RayPositions[i].position, (Vector2)m_RayPositions[i].position + -Vector2.up * sensorDistance, Color.red);
				}
				else
					Debug.DrawLine((Vector2)m_RayPositions[i].position, (Vector2)m_RayPositions[i].position + -Vector2.up * sensorDistance, Color.green);
			}
		}
	}
	
	public void SetGroundRayPositions(Transform[] pos) { m_RayPositions = pos; }
	
	public float GetFloorOffset() { return m_FloorOffset; }
	public void SetFloorOffset(float f) { m_FloorOffset = f; }
	
	public void SetCanHitGround(bool b){m_CanHitGround = b;}
	public bool CanHitGround(){ return m_CanHitGround; }

	public float getSpeed() {return m_Speed;}
	public void setSpeed( float s ) { m_Speed = s; }

	public void Move(float x, float y, float dx, float dy)
	{
		Vector2 movement = new Vector2(x * dx, y * dy);
		
		movement *= Time.deltaTime;

		if(m_EntityType == EntityTypes.entity_type.ET_NPC
			|| m_EntityType == EntityTypes.entity_type.ET_Playable 
		   && Mathf.Abs(GetPosition().x + movement.x - GameManager.Instance().GetCam().transform.position.x) < GameManager.Instance().GetCamEdgeDistance())
		{
			movement.x *= GetDirectionFacing();
			m_GameObject.transform.Translate(movement);
		}
	}
	
	public void Jump()
	{
		if(m_GameObject && m_GameObject.rigidbody2D.velocity.y <= 0)
			m_GameObject.rigidbody2D.AddForce(new Vector2(0, m_JumpingForce));

		GetAnimator().Play("Jump");
	}

	public void HandleDamage(HitBox hb)
	{
		m_Health -= hb.m_Damage;
		
		if (hb.m_KnockBack < m_Poise)
		{
			//stun in place
		}
		else
		{
			Debug.Log("Knocking down");
			//knock down
			float radians = Mathf.Deg2Rad * hb.m_Trajectory;
			Vector2 direction = new Vector2((float)Mathf.Cos(radians), (float)Mathf.Sin(radians));
			m_GameObject.rigidbody2D.velocity = (direction * (hb.m_KnockBack));
		}
		
		if(m_Health <= 0)
			Death(hb.m_Owner);
			
		Debug.Log("Health: " + m_Health);
	}
	
	//Velocity isn't used?
	public Vector2 Get2DVelocity() { return m_GameObject.rigidbody2D.velocity; }
	public void Set2DVelocity(Vector2 v) { m_GameObject.rigidbody2D.velocity = v; }
	
	public Collision2D GetCollision(){return m_Collision;}
	public void SetCollision(Collision2D collision){ m_Collision = collision; }
	
	public int getLayer() { return m_Layer; }
	public void SetLayer(int layer) { m_GameObject.layer = layer; m_Layer = layer;}
	
	public override void Death (GameObject murderer)
	{
		
	}

	public override void Start()
	{
	}

	public override void Update(float dt)
	{
		RayCollisionsCheck();
		//Debug.Log (m_GameObject.rigidbody2D.velocity);
	}
}

public enum body_parts
{
	Body,
	RedFist,
	BlueFist
};

