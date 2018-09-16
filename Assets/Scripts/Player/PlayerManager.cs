using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerManager : WorldBehaviour {

	private Vector3 lastPos;

	void Start()
	{
		lastPos = transform.position;
		InvokeRepeating("CheckTiles", 1f, 0.1f);
	}

	private void CheckTiles()
	{
		if(lastPos == transform.position)
			return;
		
		worldSingleton.RemoveTiles();
		worldSingleton.BuildTiles();

		lastPos = transform.position;
	}

	

}