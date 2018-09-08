using UnityEngine;
using UnityEngine.UI;

using GameManagement;

public class MenuScript : MonoBehaviour {

	public Text sliderTextValue, infoText;

	public Slider sliderValue;
	public InputField inputValue;

	private World worldToGenerate;

	private string seed = string.Empty;
	private int size = 0;

	void Start()
	{
		OnSliderValueChanged();
		Game.SetCursorActiveSafe(true);
	}

	public void OnClickQuit()
	{
		Game.QuitGame();
	}

	public void OnClickRandom()
	{
		SetRandomSeed(0);
		size = Random.Range(6, 13);

		if(size % 2 != 0)
			size++;

		SetInfoPanel();
	}

	public void OnClickOwn()
	{
		OnInputChanged();

		size = (int)sliderValue.value;
		SetInfoPanel();
	}

	public void OnSliderValueChanged()
	{
		int _value = (int)sliderValue.value;

		if(_value % 2 != 0)
		{
			sliderValue.value = ++_value;
			return;
		}

		sliderTextValue.text = "<b>" + (Mathf.Pow((_value+1),2) * World.tileSize) + " <color=green>B</color></b>";
	}

	public void OnInputChanged()
	{
		seed = inputValue.text;

		if(inputValue.text.Length != 6)
		{
			SetRandomSeed(inputValue.text.Length);

			inputValue.text = seed;

			return;	
		}
	}

	public void OnClickCancel()
	{
		seed = string.Empty;
		size = 0;

		inputValue.text = seed;
	}

	public void OnClickContinue()
	{
		LoadingScreen.LoadWorld(seed, size);
	}

	private void SetRandomSeed(int _startI)
	{
		for(int i = _startI; i < 6; i++)
		{
			seed += "" + Random.Range(0,10);
		}

		inputValue.text = seed;
	}

	private void SetInfoPanel()
	{
		infoText.text = "Seed světa: <b><color=green>" + seed + "</color></b>\nMnožství předrengerovaných bloků: <b><color=red>" + Mathf.Pow((size+1),2) * World.tileSize + "</color></b>\n\nChcete pokračovat?";
	}
}
