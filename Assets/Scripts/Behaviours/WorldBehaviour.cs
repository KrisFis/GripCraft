using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBehaviour : BlockBehaviour {

	public static WorldManager worldSingleton;
	public static PlayerManager playerSingleton;
	private static List<WorldBehaviour> CallReceivers;

	protected enum MessageType {TILE_GENERATED,LOADING_UPDATE, WORLD_GENERATED, GAME_QUIT, LOAD_COMPLETE};

	protected void RegisterCalls()
	{
		if(CallReceivers == null)
		{
			CallReceivers = new List<WorldBehaviour>();
		}

		CallReceivers.Add(this);
	}

	/* VIRTUAL METHODS */ /* CAN TAKE ONLY ONE PARAMETER */
	protected virtual void OnTileGenerated(int tileIndex) {}
	protected virtual void OnWorldGenerated() {}
	protected virtual void BeforeGameQuit() {}
	protected virtual void OnLoadComplete() {}
	protected virtual void OnLoadingUpdate(int loadingProgress) {}
	/* VIRTUAL METHODS */

	public static void CallMessage(string _outerCall)
	{
		switch(_outerCall)
		{
			case "GAME_QUIT":
				worldSingleton.CallMessage(MessageType.GAME_QUIT);
				break;
			case "LOAD_COMPLETED":
				worldSingleton.CallMessage(MessageType.LOAD_COMPLETE);
				break;
			default:
				break;
		}
	}

	/* Call VIRTUAL METHOD on all inherited objects */
	protected void CallMessage(MessageType _id, string _parameter = "")
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
			case MessageType.GAME_QUIT:
				behaviour.BeforeGameQuit();
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
