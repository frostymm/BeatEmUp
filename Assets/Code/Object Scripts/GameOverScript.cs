using UnityEngine;
using System.Collections;

public class GameOverScript : MonoBehaviour 
{
	
	void OnGUI()
	{
		if(GameManager.Instance().GetGameOverOutcome() == "win")
			GUI.Box(new Rect((Screen.width / 2) - 100, Screen.height / 3, 200, 150), "Congratulations! You Win!");
		else
			GUI.Box(new Rect((Screen.width / 2) - 100, Screen.height / 3, 200, 150), "Congratulations! You Suck!");
		
		if(GUI.Button(new Rect((Screen.width / 2) - 50, (Screen.height / 3) + 30, 100, 100), "Play Again"))
		{				
			GameManager.Instance().RestartGame();
		}
	}
}
