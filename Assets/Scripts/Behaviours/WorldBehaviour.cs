using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GripEngine;

public class WorldBehaviour : BlockBehaviour {

	public static WorldManager worldSingleton;
	public static PlayerManager playerSingleton;
	private static List<WorldBehaviour> CallReceivers;

	static WorldBehaviour()
	{
		CallReceivers = new List<WorldBehaviour>();
	}

	public static void StartGame()
	{
		if(CallReceivers.Count > 0)
		{
			CallReceivers.Clear();
		}

		worldSingleton = null;
		playerSingleton = null;
	}

	protected void RegisterCalls()
	{
		CallReceivers.Add(this);
	}

	/* VIRTUAL METHODS */ /* CAN TAKE ONLY ONE PARAMETER */
	protected virtual void OnTileGenerated(int tileIndex) {}
	protected virtual void OnWorldGenerated() {}
	protected virtual void OnLoadComplete() {}
	protected virtual void OnLoadingUpdate(int loadingProgress) {}
	/* VIRTUAL METHODS */

	/* Call VIRTUAL METHOD on all inherited objects */
	public static void CallMessage(MessageType _id, string _parameter = "")
	{
		foreach(WorldBehaviour behaviour in CallReceivers.ToArray())
		{
			switch(_id)
			{
			case MessageType.TILE_GENERATED: //TileGenerated (int)
				behaviour.OnTileGenerated(int.Parse(_parameter));
				break;
			case MessageType.LOADING_UPDATE: //LoadingUpdate (int)
				behaviour.OnLoadingUpdate(int.Parse(_parameter));
				break;
			case MessageType.WORLD_GENERATED:
				behaviour.OnWorldGenerated();
				break;
			case MessageType.LOAD_COMPLETE:
				behaviour.OnLoadComplete();
				break;
			default:
				return;
			}
		}
	}
}
