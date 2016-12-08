using UnityEngine;
using System.Collections;

public abstract class State<entity_type>
{
	//this will execute when the state is entered
	public abstract void Enter(entity_type entity);
	
	//this is the states normal update function
	public abstract void Execute(entity_type entity);
	
	//this will execute when the state is exited. (My word, isn't
	//life full of surprises... ;o))
	public abstract void Exit(entity_type entity);
}
