using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using GripEngine;
using GripEngine.GameManagement;

public class WorldBuilder : WorldBehaviour
{
    private bool isCalculated = true;
    private float loadingAdd, loadingPerc;

	private Transform terrain;
	private bool Generated = true;

	public Queue<Vector3> buildQue {get; private set;}
	private int[] seed;

	void Awake()
	{
		buildQue = new Queue<Vector3>();
	}

	void Start()
	{
		terrain = new GameObject("Terrain").transform;
		terrain.transform.position = Vector3.zero;
		Data.ConvertWorldSeed(out seed);
	}

    public void BuildWorld()
	{
		int size =  Settings.maxDistance / (Settings.tileSize * 2);

		Queue<Vector3> tileQue = new Queue<Vector3>();

		Vector3 center = new Vector3((int)Data.playerPos.x / (int)Settings.tileSize, 0, (int)Data.playerPos.z / (int)Settings.tileSize );

		for(int x = -size; x <= size; x++)
		{
			for(int z = -size; z <= size; z++)
			{
				tileQue.Enqueue(new Vector3(center.x + (x * Settings.tileSize), 0, center.z + (z * Settings.tileSize)));
			}
		}

		loadingPerc = 0;
		loadingAdd = (100.0f / tileQue.Count);
		
		AddToBuildQueue(tileQue);
	}

	public void AddToBuildQueue(Queue<Vector3> _tilePos)
	{
		if(_tilePos.Count <= 0)
			return;

		foreach(Vector3 pos in _tilePos)
		{
			buildQue.Enqueue(pos);
		}

		if(Generated)
		{
			StartCoroutine(BuildAllTiles());
		}
	}

	private IEnumerator BuildAllTiles()
	{
		Generated = false;
		Coroutine Calc = null;

		while(buildQue.Count > 0)
		{
			/* Calculating */
			if(!TileExist(buildQue.Peek()))
			{
				if(Calc != null)
					StopCoroutine(Calc);

				Calc = StartCoroutine(CalculateTile(buildQue.Peek()));
			}
			
			yield return new WaitUntil(() => isCalculated);

			BuildTile(buildQue.Peek());

			buildQue.Dequeue();

			loadingPerc+=loadingAdd;
			CallMessage(MessageType.LOADING_UPDATE, (Mathf.Ceil(loadingPerc)).ToString());

			yield return null;
		}

		StopAllCoroutines();
		Generated = true;

		yield break;
	}

	private void BuildTile(Vector3 _tilePos) //HAVE TO EXIST
	{
		GameObject tile;

		CreateTile(terrain, out tile, _tilePos);

		Dictionary<Vector3, BlockType> blocks = Data.tileList[_tilePos].blockList;

		foreach(Vector3 blockPos in blocks.Keys)
		{	
			CreateBlock(blocks[blockPos], blockPos, tile);
		}

		
		CombineBlocks(tile);
		tile.transform.position = _tilePos;
	}

	private bool TileExist(Vector3 _tilePos)
	{
		foreach(Vector3 tile in Data.tileList.Keys)
		{
			if(tile == _tilePos)
			{
				return true;
			}
		}

		return false;
	}

    private IEnumerator CalculateTile(Vector3 _tilePos)
	{
		isCalculated = false;
		Dictionary<Vector3, BlockType> blocks = new Dictionary<Vector3, BlockType>();

		for(int z=0; z < Settings.tileSize; z++)
		{
			for(int x=0; x < Settings.tileSize; x++)
			{
				int y = (int)(Mathf.PerlinNoise((_tilePos.x + x + seed[0]) / Settings.tileDetailScale,
				(_tilePos.z + z + seed[1]) / Settings.tileDetailScale) * Settings.tileHeightScale);

				if(y > (Settings.tileHeightScale - 24))
				{
					blocks.Add(new Vector3(x,y,z), BlockType.GRASS);
				}
				else if(y > (Settings.tileHeightScale - 35))
				{
					blocks.Add(new Vector3(x,y,z), BlockType.DIRT);
				}
				else if(y > (Settings.tileHeightScale - 40))
				{
					blocks.Add(new Vector3(x,y,z), BlockType.SAND);
				}
				else
				{
					blocks.Add(new Vector3(x,y,z), BlockType.ROCK);
				}
				
			}
			yield return null;
		}

		Data.AddTile(_tilePos, ref blocks);
		isCalculated = true;
	}
}