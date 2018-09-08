using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerManager : WorldBehaviour {
	
	void Start()
	{
		StartCoroutine(CoroutineUpdate());
	}

	private IEnumerator CoroutineUpdate()
	{
		while(this.enabled)
		{
			yield return new WaitForSeconds(1f);
			worldSingleton.RegenerateTiles();
		}
	}

	

}