using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GameManagement;

public class WorldManager : WorldBehaviour {

	private bool isGeneratedLocally = false;
	private bool isGenerating = false;

	public static World mainWorld {get; private set;}

	private float loadingPerc = 0, loadingAdd = 0;

	void Awake()
	{
		// !IMPORTANT
		worldSingleton = this;
		// !IMPORTANT

		//Initialize Managers
		Tiler.Initialize();
		
		RegisterCalls();
	}

	private IEnumerator BuildTilesRecursively(Vector3[] _position)
	{
		isGeneratedLocally = false;

		Tile GeneratedTile;

		GeneratedTile = Tiler.Add();

		StartCoroutine(GenerateTile(GeneratedTile, _position[_position.Length-1] * World.tileSize));

		while(!isGeneratedLocally)
		{
			yield return null;
		}

		GeneratedTile.gameObject.SetActiveSafe(false);

		Vector3[] newPos = new Vector3[_position.Length-1];

		for(int i = 0; i < newPos.Length; i++)
		{
			newPos[i] = _position[i];
		}

		if(loadingAdd != 0)
		{
			loadingPerc += loadingAdd;
			CallMessage(MessageType.LOADING_UPDATE, (int)loadingPerc + "");
		}

		if(newPos.Length > 0)
			yield return BuildTilesRecursively(newPos);
		else if(isGenerating)
		{
			isGenerating = false;
		}
	}

	private void GenerateTilesAroundTile(Tile _tile)
	{
		List<Vector3> tilesToGenerate = new List<Vector3>();
		int[] tileTempPos = {(int)_tile.transform.position.x, (int)_tile.transform.position.z};

		for(int i = 1; i <= 2; i++)
		{
			if(Tiler.GetTile(new Vector3(tileTempPos[0] + (32 * Mathf.Pow(-1,i)), 0, tileTempPos[1])) == null)
			{
				tilesToGenerate.Add(new Vector3(tileTempPos[0] / 32f + Mathf.Pow(-1,i) , 0, tileTempPos[1] / 32f));
			}
		}

		for(int i = 1; i <= 2; i++)
		{
			if(Tiler.GetTile(new Vector3(tileTempPos[0], 0, tileTempPos[1] + (32 * Mathf.Pow(-1,i)))) == null)
			{
					tilesToGenerate.Add(new Vector3(tileTempPos[0] / 32f, 0, tileTempPos[1] / 32f + Mathf.Pow(-1,i)));
			}
		}

		if(tilesToGenerate.Count > 0)
		{
			isGenerating = true;
			StartCoroutine(BuildTilesRecursively(tilesToGenerate.ToArray()));
		}

				
	}

	private IEnumerator GenerateTile(Tile tile, Vector3 position)
	{
		int[] localSeed = {int.Parse(mainWorld.seed.Substring(0,3)), int.Parse(mainWorld.seed.Substring(3,3))};

		for(int z=0; z <= World.tileSize; z++)
		{
			for(int x=0; x<= World.tileSize; x++)
			{
				int y = (int)(Mathf.PerlinNoise((position.x + x + localSeed[0]) / World.tileDetailScale,
				(position.z + z + localSeed[1]) / World.tileDetailScale) * World.tileHeightScale);

				if(y > 15)
				{
					CreateBlock(BlockType.GRASS, new Vector3(x,y,z), tile.gameObject);
				}
				else if(y > 10)
				{
					CreateBlock(BlockType.DIRT, new Vector3(x,y,z), tile.gameObject);
				}
				else if(y > 5)
				{
					CreateBlock(BlockType.SAND, new Vector3(x,y,z), tile.gameObject);
				}
				else
				{
					CreateBlock(BlockType.ROCK, new Vector3(x,y,z), tile.gameObject);
				}
			}
			yield return null;
		}

		CombineBlocks(tile.gameObject);
		tile.transform.position = position;
		isGeneratedLocally = true;
	}

	public void BuildWorld(World _worldObj)
	{
		if(_worldObj.GetLocalWorldSize() % 2 != 0)
		{
			Debug.LogError("LocalWorldSize is even!!");
			Game.QuitGame();
			return;
		}

		mainWorld = _worldObj;

		int size = mainWorld.GetLocalWorldSize();

		//First few tiles, others may spawn later
		List<Vector3> posList = new List<Vector3>();

		for(int x = -(size/2); x <= (size/2); x++)
		{
			for(int z = -(size/2); z <= (size/2); z++)
			{
				posList.Add(new Vector3(x,0,z));
			}
		}

		loadingPerc = 0;
		loadingAdd = (100.0f / posList.Count);
		
		StartCoroutine(BuildTilesRecursively(posList.ToArray()));

	}

	protected override void OnWorldGenerated()
	{
		loadingAdd = 0;
		loadingPerc = 0;
	}
	
	//Povolená vzdálenost od tile
	private const int maxDistance = 128;

	/* FUNCTION FOR REACTIVATING TILES (ODGENEROVÁNÍ A PŘIGENEROVÁNÍ) */
	public void RegenerateTiles()
	{
		foreach (Tile tile in Tiler.AllTilesToArray())
		{
			Vector3 tilePosition = tile.transform.position + (Vector3.one / 2.0f);
 
			float xDistance = Mathf.Abs(tilePosition.x - playerSingleton.transform.position.x);
			float zDistance = Mathf.Abs(tilePosition.z - playerSingleton.transform.position.z);
 
			if (xDistance + zDistance > maxDistance) {
				tile.gameObject.SetActiveSafe(false);
			} else {
				tile.gameObject.SetActiveSafe(true);

				if(!isGenerating)
				{
					GenerateTilesAroundTile(tile);
				}
			}
		}
	}
}
