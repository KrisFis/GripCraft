using UnityEngine;
using UnityEngine.UI;

using GameManagement;

public class LoadingScreen : WorldBehaviour
{
    public static World activeWorld {get; private set;}

	public static void LoadWorld(string _seed, int _worldSize)
	{
		activeWorld = new World(_seed,_worldSize);
        Game.ChangeScene(1);
        Game.SetCursorActiveSafe(false);
	}

    public GameObject uiHUD, uiScreen;
    public RectTransform uiBar;
    public Text progressPerc;
    public PlayerHUD uiHud;

    void Start()
    {
        RegisterCalls();

        uiHud.Initiate();
        
        progressPerc.text = "0%";

        worldSingleton.BuildWorld(activeWorld);
    }

	public void SpawnPlayer(Vector3 playerPosition)
	{
		//Spawn Main Tile
        int size = 2;

        for(int x = -size; x <= size; x++)
		{
			for(int z = -size; z <= size; z++)
			{
				Tiler.GetTile(new Vector3(x * World.tileSize, 0, z * World.tileSize)).gameObject.SetActiveSafe(true);
			}
		}

		GameObject player = Resources.Load<GameObject>("Player/Prefab");
		player = Instantiate(player, playerPosition, Quaternion.identity);
		player.name = "Player";

		//!IMPORTANT
		playerSingleton = player.GetComponent<PlayerManager>();
		//!IMPORTANT
	}

    protected override void OnLoadingUpdate(int loadingProgress)
    {
        
        if(loadingProgress >= 100)
        {
            CallMessage(MessageType.WORLD_GENERATED);
            return;
        }

        uiBar.localScale = new Vector3(loadingProgress/100.0f, uiBar.localScale.y, uiBar.localScale.z);
        progressPerc.text = loadingProgress + "%";


    }

    protected override void OnWorldGenerated()
    {
        SpawnPlayer(new Vector3(16,World.tileHeightScale + 5,0)); // y + 5 = proti bugum, x = 16 -> kvůli pivotu, do centra 32/2
        
        Destroy(uiScreen.gameObject);

        Destroy(this.gameObject);
    }

}