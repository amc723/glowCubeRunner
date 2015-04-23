using UnityEngine;
using System.Collections;

// contains things considered necessary for any map segment generator
abstract public class AbstMapSegmentGenerator : System.Object
{
	protected delegate GameObject GenerateMapSegment(float generationSeed);					// generalized map segment delegate so that arrays can be used for generation functions instead of case statements
	protected static GenerateMapSegment[] segmentGenerationFunctions;						// see above
	protected Vector3 nextMapPieceLoc;														// where to place the next segment of the map in space
	public DangerousMapPiece lastDangerousMapPiece = null;									// used to determine next dangerous piece's dangerPoint and for coin generation
	protected static readonly float mapPieceSize = BasicMapPiece.mapPieceSize;				// refers to x and z sizes
	protected static readonly float halfMapPieceSize = BasicMapPiece.halfMapPieceSize;		// just so it doesn't have to get calculated all the time, it's used a lot

	abstract public GameObject Generate(int currLevel);
	abstract protected GameObject InitMapSegment();

	public float NextMapPieceLocZ
	{
		get { return nextMapPieceLoc.z; }
		set { nextMapPieceLoc.z = value; }
	}
}

// contains methods for generating map segments and generating coins along segments
public class MapSegmentGenerator : AbstMapSegmentGenerator
{
	protected Color prevLightColor;
	protected AbstCoinPopulator coinPopulator;

	public MapSegmentGenerator()
	{
		coinPopulator = new CoinPopulator (this);

		segmentGenerationFunctions = new GenerateMapSegment[4]
		{
			GenerateLevel1MapSegment,
			GenerateLevel2MapSegment,
			GenerateLevel3MapSegment,
			GenerateLevel4MapSegment
		};

		nextMapPieceLoc = new Vector3 (0f, 0f, 0f);
		prevLightColor = new Color(Random.Range(.5f, 1f), Random.Range(.5f, 1f), Random.Range(.5f, 1f));
	}

	override public GameObject Generate(int currLevel)
	{
		return segmentGenerationFunctions [currLevel] (Random.Range (0f, 1f));
	}

	// create a map segment and initialize its position properly
	override protected GameObject InitMapSegment()
	{
		GameObject mapSegment = new GameObject();
		mapSegment.name = "MapSegment";
		mapSegment.transform.position = nextMapPieceLoc;
		return mapSegment;
	}

	// picks a random danger component to add to the passed in mapPiece
	protected DangerousMapPiece AddDangerousComponent(float generationSeed, GameObject mapPiece)
	{
		if (generationSeed < .5f)
			return mapPiece.AddComponent<ObstacleMapPiece>();
		else
			return mapPiece.AddComponent<FloorlessMapPiece>();
	}

	// create a map piece which should be given a MapPiece component after returned
	protected GameObject CreateUnspecifiedMapPiece(Transform parent, int segmentPieceNo)
	{
		GameObject mapPiece = new GameObject();
		mapPiece.transform.SetParent(parent);
		mapPiece.transform.localPosition = new Vector3(0, 0, segmentPieceNo * mapPieceSize);
		nextMapPieceLoc.z += mapPieceSize;
		return mapPiece;
	}

	// general function for generating map segments that takes parameters from the by-level map segment generators
	protected GameObject GenerateMapSegment(int safePieces, float prevDangerPoint, float allowance, float generationSeed, int currLevel, ref DangerousMapPiece dangerComponent)
	{
		GameObject mapSegment = InitMapSegment();
		
		for (int i = 0; i < safePieces; i++)
		{
			GameObject safeMapPiece = CreateUnspecifiedMapPiece(mapSegment.transform, i);
			SafeMapPiece mapPieceComponent = safeMapPiece.AddComponent<SafeMapPiece>();
			mapPieceComponent.SetLightColor(GenerateLightColor(generationSeed));
			mapPieceComponent.Initialize();
		}
		
		GameObject dangerousMapPiece = CreateUnspecifiedMapPiece(mapSegment.transform, safePieces);
		dangerComponent = AddDangerousComponent(generationSeed, dangerousMapPiece);
		dangerComponent.Initialize(prevDangerPoint, allowance, currLevel);
		dangerComponent.SetLightColor(GenerateLightColor(generationSeed));

		return mapSegment;
	}

