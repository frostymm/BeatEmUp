using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class NPCOwnedStates
{
	//----------------------------------------------------------------------------- Global States
	public class OnGround : State<NPCEntity>
	{
		private OnGround() {}
		private static OnGround m_Instance = new OnGround();
		public static OnGround Instance()
		{
			return m_Instance;
		}
		
		public override void Enter(NPCEntity NE)
		{
			Vector3 floor = NE.GetFloorStanding().transform.position;
			Vector3 PEPos = NE.GetGameObject().transform.position;
			
			NE.GetGameObject().rigidbody2D.gravityScale = 0; //Stop vertical movement
			NE.GetGameObject().rigidbody2D.velocity = Vector3.zero; //Stop vertical movement
			NE.GetGameObject().transform.position = new Vector3(PEPos.x, floor.y + NE.GetFloorOffset(), PEPos.z); //Set Position from ground

			NE.GetFSM().ChangeState(Neutral.Instance());
		}
		
		public override void Execute(NPCEntity NE)
		{
			CheckStateChange (NE);
			//NE.Jump();
		}
		
		public override void Exit(NPCEntity NE)
		{
			//Debug.Log("Exiting OnGround Global State");
		}
		
		public void CheckStateChange(NPCEntity NE)
		{
			if(!NE.AnyRaysColliding())
				NE.GetFSM().ChangeGlobalState(InAir.Instance());
				
			if(NE.GetFSM().CurrentState() != Attacking.Instance())
			{
				if(NE.GetTarget() != null)
					if(NE.GetTarget().GetGameObject().transform.position.y > NE.GetGameObject().transform.position.y 
					&& Mathf.Abs(NE.GetTarget().GetGameObject().transform.position.y - NE.GetGameObject().transform.position.y) > 3
					&& Mathf.Abs(NE.GetTarget().GetGameObject().transform.position.x - NE.GetGameObject().transform.position.x) < 3)
					{
						NE.Jump();
						NE.GetFSM().ChangeGlobalState(InAir.Instance());
					}
			}
		}
	};
	
	public class InAir : State<NPCEntity>
	{
		private InAir() {}
		private static InAir m_Instance = new InAir();
		public static InAir Instance()
		{
			return m_Instance;
		}
		
		public override void Enter(NPCEntity NE)
		{
			//Debug.Log("Entering InAir Global State");
			NE.SetCanHitGround(false);
			NE.GetGameObject().rigidbody2D.gravityScale = 2;
			
		}
		
		public override void Execute(NPCEntity NE)
		{
			CheckCanHitGround(NE);
			CheckStateChange(NE);
		}
		
		public override void Exit(NPCEntity NE)
		{
			//Debug.Log("Exiting InAir Global State");
		}
		
		public void CheckStateChange(NPCEntity NE)
		{
			if(NE.CanHitGround() && NE.GetFloorStanding() != null && GameManager.Instance().GetLayer(NE.getLayer()).GetFloors().Contains(NE.GetFloorStanding()))
				NE.GetFSM().ChangeGlobalState(OnGround.Instance());
		}
		
		public void CheckCanHitGround(NPCEntity NE)
		{
			if(!NE.CanHitGround() && NE.GetGameObject().rigidbody2D.velocity.y < 0)
				NE.SetCanHitGround(true);
		}
	};
	
	//----------------------------------------------------------------------------- States
	public class Neutral : State<NPCEntity>
	{
		private Neutral() {}
		private static Neutral m_Instance = new Neutral();
		public static Neutral Instance()
		{
			return m_Instance;
		}
		
		public override void Enter(NPCEntity NE)
		{
			//Debug.Log("Entering Neutral State");

			if(NE.GetFSM().GlobalState() != InAir.Instance())
				NE.GetAnimator().Play("Idle");
			else
				NE.GetAnimator().Play("Fall");
		}
		
		public override void Execute(NPCEntity NE)
		{
			CheckStateChange (NE);
		}
		
		public override void Exit(NPCEntity NE)
		{
			//Debug.Log("Exiting Neutral State");
		}
		
		public void CheckStateChange(NPCEntity NE)
		{
			if(NE.GetTarget() != null && NE.GetFSM().CurrentState() != Attacking.Instance())
				NE.GetFSM().ChangeState(Moving.Instance());
		}
	};
	
	public class Moving : State<NPCEntity>
	{
		private Moving() {}
		private static Moving m_Instance = new Moving();
		public static Moving Instance()
		{
			return m_Instance;
		}
		
		public override void Enter(NPCEntity NE)
		{
			//Debug.Log("Entering Moving State");
			
		}
		
		public override void Execute(NPCEntity NE)
		{
			CheckStateChange (NE);
			
			float direction;
			
			if(NE.GetTarget() != null)
			{
				if(NE.GetTarget().GetGameObject().transform.position.x > NE.GetGameObject().transform.position.x)
				direction = 1;
				else
				direction = -1;
				
				NE.Move(NE.getSpeed(), 0, direction, 0);

				if(NE.GetFSM().GlobalState() != InAir.Instance())
				{
					NE.SetDirectionFacing(direction);
				}
			}
		}
		
		public override void Exit(NPCEntity NE)
		{	
			//Debug.Log("Exiting Moving State");
		}
		
		public void CheckStateChange(NPCEntity NE)
		{
			if(NE.GetFSM ().CurrentState() != Attacking.Instance())
			{
				if(NE.GetTarget() == null)
					NE.GetFSM().ChangeState(Neutral.Instance());
					
				if(Vector2.Distance(NE.GetTarget().GetGameObject().transform.position, NE.GetGameObject().transform.position) < 1
				   && (NE.GetFSM().CurrentState() != InAir.Instance()))
					NE.GetFSM().ChangeState(Attacking.Instance());
			}
		}
	};
	
	//--------------------------------------------Attacking State
	public class Attacking : State<NPCEntity>
	{
		public List<HitBox> hbList;
		Attack currAttack;
		float timeTillExit;
		public bool attackDone = false;
		
		private void ChangeAttack(NPCEntity PE, Attack newAttack)
		{
			if(currAttack != null)
				currAttack.Exit(PE);
			
			currAttack = newAttack;
			currAttack.Enter(PE);
		}
		
		private Attacking() 
		{
			JumpAttack.Instance();
			NeutralCombo1.Instance();
		}
		
		private static Attacking m_Instance = new Attacking();
		public static Attacking Instance()
		{
			return m_Instance;
		}
		
		public override void Enter(NPCEntity PE)
		{	
			//Debug.Log("Entering Attacking State");
			
			hbList = new List<HitBox>();
			attackDone = false;
			
			if(PE.GetFSM().GlobalState() == InAir.Instance())
				ChangeAttack(PE, JumpAttack.Instance());
			else if(PE.GetFSM().PreviousState() == Neutral.Instance()
			        || PE.GetFSM().PreviousState() == Moving.Instance())
				ChangeAttack(PE, NeutralCombo1.Instance());
			else
				Debug.Log("Entering Attacking State with no attacks?");
		}
		
		public override void Execute(NPCEntity PE)
		{
			currAttack.Execute(PE);
			
			Debug.Log("CurrentAttack: " + currAttack);

			
			CheckStateChange(PE);
		}
		
		public override void Exit(NPCEntity PE)
		{
			Debug.Log("Exiting Attacking State");
			if(currAttack != null)
				currAttack.Exit(PE);
			
			foreach(HitBox hb in hbList)
			{
				hb.OnDestroyMe();
			}
			
			attackDone = false;
		}
		
		public void CheckStateChange(NPCEntity PE)
		{
			if (attackDone)
			{
				PE.GetFSM().ChangeState(Neutral.Instance());
			}
		}
		
		public enum attacks
		{
			nothing,
			neutralcombo1,
			jumpattack
		};
		
		//-------------------------------------------------------- Attacks
		public class JumpAttack : Attack
		{
			private static JumpAttack m_Instance = new JumpAttack();
			public static JumpAttack Instance()
			{
				return m_Instance;
			}
			
			public override void Enter(NPCEntity PE)
			{
				base.Enter(PE);
				
				PE.GetAnimator().Play("jumpattack");
				animation = "jumpattack";
				m_Attack = attacks.jumpattack;
			}
			
			public override void Execute(NPCEntity PE)
			{
				base.Execute(PE);
				
				if(TimeAnimated >= 0.30f)
				{
					if(AddHitBox(0))
					{
						HitBox hb = new HitBox(PE.m_BodyParts[(int)body_parts.Body], 1, PE.GetGameObject(), 10, 2, 5, 45, 0.50f);
						hbList.Insert(0, hb);
					}
				}
			}
			
			public override void Exit(NPCEntity PE)
			{
				base.Exit(PE);
			}
		}
		
		public class NeutralCombo1 : Attack
		{
			private static NeutralCombo1 m_Instance = new NeutralCombo1();
			public static NeutralCombo1 Instance()
			{
				return m_Instance;
			}
			
			public override void Enter(NPCEntity PE)
			{
				base.Enter(PE);
				
				PE.GetAnimator().Play("neutralcombo1");
				animation = "neutralcombo1";
				m_Attack = attacks.neutralcombo1;
			}
			
			public override void Execute(NPCEntity PE)
			{
				base.Execute(PE);
				
				if(TimeAnimated >= 0.35f)
				{
					if(AddHitBox(0))
					{
						HitBox hb = new HitBox(PE.m_BodyParts[(int)body_parts.BlueFist], 1, PE.GetGameObject(), 10, 2, 5, 45, 1.0f);
						hbList.Insert(0, hb);
					}
				}
				
				if(TimeAnimated >= 1.20f)
					canInterruptAttack = true;
			}
			
			public override void Exit(NPCEntity PE)
			{
				base.Exit(PE);
			}
		}
		
		public class Attack
		{
			public attacks m_Attack = attacks.nothing;
			public bool canInterruptAttack = false;
			public float TimeAnimated = 0;
			public string animation = "";
			public List<HitBox> hbList = new List<HitBox>();
			
			public bool AddHitBox(int id)
			{
				if(hbList.Count - 1 >= id)
				{
					if(hbList[id] != null)
						return false;
				}
				
				
				return true;
			}
			
			public float GetAnimationLength(NPCEntity PE)
			{
				return PE.GetAnimator().GetCurrentAnimationClipState(0)[0].clip.length;
			}
			
			public float GetAnimationNormalizedTime(NPCEntity PE)
			{
				return PE.GetAnimator().GetCurrentAnimatorStateInfo(0).normalizedTime;
			}
			
			public virtual void Enter(NPCEntity PE)
			{
				canInterruptAttack = false;
			}
			
			public virtual void Execute(NPCEntity PE)
			{
				TimeAnimated = GetAnimationLength(PE) * GetAnimationNormalizedTime(PE);
				
				if(TimeAnimated >= GetAnimationLength(PE))//if(!entity.GetAnimator().GetCurrentAnimatorStateInfo(0).IsName(animation))
				{
					Attacking.Instance().attackDone = true;
				}
			}
			
			public virtual void Exit(NPCEntity PE)
			{
				hbList.Clear();
			}
		}
	};

}
