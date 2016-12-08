using UnityEngine;
using System.Collections;

public class StateMachine<entity_type>
{
	//a pointer to the agent that owns this instance
	entity_type m_pOwner;
	
	State<entity_type> m_pCurrentState;
	
	//a record of the last state the agent was in
	State<entity_type> m_pPreviousState;
	
	//this is called every time the FSM is updated
	State<entity_type> m_pGlobalState;
	
	public StateMachine(entity_type owner)
	{
		m_pOwner= owner;
		m_pCurrentState = null;
		m_pPreviousState = null;
		m_pGlobalState = null;
	}
	
	//use these methods to initialize the FSM
	public void SetCurrentState(State<entity_type> s){m_pCurrentState = s;}
	public void SetGlobalState(State<entity_type> s) {m_pGlobalState = s;}
	public void SetPreviousState(State<entity_type> s){m_pPreviousState = s;}
	
	//call this to update the FSM
	public void Update()
	{
		//if a global state exists, call its execute method, else do nothing
		if (m_pGlobalState != null)   
			m_pGlobalState.Execute(m_pOwner);
		
		//same for the current state
		if (m_pCurrentState != null) 
			m_pCurrentState.Execute(m_pOwner);
		
		//MessageDispatcher.Instance().DispatchDelayedMessages();
	}
	
	//change to a new state
	public void  ChangeState(State<entity_type> pNewState)
	{
		//keep a record of the previous state
		m_pPreviousState = m_pCurrentState;
		
		//call the exit method of the existing state
		m_pCurrentState.Exit(m_pOwner);
		
		//change state to the new state
		m_pCurrentState = pNewState;
		
		//call the entry method of the new state
		m_pCurrentState.Enter(m_pOwner);
	}

	public void ChangeGlobalState(State<entity_type> pNewState)
	{
		m_pGlobalState.Exit (m_pOwner);

		m_pGlobalState = pNewState;

		m_pGlobalState.Enter (m_pOwner);
	}

	//change state back to the previous state
	public void  RevertToPreviousState()
	{
		ChangeState(m_pPreviousState);
	}
	
	//returns true if the current state's type is equal to the type of the
	//class passed as a parameter. 
	public bool isInState(State<entity_type> st)
	{
		return m_pCurrentState.GetType() == st.GetType();
	}
	
	public State<entity_type> CurrentState() { return m_pCurrentState; }
	public State<entity_type> GlobalState() { return m_pGlobalState; }
	public State<entity_type> PreviousState() { return m_pPreviousState; }
}
