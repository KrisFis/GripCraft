using UnityEngine;
using UnityEngine.UI;

using GripEngine;
using GripEngine.GameManagement;

public class PlayerHUD : WorldBehaviour
{
    public GameObject uiMenu, uiInventory;
    public RectTransform uiLoad;

    private float currentTime;
    private bool isEnabled;

    private float currentAdd;

    void Update()
    { 
        if(Input.GetKeyDown(KeyCode.Escape))
            ShowMenu();

        AddProgress();
    }

    public void StartLoading(float _time)
    {
        currentTime = _time;
        isEnabled = true;
        currentAdd = 1.0f/_time;

        playerSingleton.GetComponent<PlayerMovement>().enabled = false;

        uiLoad.localScale = new Vector3(1,0,1);

        uiLoad.parent.gameObject.SetActiveSafe(true);

        if(_time == -1)
        {
            isEnabled = false;
        }
    }

    public void StopLoading(bool _isInterrupted = true)
    {
        isEnabled = false;
        currentTime = 0;
        
        playerSingleton.GetComponent<PlayerMovement>().enabled = true;

        uiLoad.parent.gameObject.SetActiveSafe(false);
        
        if(!_isInterrupted)
            CallMessage(MessageType.LOAD_COMPLETE);
    }

    private void AddProgress()
    {
        if(isEnabled)
        {
            if(currentTime > 0)
            {
                currentTime -= Time.deltaTime;

                uiLoad.localScale += new Vector3(0, currentAdd * Time.deltaTime, 0);
            }
            else
            {
                StopLoading(false);
            }
        }
    }

    public void ShowMenu()
    {
        bool choose = (uiMenu.activeSelf) ? false : true;

        Core.SetCursorActiveSafe(choose);

        uiMenu.SetActiveSafe(choose);
        uiInventory.SetActiveSafe(!choose);

        playerSingleton.GetComponent<PlayerMovement>().enabled = !choose;
        playerSingleton.GetComponent<PlayerManipulator>().enabled = !choose;
    }

    public void Initiate()
    {
        RegisterCalls();
    }

    public void OnClickExitGame()
    {
        Game.QuitGame();
    }

    public void OnClickMainMenu()
    {
        Game.SaveGame(Data.worldName);
        Game.ChangeScene(0);
    }

    protected override void OnWorldGenerated()
    {
        this.gameObject.SetActiveSafe(true);
    }

}