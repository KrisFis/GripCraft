using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBehaviour : MonoBehaviour {

	public enum BlockType {GRASS, DIRT, SAND, ROCK}

	protected GameObject CreateBlock(BlockType _type, Vector3 _position, GameObject _parent, bool _isTerrain = true)
	{
        GameObject block = Instantiate(Resources.Load<GameObject>("Blocks/BlockPrefab"), _position, Quaternion.identity);

		int _tough = 0;

		int[] xy = new int[2];

		switch(_type)
		{
			case BlockType.GRASS:
				block.name = "Grass";
				_tough = 2;
				xy[0] = 1;
				xy[1] = 0;
				break;
			case BlockType.DIRT:
				block.name = "Dirt";
				_tough = 3;
				xy[0] = 0;
				xy[1] = 0;
				break;
			case BlockType.SAND:
				block.name = "Sand";
				_tough = 1;
				xy[0] = 1;
				xy[1] = 1;
				break;
			case BlockType.ROCK:
				block.name = "Rock";
				_tough = 7;
				xy[0] = 0;
				xy[1] = 1;
				break;
			default:
				Destroy(block);
				return null;
		}

		block.GetComponent<BlockManager>().SetBlock(_tough, _type, _isTerrain, _parent);
		SetBlockTexture(block, xy[0], xy[1]);
		block.transform.SetParent(_parent.transform);

		return block;
	}

	protected void CombineBlocks(GameObject _parentTile)
	{
		/* Když bude docházet k obrovským výpočetním operacím (velký svět - na tile je hodně kostek).
		Může na chvíli dojít k ohrožení hráče odtažených colliders.

		Je nejchytřejší a nejošetřenější vypnout gravitaci pro hráče!

		(ale jen když existuje, což není jen v případě volání této funkce z "BuildTilesRecursively" (<- WorldManager)
		- jinak Exception)
		 */

		Vector3 oldPos = _parentTile.transform.position;
		_parentTile.transform.position = Vector3.zero;
		_parentTile.SetActive(false);

		MeshFilter[] meshFilters = _parentTile.GetComponentsInChildren<MeshFilter>();
		CombineInstance[] combine = new CombineInstance[meshFilters.Length];

		MeshFilter filter = _parentTile.GetComponent<MeshFilter>();
		filter.mesh = new Mesh();

		int i = 0;
		while(i < meshFilters.Length)
		{
			combine[i].mesh = meshFilters[i].sharedMesh;
			combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
			i++;
		}

		filter.mesh = new Mesh();
		filter.mesh.CombineMeshes(combine);
		filter.mesh.RecalculateBounds();
		filter.mesh.RecalculateNormals();

		_parentTile.transform.position = oldPos;

		i = 0;
		while(i < meshFilters.Length)
		{
			if(meshFilters[i] != filter)
				meshFilters[i].GetComponent<BoxCollider>().enabled = true;
			
			i++;
		}

		_parentTile.SetActive(true);
	}

    private const float sheetSize = 2;
	private bool SetBlockTexture(GameObject _block, float _tileX, float _tileY)
	{
		float tilePerc = 1/sheetSize;
		
		float[,] uv = new float[2,2];

		uv[0,0] = tilePerc * _tileX; //u + min
		uv[0,1] = tilePerc * (_tileX+1); //u + max
		uv[1,0] = tilePerc * _tileY; //v + min
		uv[1,1] = tilePerc * (_tileY+1); //v + max

		/* [u/v,min/max] : [0/1,0/1] */
		Vector2[] blockUVs = new Vector2[24];

		blockUVs[0] = new Vector2(uv[0,0], uv[1,0]);
		blockUVs[1] = new Vector2(uv[0,1], uv[1,0]);
		blockUVs[2] = new Vector2(uv[0,0], uv[1,1]);
		blockUVs[3] = new Vector2(uv[0,1], uv[1,1]);
	
		blockUVs[4] = new Vector2(uv[0,0], uv[1,1]);
		blockUVs[5] = new Vector2(uv[0,1], uv[1,1]);
		blockUVs[6] = new Vector2(uv[0,0], uv[1,1]);
		blockUVs[7] = new Vector2(uv[0,1], uv[1,1]);
		
		blockUVs[8] = new Vector2(uv[0,0], uv[1,0]);
		blockUVs[9] = new Vector2(uv[0,1], uv[1,0]);
		blockUVs[10] = new Vector2(uv[0,0], uv[1,0]);
		blockUVs[11] = new Vector2(uv[0,1], uv[1,0]);

		blockUVs[12] = new Vector2(uv[0,0], uv[1,0]);
		blockUVs[13] = new Vector2(uv[0,0], uv[1,1]);
		blockUVs[14] = new Vector2(uv[0,1], uv[1,1]);
		blockUVs[15] = new Vector2(uv[0,1], uv[1,0]);
		
		blockUVs[16] = new Vector2(uv[0,0], uv[1,0]);
		blockUVs[17] = new Vector2(uv[0,0], uv[1,1]);
		blockUVs[18] = new Vector2(uv[0,1], uv[1,1]);
		blockUVs[19] = new Vector2(uv[0,1], uv[1,0]);
		
		blockUVs[20] = new Vector2(uv[0,0], uv[1,0]);
		blockUVs[21] = new Vector2(uv[0,0], uv[1,1]);
		blockUVs[22] = new Vector2(uv[0,1], uv[1,1]);
		blockUVs[23] = new Vector2(uv[0,1], uv[1,0]);
		
        _block.GetComponent<MeshFilter>().mesh.uv = blockUVs;
        return true;
	}
}
