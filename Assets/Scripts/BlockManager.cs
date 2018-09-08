using System.Collections.Generic;
using UnityEngine;

using GameManagement;

public class BlockManager : BlockBehaviour
{
    public int blockToughness {get; private set;}
    public BlockType blockType {get; private set;}
    public bool IsTerrain {get; private set;}

    public int parentTile {get; private set;}
    private List<int> parentTiles;

    public void SetBlock(int _tough, BlockType _type, bool _isTerrain, GameObject _parentTile)
    {
        blockToughness = _tough;
        blockType = _type;
        IsTerrain = _isTerrain;
        parentTile = Tiler.GetTile(_parentTile).index;

        parentTiles = new List<int>();

        if(this.transform.position.y <= -(Settings.heightWorldSize-1))
        {
            blockToughness = -1; //Unbreakable -> Maximum world height reached
        }
    }

    public void DestroyBlock()
    {
        AddToList(parentTile);

        if(IsTerrain)
        {
            ReCreateAround();
        }

        Destroy(this.GetComponent<MeshFilter>());
    
        Invoke("CombineInvoke", 0.1f);
    }

    private void CombineInvoke()
    {
        foreach(int tileIndex in parentTiles.ToArray())
        {
            CombineBlocks(Tiler.GetTile(tileIndex).gameObject);
        }

        Destroy(this.gameObject);
    }

    private void ReCreateAround()
    {
        if(!ShotRay(this.transform.position, -this.transform.up))
        {
                Vector3 newPos = this.transform.position - this.transform.up;
                CreateTerrainBlock(newPos, parentTile);
        }

        CheckDirectionRay(this.transform.forward);
        CheckDirectionRay(-this.transform.forward);
        CheckDirectionRay(this.transform.right);
        CheckDirectionRay(-this.transform.right);
    }

    private void AddToList(int _tileIndex)
    {
        foreach(int tileIndex in parentTiles)
        {
            if(tileIndex == _tileIndex)
                return;
        }

        parentTiles.Add(_tileIndex);
    }

    private void CheckDirectionRay(Vector3 _direction)
    {
        RaycastHit hit;

        if(!ShotRay(this.transform.position, _direction, out hit))
        {
            if(ShotRay(this.transform.position + _direction, this.transform.up, out hit) && hit.transform.GetComponent<BlockManager>().IsTerrain)
            {
                if(ShotRay(this.transform.position + _direction, -this.transform.up))
                    return;
                
                Vector3 newPos = this.transform.position + _direction;
                CreateTerrainBlock(newPos, hit.transform.GetComponent<BlockManager>().parentTile);
            }
        }
    }

    private bool ShotRay(Vector3 origin, Vector3 direction, out RaycastHit hit)
	{
		return Physics.Raycast(new Ray(origin,direction), out hit, 1.0f ,~LayerMask.NameToLayer("Blocks"));
	}
    
    private bool ShotRay(Vector3 origin, Vector3 direction)
	{
        RaycastHit hit;
		return Physics.Raycast(new Ray(origin,direction), out hit, Settings.heightWorldSize - Mathf.Abs(origin.y) ,~LayerMask.NameToLayer("Blocks"));
	}

    private GameObject CreateTerrainBlock(Vector3 _position, int _parentIndex)
    {
        GameObject newBlock;

        GameObject _parent = Tiler.GetTile(_parentIndex).gameObject;

        if(_position.y > 15)
		{
			newBlock = CreateBlock(BlockType.GRASS, _position, _parent);
		}
		else if(_position.y > 10)
		{
			newBlock = CreateBlock(BlockType.DIRT, _position, _parent);
		}
		else if(_position.y > 5)
		{
			newBlock = CreateBlock(BlockType.SAND, _position, _parent);
		}
		else
		{
			newBlock = CreateBlock(BlockType.ROCK, _position, _parent);
		}

        AddToList(_parentIndex);

        return newBlock;
    }

}