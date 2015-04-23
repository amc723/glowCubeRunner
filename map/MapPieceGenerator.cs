using UnityEngine;
using System.Collections;

// contains members and methods universal to all map pieces, dangerous or nondangerous
abstract public class BasicMapPiece : MonoBehaviour
{
	// these values are used a lot, so they're nice to have immediately available in memory
	public static readonly float mapPieceSize = 4f;
	public static readonly float halfMapPieceSize = mapPieceSize / 2f;

	protected Color lightColor;		// each map piece has a light color, should be set with this value on Start()

	public void SetLightColor(Color color)
	{
		lightColor = color;
		
		// this tends to get called before Start() and the light doesn't actually exist yet. But if it does, set its color
		Transform walls = transform.FindChild("BasicWalls");
		if (walls != null)
		{
			Transform light = walls.FindChild("PointLight");
			light.GetComponent<Light>().color = color;
		}
	}

	// every map piece has walls. Generate those.
	public void Start () 
	{
		GameObject walls = Resources.Load<GameObject>("prefabs/BasicWalls");
		walls = (GameObject) Instantiate((Object) walls);
		walls.transform.SetParent(transform);
		walls.transform.localPosition = new Vector3(0f, 0f, 0f);
		walls.transform.FindChild("PointLight").GetComponent<Light>().color = lightColor;
	}

	// since safe grounds are common, this is a nice thing to have other map pieces inherit
	public void GenerateSafeGround()
	{
		GameObject safeGround = Resources.Load<GameObject>("prefabs/BasicGround");
		safeGround = (GameObject)Instantiate((Object)safeGround);
		safeGround.transform.SetParent(transform);
		safeGround.transform.localPosition = new Vector3(0f, 0f, 0f);
	}
}

// contains the basic necessities for a single dangerous map piece
abstract public class DangerousMapPiece : BasicMapPiece
{
	protected Vector2 coinPoint;		// rows of coins should be placed away from danger. If this map piece is dangerous, specify where coins should be placed
	protected float dangerPoint = -1f;	// halfMapPieceSize * -1 < dangerPoint < halfMapPieceSize. Used to determine and store where a dangerous gameObject is located

	public bool IsDangerous()
	{
		return dangerPoint != -1;
	}

	public float GetDangerPoint()
	{
		return dangerPoint;
	}

	public Vector2 GetCoinPoint()
	{
		return coinPoint;
	}

	// allowance refers to how much to let the new danger points and coin points stray from previous dangerous map piece's points
	public abstract void Initialize(float previousDangerPoint, float allowance, int currLevel);
}

// safe map pieces contain nothing but full grounds and walls
public class SafeMapPiece : BasicMapPiece
{
	public void Initialize()
	{
		gameObject.name = "SafeMapPiece";
		GenerateSafeGround();
	}
}

// map piece with an obstacle block
public class ObstacleMapPiece : DangerousMapPiece
{
	// SetScaleParams sets scaling parameters for the wall
	protected delegate void SetScaleParams(ref float x, ref float y, ref float z);

	protected static SetScaleParams[] ScaleSetters;

	// TODO: load these by file rather than hard coding if you get the chance (low priority)
	protected static void SetScaleParamsLvl1(ref float x, ref float y, ref float z)
	{
		x = Random.Range(.5f, 1.5f);
		y = Random.Range(.2f, .8f);
		z = Random.Range(.5f, 3f);
	}

	protected static void SetScaleParamsLvl2(ref float x, ref float y, ref float z)
	{
		x = Random.Range(.5f, 2f);
		y = Random.Range(.2f, 1f);
		z = Random.Range(.5f, 3f);
	}

	protected static void SetScaleParamsLvl3(ref float x, ref float y, ref float z)
	{
		x = Random.Range(.5f, 2.5f);
		y = Random.Range(.2f, 1.1f);
		z = Random.Range(.5f, 3f);
	}

	protected static void SetScaleParamsLvl4(ref float x, ref float y, ref float z)
	{
		x = Random.Range(.5f, 3f);
		y = Random.Range(.2f, 1.2f);
		z = Random.Range(.5f, 3f);
	}

