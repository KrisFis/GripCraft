using UnityEngine;
using UnityEngine.UI;
using System;

using GameManagement;

public class PlayerManipulator : WorldBehaviour
{
    private Tile hitTile;
	private Transform uiItems;

	private Image currentImage;

    public BlockType equipType {get; private set;}

	private GameObject previewObj;
	private Vector3 lastPos = Vector3.zero;
	public Material close, normal;
	private bool isClose = true;

	private const float minDistance = 2f;

	private BlockManager block;
	private PlayerHUD hud;

	void Awake()
	{
		previewObj = Instantiate(Resources.Load<GameObject>("Blocks/PreviewPrefab"), lastPos, Quaternion.identity);
		previewObj.GetComponent<MeshRenderer>().material = normal;
		previewObj.SetActiveSafe(false);


		RegisterCalls();
	}

	void Start()
	{
		uiItems = GameObject.Find("Items").transform;

		hud = GameObject.Find("HUD").GetComponent<PlayerHUD>();

		EquipBlock(BlockType.GRASS);
	}

    void Update()
    {
        BuildBlock();

        DestroyBlock();  

		ChangeEquip();

		ShowPreview();
    }

	protected override void OnLoadComplete()
	{
		if(block != null)
		{
			block.DestroyBlock();
			block = null;
		}
	}

	private void ShowPreview()
	{
		RaycastHit hit;
			
		if(ShotRay(out hit))
		{
			Vector3 blockPos = hit.point + hit.normal/2.0f;

			blockPos.x = (float) Math.Round(blockPos.x, MidpointRounding.AwayFromZero);
			blockPos.y = (float) Math.Round(blockPos.y, MidpointRounding.AwayFromZero);
			blockPos.z = (float) Math.Round(blockPos.z, MidpointRounding.AwayFromZero);

			if(Vector3.Distance(this.transform.position, blockPos) < minDistance)
			{
				previewObj.GetComponent<MeshRenderer>().material = close;
				isClose = true;
			}
			else
			{
				previewObj.GetComponent<MeshRenderer>().material = normal;
				isClose = false;
			}

			if(lastPos == blockPos)
				return;

			lastPos = blockPos;

			previewObj.transform.position = blockPos;
			previewObj.SetActiveSafe(true);
		}
		else if(previewObj.activeSelf)
		{
			lastPos = Vector3.zero;
			isClose = true;
			previewObj.SetActiveSafe(false);
		}
	}

	private void ChangeEquip()
	{
		if(uiItems.parent == null)
			return;

		if(Input.GetKeyDown(KeyCode.Alpha1))
		{
			EquipBlock(BlockType.GRASS);
		}
		else if(Input.GetKeyDown(KeyCode.Alpha2))
		{
			EquipBlock(BlockType.DIRT);
		}
		else if(Input.GetKeyDown(KeyCode.Alpha3))
		{
			EquipBlock(BlockType.SAND);
		}
		else if(Input.GetKeyDown(KeyCode.Alpha4))
		{
			EquipBlock(BlockType.ROCK);
		}
	}

	private void ChangeImage(Image _image)
	{
		if(currentImage != null)
			currentImage.color = Color.black;

		_image.color = Color.green;

		currentImage = _image;
	}

	private void EquipBlock(BlockType _type)
	{
		switch(_type)
		{
			case BlockType.GRASS:
				ChangeImage(uiItems.GetChild(0).GetChild(0).GetComponent<Image>());
				break;
			case BlockType.DIRT:
				ChangeImage(uiItems.GetChild(1).GetChild(0).GetComponent<Image>());
				break;
			case BlockType.SAND:
				ChangeImage(uiItems.GetChild(2).GetChild(0).GetComponent<Image>());
				break;
			case BlockType.ROCK:
				ChangeImage(uiItems.GetChild(3).GetChild(0).GetComponent<Image>());
				break;
			default:
				return;
		}
		
		equipType = _type;

	}

	private bool ShotRay(out RaycastHit hit)
	{
		Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
		return Physics.Raycast(ray, out hit, Settings.blockPlaceDistance ,~LayerMask.NameToLayer("Blocks"));
	}

	private void DestroyBlock()
	{
        if(Input.GetMouseButtonUp(1))
		{
			block = null;
			hud.StopLoading();
			return;
		}

		if(!Input.GetMouseButtonDown(1))
			return;

		RaycastHit hit;

		if(ShotRay(out hit))
		{
			hitTile = hit.transform.parent.position.GetTile();

			if(hitTile == null)
				return;

			hud.StartLoading(hit.transform.GetComponent<BlockManager>().blockToughness * Settings.blockToughness);

			block = hit.transform.GetComponent<BlockManager>();
		}
	}

    private void BuildBlock()
    {
        if(!Input.GetMouseButtonDown(0) || isClose)
            return;

		RaycastHit hit;
			
		if(ShotRay(out hit))
		{
			hitTile = hit.transform.parent.position.GetTile();

			if(hitTile == null)
				return;

			Vector3 blockPos = hit.point + hit.normal/2.0f;

			blockPos.x = (float) Math.Round(blockPos.x, MidpointRounding.AwayFromZero);
			blockPos.y = (float) Math.Round(blockPos.y, MidpointRounding.AwayFromZero);
			blockPos.z = (float) Math.Round(blockPos.z, MidpointRounding.AwayFromZero);

			if(blockPos.y >= (Settings.heightWorldSize-1)) //Maximal world height reached
				return;

			CreateBlock(equipType, blockPos, hitTile.gameObject, false);

			CombineBlocks(hitTile.gameObject);
		}
    }

}