using UnityEngine;
using UnityEngine.UI;

using GripEngine.GameManagement;
using GripEngine;

public class LoadingScreen : WorldBehaviour
{
    public GameObject uiHUD, uiScreen;
    public RectTransform uiBar;
    public Text progressPerc;
    public PlayerHUD uiHud;
    
    void Start()
    {
        uiHud.Initiate();
        
        progressPerc.text = "0%";

        RegisterCalls();

        Core.SetVSyncActiveSafe(false);

        worldSingleton.BuildGame();
    }

	public void SpawnPlayer()
	{
        if(Data.playerPos == Vector3.zero)
        {
            RaycastHit hit;
            if(Physics.Raycast(Vector3.zero, -transform.up, out hit, Mathf.Infinity, ~LayerMask.NameToLayer("Blocks")))
            {
                Data.playerPos = hit.transform.position + (transform.up * 2);
            }
            else if(Physics.Raycast(Vector3.zero, transform.up, out hit, Mathf.Infinity, ~LayerMask.NameToLayer("Blocks")))
            {
                Data.playerPos = hit.transform.position + (transform.up * 4);
            }
            else
            {
                Debug.LogError("WORLD NOT GENERATED!!");
                return;
            }
        }

		GameObject player = Resources.Load<GameObject>("Player/Prefab");
		player = Instantiate(player, Data.playerPos + transform.up, Quaternion.identity);
		player.name = "Player";

		//!IMPORTANT
		playerSingleton = player.GetComponent<PlayerManager>();
		//!IMPORTANT
	}

    protected override void OnLoadingUpdate(int loadingProgress)
    {
        
        if(loadingProgress >= 100)
        {
            if(playerSingleton == null)
                CallMessage(MessageType.WORLD_GENERATED);
            return;
        }

        uiBar.localScale = new Vector3(loadingProgress/100.0f, uiBar.localScale.y, uiBar.localScale.z);
        progressPerc.text = loadingProgress + "%";
    }

    protected override void OnWorldGenerated()
    {
        SpawnPlayer(); // y + 5 = proti bugum, x = 16 -> kvůli pivotu, do centra 32/2
        
        Game.SaveGame(Data.worldName);
        Core.SetVSyncActiveSafe(true);

        Destroy(uiScreen.gameObject);

        Destroy(this.gameObject);
    }

}