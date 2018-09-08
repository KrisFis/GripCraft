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
        /*
        RaycastHit[] hits;
        if(ShotRayAll(this.transform.position, this.transform.up, out hits))
        {
            if(hits.Length > 1)
                return;

            CreateTerrainBlock(this.transform.position, parentTile);
        }
        */

        CheckDown();
        CheckDirectionRay(this.transform.up);
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
            if(!ShotRayDown(this.transform.position + _direction, out hit))
            {
                if(!ShotRayUp(this.transform.position + _direction, out hit)) //To get RAY
                    return; //JUST IN CASE - ERROR

                Vector3 newPos = this.transform.position + _direction;
                int newParent = hit.transform.GetComponent<BlockManager>().parentTile;

                if(hit.distance > 0.75f)
                {
                    CheckUp(newPos + this.transform.up, newParent);
                }

                CreateTerrainBlock(newPos, newParent);
            }
        }
    }

    private void CheckUp(Vector3 _position, int _parent)
    {
        int maxY = (int)(Mathf.PerlinNoise((_position.x + World.seed[0]) / World.tileDetailScale,
				(_position.z + World.seed[1]) / World.tileDetailScale) * World.tileHeightScale);

        if((maxY - _position.y) < 0)
            return;

        CreateTerrainBlock(_position, _parent);
    }

    private void CheckDown()
    {
        RaycastHit hit;

        if(!ShotRayDown(this.transform.position, out hit))
        {
                Vector3 newPos = this.transform.position - this.transform.up;
                CreateTerrainBlock(newPos, parentTile);
        }
    }

    private bool ShotRay(Vector3 origin, Vector3 direction, out RaycastHit hit)
	{
        if(Physics.Raycast(new Ray(origin,direction), out hit, 1.0f , ~LayerMask.NameToLayer("Blocks")))
            return hit.transform.GetComponent<BlockManager>().IsTerrain;

        return false;
	}
    
    private bool ShotRayUp(Vector3 origin, out RaycastHit hit)
	{
		if(Physics.Raycast(new Ray(origin, this.transform.up), out hit, Settings.heightWorldSize + origin.y, ~LayerMask.NameToLayer("Blocks")))
            return hit.transform.GetComponent<BlockManager>().IsTerrain;

        return false;
    }

    private bool ShotRayDown(Vector3 origin, out RaycastHit hit)
    {
        if(Physics.Raycast(new Ray(origin, -this.transform.up), out hit, Settings.heightWorldSize - origin.y, ~LayerMask.NameToLayer("Blocks")))
            return hit.transform.GetComponent<BlockManager>().IsTerrain;

        return false;
    }

    private bool ShotRayAll(Vector3 origin, Vector3 direction, out RaycastHit[] hits)
    {
        hits = Physics.RaycastAll(new Ray(origin, direction), Settings.heightWorldSize + origin.y, ~LayerMask.NameToLayer("Blocks"));

        List<RaycastHit> newHits = new List<RaycastHit>();

        foreach(RaycastHit hit in hits)
        {
            if(hit.transform.GetComponent<BlockManager>().IsTerrain)
            {
                newHits.Add(hit);
            }
        }

        hits = newHits.ToArray();

        if(hits.Length == 0)
            return false;

        return true;
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