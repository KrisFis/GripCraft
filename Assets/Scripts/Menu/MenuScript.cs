using UnityEngine;
using UnityEngine.UI;

using GripEngine;
using GripEngine.GameManagement;

public class MenuScript : MonoBehaviour {

	public InputField seedInput, nameInput;
	public GameObject savesObj;
	public Text errorText;

	private string seed = string.Empty;
	private string worldName = string.Empty;

	private static GameObject savePrefab;
	private int numberOfSaves;

	void Awake()
	{
		savePrefab = Resources.Load<GameObject>("UI/SavePrefab");
	}

	void Start()
	{
		Core.SetCursorActiveSafe(true);
		Data.ReStart();

		PrepareLoadButtons();
	}

	private void ShowError(string _text, float _time)
	{
		errorText.text = _text;
		errorText.gameObject.SetActiveSafe(true);

		Invoke("HideError", _time);
	}
	
	private void HideError()
	{
		errorText.gameObject.SetActiveSafe(false);
	}

	private void PrepareLoadButtons()
	{
		foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Save"))
		{
			Destroy(obj);
		}

		string[] worldNames;
		if(!Game.GetSavesDirectoryNames(out worldNames))
			return;

		numberOfSaves = worldNames.Length;

		for(int i = 0; i < numberOfSaves; i++)
		{
			string wName = worldNames[i];
			GameObject saveObj = Instantiate(savePrefab, new Vector3((i * 202.5f) - 405f , 0 , 0 ) , Quaternion.identity);
			saveObj.transform.SetParent(savesObj.transform, false);
			saveObj.name = wName;

			saveObj.GetComponent<Button>().onClick.AddListener(delegate {OnClickLoad(wName);});
			saveObj.transform.GetChild(0).GetComponent<Text>().text = worldNames[i];

			saveObj.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate {OnClickDelete(wName);});
		}
	}

	public void OnClickDelete(string _worldName)
	{
		Game.DeleteGame(_worldName);
		PrepareLoadButtons();
	}

	public void OnClickQuit()
	{
		Game.QuitGame();
	}

	public void OnClickLoad(string _worldName)
	{
		int errorMsg; // For diag
		if(!Game.LoadGame(_worldName, out errorMsg))
			return;

		Game.ChangeScene(1);
		Core.SetCursorActiveSafe(false);
	}

	public void OnClickCreate()
	{
		if(numberOfSaves >= 5)
		{
			ShowError("<b>Byl dosažen limit uložených světů.</b>", 5f);
		}

		if(worldName == "" || worldName == string.Empty)
		{
			ShowError("<b>Zadejte název světa!</b>", 3f);
			return;
		}

		foreach(GameObject save in GameObject.FindGameObjectsWithTag("Save"))
		{
			if(save.name == worldName)
			{
				ShowError("<b>Zadejte platný název světa!</b>", 3f);
				return;
			}
		}

		OnSeedChanged();

		Data.worldName = worldName;
		Data.worldSeed = seed;
		
        Game.ChangeScene(1);
        Core.SetCursorActiveSafe(false);
	}

	public void OnNameChanged()
	{
		worldName = nameInput.text;
	}

	public void OnSeedChanged()
	{
		seed = seedInput.text;

		if(seedInput.text.Length != 6)
		{
			SetRandomSeed(seedInput.text.Length);
			seedInput.text = seed;
		}
	}

	private void SetRandomSeed(int _startI)
	{
		for(int i = _startI; i < 6; i++)
		{
			seed += "" + Random.Range(0,10);
		}

		seedInput.text = seed;
	}
}
