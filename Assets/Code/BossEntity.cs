using UnityEngine;
using System.Collections;

public class BossEntity : NPCEntity 
{
	public BossEntity(GameObject go) : base(go)
	{
		SetFloorOffset(1.78f);
		m_Health = 50;

		GetFSM().SetCurrentState (BossOwnedStates.Neutral.Instance());
		GetFSM().SetGlobalState (BossOwnedStates.InAir.Instance());
	}

	public override void Death(GameObject murderer)
	{
		//GameManager.Instance ().GameOver ("win");

		base.Death(murderer);
	}
}