	void Awake()
	{
		ScaleSetters = new SetScaleParams[4]
		{
			SetScaleParamsLvl1,
			SetScaleParamsLvl2,
			SetScaleParamsLvl3,
			SetScaleParamsLvl4
		};
	}

	override public void Initialize(float previousDangerPoint, float allowance, int currLevel)
	{
		gameObject.name = "WalledMapPiece";
		GenerateSafeGround();

		GameObject killCube = Resources.Load<GameObject>("prefabs/KillCube");
		killCube = (GameObject)Instantiate((Object)killCube);
		killCube.transform.SetParent(transform);

		// set scale
		float scaleX = 0f, scaleY = 0f, scaleZ = 0f;
		ScaleSetters[currLevel](ref scaleX, ref scaleY, ref scaleZ);
		killCube.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
		
		// set position of wall and dangerPoint, TODO: confirm all this math is actually right, specifically ensuring x position stays in bounds of walls
		float localPosY, localPosZ;
		float halfWidth = (scaleX / 2f);
		float minXPos = Mathf.Max(-1f * halfMapPieceSize + halfWidth, previousDangerPoint - allowance);
		float maxXPos = Mathf.Min(previousDangerPoint + allowance, halfMapPieceSize - halfWidth);

		dangerPoint = Random.Range(minXPos, maxXPos);
		localPosY = .5f + (scaleY / 2f);
		localPosZ = Random.Range(0f - halfMapPieceSize, halfMapPieceSize);
		killCube.transform.localPosition = new Vector3(dangerPoint, localPosY, localPosZ);

		// for simplicity, set coinPoint at side with more available space
		float coinPointX, coinPointZ;
		coinPointZ = Random.Range(transform.position.z - halfMapPieceSize, transform.position.z + halfMapPieceSize);

		if (dangerPoint > 0f)
			coinPointX = Random.Range(transform.position.x - (halfMapPieceSize - .3f), dangerPoint - (scaleX + .05f));
		else
			coinPointX = Random.Range(dangerPoint + (scaleX + .05f), transform.position.x + (halfMapPieceSize - .3f));

		coinPoint = new Vector2(coinPointX, coinPointZ);
	}
}

public class FloorlessMapPiece : DangerousMapPiece
{
	protected delegate float XScaleGenerators();

	protected static XScaleGenerators[] GenerateGapScale;
	
	// TODO: load these by file rather than hard coding if you get the chance (low priority)
	protected static float GenScaleParamsLvl1()
	{
		return Random.Range(1f, 2f);
	}

	protected static float GenScaleParamsLvl2()
	{
		return Random.Range(1f, 2.5f);
	}

	protected static float GenScaleParamsLvl3()
	{
		return Random.Range(1f, 2.8f);
	}
	protected static float GenScaleParamsLvl4()
	{
		return Random.Range(1f, 3f);
	}

	
	void Awake()
	{
		GenerateGapScale = new XScaleGenerators[4]
		{
			GenScaleParamsLvl1,
			GenScaleParamsLvl2,
			GenScaleParamsLvl3,
			GenScaleParamsLvl4
		};
	}

