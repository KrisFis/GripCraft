using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

namespace GripEngine
{
	public enum BlockType {GRASS, DIRT, SAND, ROCK}

	public enum MessageType {
		TILE_GENERATED,
		LOADING_UPDATE, 
		WORLD_GENERATED, 
		GAME_QUIT, 
		LOAD_COMPLETE
		};

	public static class Core
	{
		public static void SetActiveSafe(this GameObject _object, bool _activeState)
		{
			if(_object.activeSelf != _activeState)
			{
				_object.SetActive(_activeState);
			}
		}

		public static void SetCursorActiveSafe(bool _activeState)
		{
			if(Cursor.visible == _activeState)
				return;
				
			Cursor.visible = _activeState;
			Cursor.lockState = (_activeState) ? CursorLockMode.None : CursorLockMode.Locked;
		}

		public static void SetVSyncActiveSafe(bool _activeState)
		{
			int count = ((_activeState) ? 1 : 0);

			if(QualitySettings.vSyncCount != count)
				QualitySettings.vSyncCount = count;
		}
	}

	namespace GameManagement
	{
		public static class Game
		{
			public static bool isRunning {get; private set;}
			private static string savesFolder;

			static Game()
			{
				savesFolder = Application.persistentDataPath + "/Worlds/";
				isRunning = false;
			}

			public static void DeleteGame(string _worldName)
			{
				if(Directory.Exists(savesFolder + _worldName))
				{
					Directory.Delete(savesFolder + _worldName, true);
				}
			}

			public static void SaveGame(string _worldName)
			{
				Data.playerPos = WorldBehaviour.playerSingleton.transform.position;
				
				Save(_worldName);
			}

			private static void Save(string _worldName)
			{
				DeleteGame(_worldName);
				CreateFolder(_worldName);

				TextWriter writer, blockWriter;
				string lineText;
				
				/* PLAYER INFO */
				writer = File.CreateText(savesFolder + _worldName + "/PlayerInfo");
				
				Vector3 playerPos = Data.playerPos;
				
				lineText = playerPos.x + "_" + playerPos.y + "_" + playerPos.z;
				writer.WriteLine(lineText);

				lineText = Data.worldSeed;
				writer.WriteLine(lineText);

				writer.WriteLine(Data.worldName);

				writer.Close();

				/* WorldInfo (TileInfo) + BlockInfo*/
				writer = File.CreateText(savesFolder + _worldName + "/WorldInfo");
				blockWriter = File.CreateText(savesFolder + _worldName + "/BlockInfo");

				foreach(Vector3 tilePos in Data.tileList.Keys)
				{
					lineText = string.Empty;
					lineText+= tilePos.x + "_" + tilePos.y + "_" + tilePos.z;

					writer.WriteLine(lineText);

					lineText = string.Empty;

					foreach(Vector3 blockPos in Data.tileList[tilePos].blockList.Keys)
					{
						lineText+= Data.tileList[tilePos].blockList[blockPos] + "_";
						lineText+= blockPos.x + "_" + blockPos.y + "_" + blockPos.z;
						lineText+= "/"; 
					}

					blockWriter.WriteLine(lineText);
				}

				writer.Close();
				blockWriter.Close();
			}

			public static bool LoadGame(string _worldName, out int errorMsg) //With diagnostics
			{
				if(!FilesExist(_worldName, out errorMsg))
					return false;

				LoadGame(_worldName);

				return true;
			}

			public static void LoadGame(string _worldName) //Without diagnostics
			{
				
				/*
					Bylo by dobrý vytvořit "corrupted data" exception, jinak hra padne.
					Zatím budu předpokládat srozumitelnost dat.
				 */

				TextReader reader, blockReader;
				string lineText;
				string[] parameters;

				/* PlayerInfo */
				reader = File.OpenText(savesFolder + _worldName + "/PlayerInfo");

				lineText = reader.ReadLine();
				parameters = lineText.Split('_');

				Data.playerPos = new Vector3(float.Parse(parameters[0]), float.Parse(parameters[1]), float.Parse(parameters[2]));

				lineText = reader.ReadLine();
				Data.worldSeed = lineText;
				
				lineText = reader.ReadLine();
				Data.worldName = lineText;

				reader.Close();

				/* WorldInfo + BlockInfo*/
				reader = File.OpenText(savesFolder + _worldName + "/WorldInfo");
				blockReader = File.OpenText(savesFolder + _worldName + "/BlockInfo");

				while((lineText = reader.ReadLine()) != null) // EOF
				{
					parameters = lineText.Split('_');
					Vector3 tilePos = new Vector3(int.Parse(parameters[0]), int.Parse(parameters[1]), int.Parse(parameters[2]));
					
					lineText = blockReader.ReadLine();
					string[] preParameters = lineText.Split('/');

					Dictionary<Vector3, BlockType> blocks = new Dictionary<Vector3, BlockType>();
					for(int i = 0; i < (preParameters.Length-1); i++) // -1 -> because of the last '/' END OF LINE
					{
						parameters = null;
						parameters = preParameters[i].Split('_');
						
						BlockType blockType;
						Vector3 blockPos;

						switch(parameters[0])
						{
							case "GRASS":
								blockType = BlockType.GRASS;
								break;
							case "DIRT":
								blockType = BlockType.DIRT;
								break;
							case "SAND":
								blockType = BlockType.SAND;
								break;
							case "ROCK":
								blockType = BlockType.ROCK;
								break;
							default:
								return;
						}

						blockPos = new Vector3(int.Parse(parameters[1]), int.Parse(parameters[2]), int.Parse(parameters[3]));
						blocks.Add(blockPos, blockType);
					}

					Data.AddTile(tilePos, ref blocks);
				}

				reader.Close();
				blockReader.Close();
			}

