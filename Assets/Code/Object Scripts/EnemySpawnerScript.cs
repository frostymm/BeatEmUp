using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawnerScript : MonoBehaviour {

	public bool IsBoss = false;
	public int NumberOfEnemies = 5;
	public bool SpawnAllAtOnce = true;
	public float DistanceToPlayer = 20f;
	public float DistanceToSpawn = 5f;
	public float TimeBetweenSpawns = 2f;
	private bool Spawned = false;
	public int howManySpawned = 0;
	private bool WaveDefeated = false;

	public bool LockCam = true;
	public float DistancePassedSpawnToLockCam = 10f;

	private List<GameObject> npcs = new List<GameObject> ();

	// Use this for initialization
	void Start () {
	}

	void WaveDefeatedCheck()
	{
		for(int i = npcs.Count - 1; i >=0; i--)
		{
			if(npcs[i] == null)
				npcs.RemoveAt(i);
		}

		if(npcs.Count == 0)
			WaveDefeated = true;

		if(WaveDefeated)
		{
			GameManager.Instance().CamIsLocked = false;
			DestroyImmediate(gameObject);

			if(IsBoss)
				GameManager.Instance().GameOver("win");
		}
	}

	void SpawnEnemiesAtOnce()
	{
		for(int i = 0; i < NumberOfEnemies; i++)
		{
			if(IsBoss)
				npcs.Add ((GameObject) Instantiate(Resources.Load("Prefabs/Boss"), new Vector3(gameObject.transform.position.x + Random.Range(-DistanceToSpawn, DistanceToSpawn), gameObject.transform.position.y, gameObject.transform.position.z), Quaternion.identity));
			else
				npcs.Add ((GameObject)Instantiate(Resources.Load("Prefabs/Enemy"), new Vector3(gameObject.transform.position.x + Random.Range(-DistanceToSpawn, DistanceToSpawn), gameObject.transform.position.y, gameObject.transform.position.z), Quaternion.identity));
		}
		
		Spawned = true;
	}
	
	float timeToSpawn = 0;
	int enemiesSpawned = 0;
	void SpawnEnemiesOverTime()
	{
		if(timeToSpawn < Time.time)
		{
			if(IsBoss)
				npcs.Add ((GameObject)Instantiate(Resources.Load("Prefabs/Boss"), new Vector3(gameObject.transform.position.x + Random.Range(-DistanceToSpawn, DistanceToSpawn), gameObject.transform.position.y, gameObject.transform.position.z), Quaternion.identity));
			else
				npcs.Add ((GameObject)Instantiate(Resources.Load("Prefabs/Enemy"), new Vector3(gameObject.transform.position.x + Random.Range(-DistanceToSpawn, DistanceToSpawn), gameObject.transform.position.y, gameObject.transform.position.z), Quaternion.identity));
			timeToSpawn = Time.time + TimeBetweenSpawns;
			enemiesSpawned++;
			howManySpawned = enemiesSpawned;
		}
	
		if(enemiesSpawned >= NumberOfEnemies)
			Spawned = true;
	}

	void StartSpawnEnemiesOverTime()
	{
		SpawnEnemiesOverTime();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(!Spawned)
		{
			bool playerClose = false;
			foreach( MovingEntity pe in GameManager.Instance().GetPlayers() )
			{
				if(Vector3.Distance(gameObject.transform.position, pe.GetPosition()) < DistanceToPlayer)
					playerClose = true;
			}

			if(playerClose)
			{
				if(SpawnAllAtOnce)
					SpawnEnemiesAtOnce();
				else
				{
					StartSpawnEnemiesOverTime();
				}
				if(!GameManager.Instance().CamIsLocked)
					GameManager.Instance().LockCam(gameObject.transform.position.x + DistancePassedSpawnToLockCam);
			}
		}
		else
		{
			WaveDefeatedCheck();
		}
	}
}