	override public void Initialize(float previousDangerPoint, float allowance, int currLevel)
	{
		gameObject.name = "FloorlessMapPiece";
		float negHalfMapPieceSize = -1f * halfMapPieceSize;
		float gapWidth = GenerateGapScale [currLevel]();
		float dangerPointMin = Mathf.Max(negHalfMapPieceSize, previousDangerPoint - allowance);
		float dangerPointMax = Mathf.Min(previousDangerPoint + allowance, halfMapPieceSize);
		dangerPoint = Random.Range(dangerPointMin, dangerPointMax);

		// does gap exceed leftmost wall bounds
		if (dangerPoint - (gapWidth / 2f) <= negHalfMapPieceSize)
		{
			dangerPoint = negHalfMapPieceSize + (gapWidth / 2f);
			float floorScale = mapPieceSize - gapWidth;
			float floorXPos = halfMapPieceSize - (floorScale / 2f);

			// generate ground
			GameObject safeGround = Resources.Load<GameObject>("prefabs/BasicGround");
			safeGround = (GameObject)Instantiate((Object)safeGround);
			safeGround.transform.SetParent(transform);
			safeGround.transform.localScale = new Vector3(floorScale, 1f, mapPieceSize);
			safeGround.transform.localPosition = new Vector3(floorXPos, 0f, 0f);
		}

		// does gap exceed rightmost wall bounds
		else if (dangerPoint + (gapWidth / 2f) >= halfMapPieceSize)
		{
			dangerPoint = halfMapPieceSize - (gapWidth / 2f);
			float floorScale = mapPieceSize - gapWidth;
			float floorXPos = negHalfMapPieceSize + (floorScale / 2f);

			// generate ground
			GameObject safeGround = Resources.Load<GameObject>("prefabs/BasicGround");
			safeGround = (GameObject)Instantiate((Object)safeGround);
			safeGround.transform.SetParent(transform);
			safeGround.transform.localScale = new Vector3(floorScale, 1f, mapPieceSize);
			safeGround.transform.localPosition = new Vector3(floorXPos, 0f, 0f);
		}

		// gap does not exceed a wall so two floors are needed
		else
		{
			float floorLeftEdge = dangerPoint - (gapWidth / 2f);
			float floorRightEdge = dangerPoint + (gapWidth / 2f);

			// generate left and right side ground
			float floorLeftScale;
			if (dangerPoint - (gapWidth / 2f) > 0f)
				floorLeftScale = halfMapPieceSize - floorLeftEdge;
			else
				floorLeftScale = floorLeftEdge - negHalfMapPieceSize;

			float floorRightScale = halfMapPieceSize - floorRightEdge;		// no if/else needed here because this value is always positive

			float floorLeftXPos = negHalfMapPieceSize + floorLeftScale / 2f;
			float floorRightXPos = halfMapPieceSize - floorRightScale / 2f;

			GameObject leftGround = Resources.Load<GameObject>("prefabs/BasicGround");
			leftGround.name = "LeftGround";
			leftGround = (GameObject)Instantiate((Object)leftGround);
			leftGround.transform.SetParent(transform);
			leftGround.transform.localScale = new Vector3(floorLeftScale, 1f, mapPieceSize);
			leftGround.transform.localPosition = new Vector3(floorLeftXPos, 0f, 0f);

			GameObject rightGround = Resources.Load<GameObject>("prefabs/BasicGround");
			rightGround.name = "RightGround";
			rightGround = (GameObject)Instantiate((Object)rightGround);
			rightGround.transform.SetParent(transform);
			rightGround.transform.localScale = new Vector3(floorRightScale, 1f, mapPieceSize);
			rightGround.transform.localPosition = new Vector3(floorRightXPos, 0f, 0f);
		}

		// a drop plane is needed to unfreeze y positioning and allow the player to fall
		GameObject dropPlane = Resources.Load<GameObject>("prefabs/DropPlaneLarge");
		dropPlane = (GameObject)Instantiate((Object)dropPlane);
		dropPlane.transform.SetParent(transform);
		dropPlane.transform.localScale = new Vector3(gapWidth, 2f, mapPieceSize);
		dropPlane.transform.localPosition = new Vector3(dangerPoint, 1.5f, 0f);

		// generate respawn killplane
		GameObject killPlane = Resources.Load<GameObject>("prefabs/KillPlane");
		killPlane = (GameObject)Instantiate((Object)killPlane);
		killPlane.transform.SetParent(transform);
		killPlane.transform.localScale = new Vector3(gapWidth, .01f, mapPieceSize);
		killPlane.transform.localPosition = new Vector3(dangerPoint, .45f, 0f);

		// set coinPoint on side with more space
		float coinPointX;
		if (dangerPoint > 0f)
			coinPointX = Random.Range(transform.position.x - (halfMapPieceSize - .3f), dangerPoint - (gapWidth + .05f));
		else
			coinPointX = Random.Range(dangerPoint + (gapWidth + .05f), transform.position.x + (halfMapPieceSize - .3f));

		float coinPointZ = Random.Range(transform.position.z - halfMapPieceSize, transform.position.z + halfMapPieceSize);
		coinPoint = new Vector2(coinPointX, coinPointZ);
	}
}