			public static bool GetSavesDirectoryNames(out string[] _names)
			{
				_names = null;

				List<string> names = new List<string>();

				if(!Directory.Exists(savesFolder))
					return false;

				DirectoryInfo dirs = new DirectoryInfo(savesFolder);
    			foreach (DirectoryInfo dir in dirs.GetDirectories("*", SearchOption.TopDirectoryOnly))
    			{	
        			names.Add(dir.Name);
    			}

				if(names.Count <= 0)
					return false;

				_names = names.ToArray();
				return true;
			}

			private static void CreateFolder(string _worldName)
			{
				if(!Directory.Exists(savesFolder + _worldName))
					Directory.CreateDirectory(savesFolder + _worldName);
			}

			private static bool FilesExist(string _worldName, out int _error)
			{
				_error = 0;

				if(!File.Exists(savesFolder + _worldName + "/PlayerInfo"))
				{
					_error = 1;
					return false;
				}
				if(!File.Exists(savesFolder + _worldName + "/WorldInfo"))
				{
					_error = 2;
					return false;
				}
				if(!File.Exists(savesFolder + _worldName + "/BlockInfo"))
				{
					_error = 3;
					return false;
				}

				return true;
			}

			private static bool FilesExist(string _worldName)
			{
				if(!File.Exists(savesFolder + _worldName + "/PlayerInfo"))
					return false;
				if(!File.Exists(savesFolder + _worldName + "/WorldInfo"))
					return false;
				if(!File.Exists(savesFolder + _worldName + "/BlockInfo"))
					return false;
				
				return true;
			}
			
			public static void ChangeScene(int _sceneNumber)
			{
				switch(_sceneNumber)
				{
					case 0:
						isRunning = false;
						break;
					case 1:
						isRunning = true;
						break;
					default:
						return;
				}

				SceneManager.LoadScene(_sceneNumber);
			}

			public static void QuitGame()
			{
				Application.Quit();
			}
		}

		public static class Data
		{
			public static Dictionary<Vector3, Tile> tileList {get; private set;}
			public static Vector3 playerPos;
			public static string worldName;
			public static string worldSeed;

			static Data()
			{
				tileList = new Dictionary<Vector3, Tile>();
				playerPos = Vector3.zero;
			}

			public static void ConvertWorldSeed(out int[] _seed)
			{
				_seed = new int[2];
				_seed[0] = int.Parse(worldSeed.Substring(0,3));
				_seed[1] = int.Parse(worldSeed.Substring(3,3));
			}

			public static void ReStart()
			{
				tileList.Clear();
				playerPos = Vector3.zero;
				worldName = string.Empty;
				worldSeed = string.Empty;
			}

			public static bool AddTile(Vector3 _tilePos, ref Dictionary<Vector3, BlockType> _blocks)
			{
				try
				{
					tileList.Add(_tilePos, new Tile());
				}
				catch
				{
					return false;
				}
				

				if(_blocks == null)
					return true;
				
				foreach(Vector3 blocksPos in _blocks.Keys)
				{
					AddBlock(blocksPos, _blocks[blocksPos], _tilePos);
				}
				
				return true;
			}

			public static bool AddBlock(Vector3 _blockPos, BlockType _blockType, Vector3 _tilePos)
			{
				try
				{
					tileList[_tilePos].blockList.Add(_blockPos, _blockType); 
				}
				catch
				{
					return false;
				}

				return true;
			}

			public static bool RemoveBlock(Vector3 _blockPos, Vector3 _tilePos)
			{
				try
				{
					tileList[_tilePos].blockList.Remove(_blockPos); 
				}
				catch
				{
					return false;
				}

				return true;
			}

			public static int GetBlockCount(Vector3 _tilePos)
			{
				return tileList[_tilePos].blockList.Count;
			}

			public class Tile
			{
				public Dictionary<Vector3, BlockType> blockList;

				public Tile()
				{
					blockList = new Dictionary<Vector3, BlockType>();
				}
			}
		}

		public static class Settings
		{
			public const float cameraVerticalSpeed = 6.0f, cameraHorizonstalSpeed = 4.0f;
			public const float blockPlaceDistance = 5f;
			public const int heightWorldSize = 128; //128 for rays, 127 for block builds
			public const float blockToughness = 0.4f;
			public const int maxDistance = 128;

			
			public const int tileSize = 16, tileHeightScale = 48, tileHeight = 8;
			public const float tileDetailScale = 64.0f;
		}
	}
}
