using UnityEngine;

using GripEngine;
using GripEngine.GameManagement;

public class BlockManager : BlockBehaviour
{
    public int toughness {get; private set;}
    public BlockType type {get; private set;}

    public void Init(int _tough, BlockType _type)
    {
        toughness = _tough;
        type = _type;

        if(this.transform.position.y <= -(Settings.heightWorldSize-1))
        {
            toughness = -1; //Unbreakable -> Maximum world height reached
        }
    }

    public void DestroyBlock()
    {
        Destroy(this.GetComponent<MeshFilter>());

        Data.RemoveBlock(this.transform.position, this.transform.parent.position);
    
        Invoke("CombineInvoke", 0.1f);
    }

    private void CombineInvoke()
    {
        CombineBlocks(this.transform.parent.gameObject);

        Destroy(this.gameObject);
    }

}