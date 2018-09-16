using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GripEngine.GameManagement;

public class WorldManager : WorldBehaviour {

	private WorldBuilder builder;
	
	void Start()
	{
		InvokeStartGame();

		worldSingleton = this;

		builder = this.GetComponent<WorldBuilder>();
	}

	void OnApplicationQuit()
	{
		Game.SaveGame(Data.worldName);
	}

	private void InvokeStartGame()
	{
		WorldBehaviour.StartGame();
	}

	public void BuildGame()
    {
     	builder.BuildWorld();
    }

	/* FUNCTION FOR REACTIVATING TILES */
	public void RemoveTiles()
	{
		Vector3 playerPos = playerSingleton.transform.position;
		GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");

		foreach (GameObject tile in tiles)
		{
			if (Mathf.Abs(Vector3.Distance(tile.transform.position, playerPos)) > Settings.maxDistance) {
				Destroy(tile);
			}
		}
	}

	public void BuildTiles()
	{
		int size =  Settings.maxDistance / (Settings.tileSize * 2);

		Vector3 center = new Vector3((int)playerSingleton.transform.position.x / Settings.tileSize, 0 , (int)playerSingleton.transform.position.z / Settings.tileSize);

		Queue<Vector3> tileQue = new Queue<Vector3>();

		GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");

		bool isFound;

		for(int x = (-size + (int)center.x); x <= (size + (int)center.x); x++)
		{
			for(int z = (-size + (int)center.z); z <= (size + (int)center.z); z++)
			{
				isFound = false;

				Vector3 tilePos = new Vector3(x * Settings.tileSize, 0,z * Settings.tileSize);

				foreach(Vector3 tile in builder.buildQue)
				{
					if(tile == tilePos)
					{
						isFound = true;
						break;
					}
				}

				if(isFound)
					continue;

				foreach(GameObject tile in tiles)
				{
					if(tile.transform.position ==  tilePos)
					{
						isFound = true;
						break;	
					}
				}

				if(!isFound)
					tileQue.Enqueue(tilePos);
			}
		}

		builder.AddToBuildQueue(tileQue);
	}
}
