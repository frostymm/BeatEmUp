using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerOwnedStates
{
	//----------------------------------------------------------------------------- Global States
	public class OnGround : State<PlayerEntity>
	{
		private OnGround() {}
		private static OnGround m_Instance = new OnGround();
		public static OnGround Instance()
		{
			return m_Instance;
		}
		
		public override void Enter(PlayerEntity PE)
		{
			//Debug.Log("Entering OnGround Global State");
			Vector3 floor = PE.GetFloorStanding().transform.position;
			Vector3 PEPos = PE.GetGameObject().transform.position;
			
			PE.GetGameObject().rigidbody2D.gravityScale = 0; //Stop vertical movement
			PE.GetGameObject().rigidbody2D.velocity = Vector3.zero; //Stop vertical movement
			PE.GetGameObject().transform.position = new Vector3(PEPos.x, floor.y + PE.GetFloorOffset(), PEPos.z); //Set Position from ground

			PE.GetFSM().ChangeState(Neutral.Instance());
		}
		
		public override void Execute(PlayerEntity PE)
		{	
			if(Input.GetAxis("Vertical") > 0)
			{
				if(PE.getLayer() != 9)
					SwitchLayer(PE, "up");
			}
			if(Input.GetAxis("Vertical") < 0)
			{
				if(PE.getLayer() != 8)
					SwitchLayer(PE, "down");
			}
		
			if (PE.GetFSM().CurrentState() != Attacking.Instance() && Input.GetButtonDown("Jump")) 
			{
				PE.Jump();
				PE.GetFSM().ChangeGlobalState(InAir.Instance());
			}
	
			CheckStateChange (PE);
		}
		
		public override void Exit(PlayerEntity PE)
		{
			//Debug.Log("Exiting OnGround Global State");
		}
	
		public void CheckStateChange(PlayerEntity PE)
		{
			if(!PE.AnyRaysColliding())
				PE.GetFSM().ChangeGlobalState(InAir.Instance());
			
			if(PE.GetFSM().CurrentState() != Attacking.Instance()
			   && PE.InputAvailable
			   && PE.GetInput() == "Punch")
			{
				PE.GetFSM().ChangeState(Attacking.Instance());
				PE.RemoveInput();
			}
		}
		
		public void SwitchLayer(PlayerEntity PE, string direction)
		{
			Vector3 MovedBack = new Vector3(PE.GetPosition().x, -1f, 0);
			Vector3 MovedFront = new Vector3(PE.GetPosition().x, -3f, 0);
			
			PE.GetFSM ().ChangeGlobalState(InAir.Instance());
			
			if(direction == "up")
			{
				PE.SetPosition(MovedBack);
				PE.SetLayer(9);
				PE.SetZIndex(-1);
			}
			else if(direction == "down")
			{
				PE.SetPosition(MovedFront);
				PE.SetLayer(8);
				PE.SetZIndex(-3);
			}
		}
	};
	
	public class InAir : State<PlayerEntity>
	{
		private InAir() {}
		private static InAir m_Instance = new InAir();
		public static InAir Instance()
		{
			return m_Instance;
		}
		
		public override void Enter(PlayerEntity PE)
		{
			//Debug.Log("Entering InAir Global State");
			PE.SetCanHitGround(false);
			PE.GetGameObject().rigidbody2D.gravityScale = 2;
			
		}
		
		public override void Execute(PlayerEntity PE)
		{
			CheckCanHitGround(PE);
			CheckStateChange (PE);
		}
		
		public override void Exit(PlayerEntity PE)
		{
			//Debug.Log("Exiting InAir Global State");
		}
	
		public void CheckStateChange(PlayerEntity PE)
		{	
			if(PE.CanHitGround() && PE.GetFloorStanding() != null && GameManager.Instance().GetLayer(PE.getLayer()).GetFloors().Contains(PE.GetFloorStanding()))
				PE.GetFSM().ChangeGlobalState(OnGround.Instance());
			
			if(PE.GetFSM().CurrentState() != Attacking.Instance()
			   && PE.InputAvailable
			   && PE.GetInput() == "Punch")
			{
				PE.RemoveInput();
				PE.GetFSM().ChangeState(Attacking.Instance());
			}
		}
		
		public void CheckCanHitGround(PlayerEntity PE)
		{
			if(!PE.CanHitGround() 
			   && PE.GetGameObject().rigidbody2D.velocity.y < 0)
			{
				if(PE.GetFSM().CurrentState() != Attacking.Instance())
					PE.GetAnimator().Play("Fall");
				PE.SetCanHitGround(true);
			}
		}
	};
	
	//----------------------------------------------------------------------------- States
	public class Neutral : State<PlayerEntity>
	{
		private Neutral() {}
		private static Neutral m_Instance = new Neutral();
		public static Neutral Instance()
		{
			return m_Instance;
		}
		
		public override void Enter(PlayerEntity PE)
		{
			//Debug.Log("Entering Neutral State");

			if(PE.GetFSM().GlobalState() != InAir.Instance())
				PE.GetAnimator().Play("Idle");
			else
				PE.GetAnimator().Play("Fall");
			
		}
		
		public override void Execute(PlayerEntity PE)
		{
			CheckStateChange (PE);
		}
	
		public override void Exit(PlayerEntity PE)
		{
			//Debug.Log("Exiting Neutral State");
		}
	
		public void CheckStateChange(PlayerEntity PE)
		{
			if (Input.GetAxis ("Horizontal") != 0 && PE.GetFSM().CurrentState() != Attacking.Instance()) 
			{
				PE.GetFSM().ChangeState(Moving.Instance());
			}
		}
	};
	
	public class Moving : State<PlayerEntity>
	{
		private Moving() {}
		private static Moving m_Instance = new Moving();
		public static Moving Instance()
		{
			return m_Instance;
		}
		
		public override void Enter(PlayerEntity PE)
		{
			//Debug.Log("Entering Moving State");
			
		}
		
		public override void Execute(PlayerEntity PE)
		{
			CheckStateChange (PE);

			PE.Move(PE.getSpeed(), 0, Input.GetAxis ("Horizontal"), 0);
			if(PE.GetFSM().GlobalState() != InAir.Instance())
			{
				PE.SetDirectionFacing(Input.GetAxis("Horizontal"));
			}
		}
		
		public override void Exit(PlayerEntity PE)
		{	
			//Debug.Log("Exiting Moving State");
		}
	
		public void CheckStateChange(PlayerEntity PE)
		{
			if (Input.GetAxis ("Horizontal") == 0 && PE.GetFSM ().CurrentState() != Attacking.Instance())
			{
				PE.GetFSM().ChangeState(Neutral.Instance());
			}
		}
	};

	//--------------------------------------------Attacking State
	public class Attacking : State<PlayerEntity>
	{
		public List<HitBox> hbList;
		Attack currAttack;
		float timeTillExit;
		public bool attackDone = false;

		private void ChangeAttack(PlayerEntity PE, Attack newAttack)
		{
			if(currAttack != null)
				currAttack.Exit(PE);

			currAttack = newAttack;
			currAttack.Enter(PE);
		}
	
		private Attacking() 
		{
			JumpAttack.Instance();
			DashAttack.Instance();
			NeutralCombo1.Instance();
			NeutralCombo2.Instance();
			NeutralCombo3.Instance();
		}

		private static Attacking m_Instance = new Attacking();
		public static Attacking Instance()
		{
			return m_Instance;
		}
		
		public override void Enter(PlayerEntity PE)
		{	
			//Debug.Log("Entering Attacking State");

			hbList = new List<HitBox>();
			attackDone = false;
			
			if(PE.GetFSM().GlobalState() == InAir.Instance())
				ChangeAttack(PE, JumpAttack.Instance());
			else if(PE.GetFSM().PreviousState() == Neutral.Instance())
				ChangeAttack(PE, NeutralCombo1.Instance());
			else if(PE.GetFSM().PreviousState() == Moving.Instance())
				ChangeAttack(PE, DashAttack.Instance());
			else
				Debug.Log("Entering Attacking State with no attacks?");
		}
		
		public override void Execute(PlayerEntity PE)
		{
			checkHitBoxRemove();

			currAttack.Execute(PE);

			Debug.Log("CurrentAttack: " + currAttack);

			if(currAttack.canInterruptAttack 
			   && PE.InputAvailable
			   && PE.GetInput() == "Punch")
			{
				if(currAttack.m_Attack == attacks.neutralcombo1)
					ChangeAttack(PE, NeutralCombo2.Instance());
				else if(currAttack.m_Attack == attacks.neutralcombo2)
					ChangeAttack(PE, NeutralCombo3.Instance());

				PE.RemoveInput();
			}
			
			CheckStateChange(PE);
		}
		
		public override void Exit(PlayerEntity PE)
		{
			Debug.Log("Exiting Attacking State");
			if(currAttack != null)
				currAttack.Exit(PE);
			
			foreach(HitBox hb in hbList)
			{
				hb.OnDestroyMe();
			}
			
			hbList = null;

			attackDone = false;
		}
		
		public void CheckStateChange(PlayerEntity PE)
		{
			if (attackDone)
			{
				PE.GetFSM().ChangeState(Neutral.Instance());
			}
		}
		
		public void checkHitBoxRemove()
		{
			foreach(HitBox hb in hbList)
			{
				if(hb == null)
					hbList.Remove(hb);
			}
		}

		public enum attacks
		{
			nothing,
			neutralcombo1,
			neutralcombo2,
			neutralcombo3,
			dashattack,
			jumpattack
		};

		//-------------------------------------------------------- Attacks
		public class DashAttack : Attack
		{
			private static DashAttack m_Instance = new DashAttack();
			public static DashAttack Instance()
			{
				return m_Instance;
			}

			public override void Enter(PlayerEntity PE)
			{
				Debug.Log("Calling DashAttack Enter");
				base.Enter(PE);

				PE.GetAnimator().Play("dashattack");
				animation = "dashattack";
				m_Attack = attacks.dashattack;
			}
			
			public override void Execute(PlayerEntity PE)
			{
				Debug.Log("Calling DashAttack Execute");
				base.Execute(PE);

				if(TimeAnimated <= 0.90f)
					PE.Move(4f, 0, PE.GetDirectionFacing(), 0);
				
				if(TimeAnimated >= 0.55f)
				{
					if(AddHitBox(0))
					{
						HitBox hb = new HitBox(PE.m_BodyParts[(int)body_parts.BlueFist], 2, PE.GetGameObject(), 10, 2, 5, 45, 1.0f);
						hbList.Insert(0, hb);
					}
				}
			}

			public override void Exit(PlayerEntity PE)
			{
				Debug.Log("Calling DashAttack Exit");
				base.Exit(PE);
			}
		}
		
		public class JumpAttack : Attack
		{
			private static JumpAttack m_Instance = new JumpAttack();
			public static JumpAttack Instance()
			{
				return m_Instance;
			}

			public override void Enter(PlayerEntity PE)
			{
				base.Enter(PE);

				PE.GetAnimator().Play("jumpattack");
				animation = "jumpattack";
				m_Attack = attacks.jumpattack;
			}
			
			public override void Execute(PlayerEntity PE)
			{
				base.Execute(PE);
				
				if(TimeAnimated >= 0.15f)
				{
					if(AddHitBox(0))
					{
						HitBox hb = new HitBox(PE.m_BodyParts[(int)body_parts.BlueFist], 1, PE.GetGameObject(), 10, 2, 5, 45, 0.50f);
						hbList.Insert(0, hb);
					}
				}
			}

			public override void Exit(PlayerEntity PE)
			{
				base.Exit(PE);
			}
		}
		
		public class NeutralCombo3 : Attack
		{
			private static NeutralCombo3 m_Instance = new NeutralCombo3();
			public static NeutralCombo3 Instance()
			{
				return m_Instance;
			}

			public override void Enter(PlayerEntity PE)
			{
				base.Enter(PE);

				PE.GetAnimator().Play("neutralcombo3");
				animation = "neutralcombo3";
				m_Attack = attacks.neutralcombo3;
			}
			
			public override void Execute(PlayerEntity PE)
			{
				base.Execute(PE);
				
				if(TimeAnimated >= 0.2f)
				{
					if(AddHitBox(0))
					{
						HitBox hb = new HitBox(PE.m_BodyParts[(int)body_parts.RedFist], 1, PE.GetGameObject(), 10, 2, 5, 45, 0.2f);
						hbList.Insert(0, hb);
					}
				}
			}

			public override void Exit(PlayerEntity PE)
			{
				base.Exit(PE);
			}
		}
		
		public class NeutralCombo2 : Attack
		{
			private static NeutralCombo2 m_Instance = new NeutralCombo2();
			public static NeutralCombo2 Instance()
			{
				return m_Instance;
			}

			public override void Enter(PlayerEntity PE)
			{
				base.Enter(PE);

				PE.GetAnimator().Play("neutralcombo2");
				animation = "neutralcombo2";
				m_Attack = attacks.neutralcombo2;
			}
			
			public override void Execute(PlayerEntity PE)
			{
				base.Execute(PE);
				
				if(TimeAnimated >= 0.2f)
				{
					if(AddHitBox(0))
					{
						HitBox hb = new HitBox(PE.m_BodyParts[(int)body_parts.BlueFist], 1, PE.GetGameObject(), 10, 2, 5, 45, 0.2f);
						hbList.Insert(0, hb);
					}
				}
				
				if(TimeAnimated >= 0.30f)
					canInterruptAttack = true;
			}

			public override void Exit(PlayerEntity PE)
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

			public override void Enter(PlayerEntity PE)
			{
				base.Enter(PE);

				PE.GetAnimator().Play("neutralcombo1");
				animation = "neutralcombo1";
				m_Attack = attacks.neutralcombo1;
			}
			
			public override void Execute(PlayerEntity PE)
			{
				base.Execute(PE);
				
				if(TimeAnimated >= 0.2f)
				{
					if(AddHitBox(0))
					{
						HitBox hb = new HitBox(PE.m_BodyParts[(int)body_parts.RedFist], 1, PE.GetGameObject(), 10, 2, 5, 45, 0.2f);
						hbList.Insert(0, hb);
					}
				}
				
				if(TimeAnimated >= 0.30f)
					canInterruptAttack = true;
			}

			public override void Exit(PlayerEntity PE)
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
			
			public float GetAnimationLength(PlayerEntity PE)
			{
				return PE.GetAnimator().GetCurrentAnimationClipState(0)[0].clip.length;
			}
			
			public float GetAnimationNormalizedTime(PlayerEntity PE)
			{
				return PE.GetAnimator().GetCurrentAnimatorStateInfo(0).normalizedTime;
			}

			public virtual void Enter(PlayerEntity PE)
			{
				canInterruptAttack = false;
			}
			
			public virtual void Execute(PlayerEntity PE)
			{
				TimeAnimated = GetAnimationLength(PE) * GetAnimationNormalizedTime(PE);
				
				if(TimeAnimated >= GetAnimationLength(PE))//if(!entity.GetAnimator().GetCurrentAnimatorStateInfo(0).IsName(animation))
				{
					Attacking.Instance().attackDone = true;
				}
			}

			public virtual void Exit(PlayerEntity PE)
			{
				hbList.Clear();
			}
		}
	};



}