	// includes map initialization things since the game starts at level 1
	// TODO: separate out a helper function for the redundant parts in all the GenerateLevel*MapSegment functions
	protected GameObject GenerateLevel1MapSegment(float generationSeed)
	{
		// map is being initialized for the first time if no dangerous map pieces were ever present
		if (lastDangerousMapPiece == null)
		{
			float prevDangerPoint = 0f;
			float allowance = halfMapPieceSize;
			int safePieces = 5;
			DangerousMapPiece dangerComponent = null;
			GameObject mapSegment = GenerateMapSegment(safePieces, prevDangerPoint, allowance, generationSeed, 1, ref dangerComponent);
			lastDangerousMapPiece = dangerComponent;
			return mapSegment;
		}
		
		// else is almost identical, with the exception of creating coins as well
		else
		{
			float prevDangerPoint = lastDangerousMapPiece.GetDangerPoint();
			float allowance = mapPieceSize * .4f;
			int safePieces = Random.Range(2, 6);
			DangerousMapPiece dangerComponent = null;
			GameObject mapSegment = GenerateMapSegment(safePieces, prevDangerPoint, allowance, generationSeed, 0, ref dangerComponent);
			GameObject setOfCoins = coinPopulator.Generate(lastDangerousMapPiece, dangerComponent, generationSeed, 0);
			
			if (setOfCoins != null)
				setOfCoins.transform.SetParent(mapSegment.transform);
			
			lastDangerousMapPiece = dangerComponent;
			return mapSegment;
		}
	}
	
	protected GameObject GenerateLevel2MapSegment(float generationSeed)
	{
		float prevDangerPoint = lastDangerousMapPiece.GetDangerPoint();
		float allowance = mapPieceSize * .6f;
		int safePieces = Random.Range(2, 5);
		DangerousMapPiece dangerComponent = null;
		GameObject mapSegment = GenerateMapSegment(safePieces, prevDangerPoint, allowance, generationSeed, 1, ref dangerComponent);
		GameObject setOfCoins = coinPopulator.Generate(lastDangerousMapPiece, dangerComponent, generationSeed, 1);
		
		if (setOfCoins != null)
			setOfCoins.transform.SetParent(mapSegment.transform);
		
		lastDangerousMapPiece = dangerComponent;
		return mapSegment;
	}
	
	protected GameObject GenerateLevel3MapSegment(float generationSeed)
	{
		float prevDangerPoint = lastDangerousMapPiece.GetDangerPoint();
		float allowance = mapPieceSize * .8f;
		int safePieces = Random.Range(2, 4);
		DangerousMapPiece dangerComponent = null;
		GameObject mapSegment = GenerateMapSegment(safePieces, prevDangerPoint, allowance, generationSeed, 2, ref dangerComponent);
		GameObject setOfCoins = coinPopulator.Generate(lastDangerousMapPiece, dangerComponent, generationSeed, 2);
		
		if (setOfCoins != null)
			setOfCoins.transform.SetParent(mapSegment.transform);
		
		lastDangerousMapPiece = dangerComponent;
		return mapSegment;
	}
	
	protected GameObject GenerateLevel4MapSegment(float generationSeed)
	{
		float prevDangerPoint = lastDangerousMapPiece.GetDangerPoint();
		float allowance = mapPieceSize;
		int safePieces = 2;
		DangerousMapPiece dangerComponent = null;
		GameObject mapSegment = GenerateMapSegment(safePieces, prevDangerPoint, allowance, generationSeed, 3, ref dangerComponent);
		GameObject setOfCoins = coinPopulator.Generate(lastDangerousMapPiece, dangerComponent, generationSeed, 3);
		
		if (setOfCoins != null)
			setOfCoins.transform.SetParent(mapSegment.transform);
		
		lastDangerousMapPiece = dangerComponent;
		return mapSegment;
	}

	protected Color GenerateLightColor(float generationSeed)
	{
		Color color = new Color(prevLightColor.r, prevLightColor.g, prevLightColor.b);
		float change = Random.Range(.1f, .15f);
		
		if (generationSeed < 1f / 3f)
		{
			float rMin = Mathf.Max(prevLightColor.r - change, .3f);
			float rMax = Mathf.Min(prevLightColor.r + change, 1f);
			color.r = Random.Range(rMin, rMax);
		}
		
		else if (generationSeed < 2f / 3f)
		{
			float gMin = Mathf.Max(prevLightColor.g - change, .3f);
			float gMax = Mathf.Min(prevLightColor.g + change, 1f);
			color.g = Random.Range(gMin, gMax);
		}
		
		else
		{
			float bMin = Mathf.Max(prevLightColor.b - change, .3f);
			float bMax = Mathf.Min(prevLightColor.b + change, 1f);
			color.b = Random.Range(bMin, bMax);
		}
		
		prevLightColor = color;
		return color;
	}
}