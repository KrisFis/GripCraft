using UnityEngine;
using UnityEngine.UI;
using System;

using GripEngine;
using GripEngine.GameManagement;

public class PlayerManipulator : WorldBehaviour
{
    private Transform hitTile;
	private Transform uiItems;

	private Image currentImage;

    public BlockType equipType {get; private set;}

	private GameObject previewObj;
	private Vector3 lastPos = Vector3.zero;
	public Material close, normal;
	private bool isClose = true;

	private const float minDistance = 1.25f;

	private BlockManager block;
	private PlayerHUD hud;

	private int equipped;

	public static GameObject previewPrefab;

	void Awake()
	{
		previewPrefab = Resources.Load<GameObject>("Blocks/PreviewPrefab");
	}

	void Start()
	{
		RegisterCalls();

		previewObj = Instantiate(previewPrefab, lastPos, Quaternion.identity);
		previewObj.GetComponent<MeshRenderer>().material = normal;
		previewObj.SetActiveSafe(false);

		uiItems = GameObject.Find("Items").transform;
		hud = GameObject.Find("HUD").GetComponent<PlayerHUD>();

		equipped = 1;
		EquipBlock();
	}

    void Update()
    {
        BuildBlock();

        DestroyBlock();  

		ChangeEquipNumbers();
		ChangeEquipWheel();

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
			previewObj.SetActive(false);
		}
	}

	private void ChangeEquipWheel()
	{
		float wheel = Input.GetAxis("Mouse ScrollWheel");

		if(wheel == 0f)
			return;

		if(wheel < 0f)
		{
			if(++equipped > 4)
			{
				equipped = 1;
			}
		}
		else
		{
			if(--equipped < 1)
			{
				equipped = 4;
			}
		}

		EquipBlock();
	}

	private void ChangeEquipNumbers()
	{
		if(Input.GetKeyDown(KeyCode.Alpha1))
		{
			equipped = 1;
		}
		else if(Input.GetKeyDown(KeyCode.Alpha2))
		{
			equipped = 2;
		}
		else if(Input.GetKeyDown(KeyCode.Alpha3))
		{
			equipped = 3;
		}
		else if(Input.GetKeyDown(KeyCode.Alpha4))
		{
			equipped = 4;
		}
		else
			return;

		EquipBlock();
	}

	private void ChangeImage(Image _image)
	{
		if(currentImage != null)
			currentImage.color = Color.black;

		_image.color = Color.green;

		currentImage = _image;
	}

	private void EquipBlock()
	{
		if(uiItems.parent == null)
			return;
		
		switch(equipped)
		{
			case 1:
				equipType = BlockType.GRASS;
				ChangeImage(uiItems.GetChild(0).GetChild(0).GetComponent<Image>());
				break;
			case 2:
				equipType = BlockType.DIRT;
				ChangeImage(uiItems.GetChild(1).GetChild(0).GetComponent<Image>());
				break;
			case 3:
				equipType = BlockType.SAND;
				ChangeImage(uiItems.GetChild(2).GetChild(0).GetComponent<Image>());
				break;
			case 4:
				equipType = BlockType.ROCK;
				ChangeImage(uiItems.GetChild(3).GetChild(0).GetComponent<Image>());
				break;
			default:
				return;
		}

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
			hitTile = hit.transform.parent;

			if(hitTile == null)
				return;

			previewObj.SetActiveSafe(false);

			hud.StartLoading(hit.transform.GetComponent<BlockManager>().toughness * Settings.blockToughness);

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
			hitTile = hit.transform.parent;

			if(hitTile == null)
				return;

			Vector3 blockPos = hit.point + hit.normal/2.0f;

			blockPos.x = (float) Math.Round(blockPos.x, MidpointRounding.AwayFromZero);
			blockPos.y = (float) Math.Round(blockPos.y, MidpointRounding.AwayFromZero);
			blockPos.z = (float) Math.Round(blockPos.z, MidpointRounding.AwayFromZero);

			if(blockPos.y >= (Settings.heightWorldSize-1)) //Maximal world height reached
				return;

			CreateBlock(equipType, blockPos, hitTile.gameObject);

			CombineBlocks(hitTile.gameObject);

			Data.AddBlock(blockPos, equipType, hitTile.position);
		}
    }

}