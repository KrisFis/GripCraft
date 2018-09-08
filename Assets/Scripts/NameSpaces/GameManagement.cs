using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameManagement
{
	public static class Game
	{
		public static bool InGame = false;

		public static void SetCursorActiveSafe(bool _activeState)
		{
			if(Cursor.visible == _activeState)
				return;
			
			Cursor.visible = _activeState;
			Cursor.lockState = (_activeState) ? CursorLockMode.None : CursorLockMode.Locked;
		}

		public static void ChangeScene(int _sceneNumber)
		{
			switch(_sceneNumber)
			{
				case 0:
					InGame = false;
					break;
				case 1:
					InGame = true;
					break;
				default:
					return;
			}

			SceneManager.LoadScene(_sceneNumber);
		}

		public static void QuitGame()
		{
			if(InGame)
				WorldBehaviour.CallMessage("GAME_QUIT");
			Application.Quit();
		}

		public static int LoadData()
		{
			return 0;
		}

		public static int SaveData()
		{
			return 0;
		}

		public static void DeleteData()
		{

		}

		public static void SetActiveSafe(this GameObject _object, bool _activeState)
		{
			if(_object.activeSelf != _activeState)
			{
				_object.SetActive(_activeState);
			}
		}

		public static void DestroyBlock(this GameObject _block)
		{
			BlockManager manager = _block.GetComponent<BlockManager>();

			if(manager == null)
				return;

			manager.DestroyBlock();
			
		}
	}

	public static class Settings
	{
		public static float cameraVerticalSpeed = 6.0f, cameraHorizonstalSpeed = 4.0f;
		public static float blockPlaceDistance = 5f;
		public static int heightWorldSize = 128; //128 for rays, 127 for block builds
		public static float blockToughness = 0.4f;
	}

	public static class World
	{	
		public const int tileSize = 32, tileHeightScale = 20;
		public const float tileDetailScale = 25.0f;

		public static int[] seed {get; private set;} //6-místní kód
		
		public static int worldSize {get; private set;} // Current world size in local size

		public static void SetNew(string _seed, int _worldSize)
		{
			seed = new int[2];
			seed[0] = int.Parse(_seed.Substring(0,3));
			seed[1] = int.Parse(_seed.Substring(3,3));

			worldSize = _worldSize;
		}

		public static uint GetRealWorldSize()
		{
			return (uint)(worldSize * World.tileSize);
		}

		public static int GetLocalWorldSize()
		{
			return worldSize;
		}
	}

	public static class Tiler
	{
		private static List<Tile> tileList;
		private static GameObject terrain;
		private static GameObject tileResource;
		private static Material materialResources;

		public static void Initialize()
		{
			if(tileList != null)
				return;
			
			tileList = new List<Tile>();
			terrain = new GameObject("Terrain");
			terrain.transform.position = Vector3.zero;
			tileResource = Resources.Load<GameObject>("Blocks/TilePrefab");
			materialResources = Resources.Load<Material>("Blocks/Textures");
		}

		public static Tile Add()
		{
			Tile newTile = new Tile(tileList.Count, tileResource, materialResources);

			newTile.transform.SetParent(terrain.transform);

			tileList.Add(newTile);

			return newTile;
		}

		public static Tile GetTile(this GameObject _tileObject)
		{
			foreach(Tile tile in tileList)
			{
				if(tile.gameObject == _tileObject)
				{
					return tile;
				}
			}

			return null;
		}

		public static Tile GetTile(this Vector3 _tilePosition)
		{
			foreach(Tile tile in tileList)
			{
				if(tile.transform.position == _tilePosition)
				{
					return tile;
				}
			}

			return null;
		}

		public static Tile GetTile(int _tileIndex)
		{
			if(_tileIndex < 0 || _tileIndex > (tileList.Count-1))
				return null;

				return tileList[_tileIndex];
		}

		public static Tile[] AllTilesToArray()
		{
			return tileList.ToArray();
		}
	}
		
	public class Tile
	{
		public GameObject gameObject {get;private set;}
		public Transform transform {get;private set;}
		public readonly int index;

		public Tile(int _tileIndex, GameObject _gameObject, Material _material)
		{
			index = _tileIndex;
			gameObject = GameObject.Instantiate(_gameObject);
			gameObject.name = "Tile_" + _tileIndex;

			transform = gameObject.transform;
			gameObject.GetComponent<MeshRenderer>().sharedMaterial = _material;
		}
	}
}
