using UnityEngine;
using System.Collections;

// contains the things considered necessary for any class that populates the map with coins
abstract public class AbstCoinPopulator : System.Object
{
	// generalized map segment delegate + delegate array for quick array accesses instead of case statements
	protected delegate GameObject GenerateCoins(DangerousMapPiece dangerPiece1, DangerousMapPiece dangerPiece2, float generationSeed);
	protected static GenerateCoins[] coinGenerationFunctions;

	abstract public GameObject Generate(DangerousMapPiece dangerPiece1, DangerousMapPiece dangerPiece2, float generationSeed, int currLevel);
}

// CoinPopulator works through a map segment generator to populate the map with coins
public class CoinPopulator : AbstCoinPopulator
{
	AbstMapSegmentGenerator mapSegmentGenerator;

	public CoinPopulator(AbstMapSegmentGenerator segmentGenerator)
	{
		mapSegmentGenerator = segmentGenerator;
		coinGenerationFunctions = new GenerateCoins[4]		
		{
			GenerateLevel1Coins,
			GenerateLevel2Coins,
			GenerateLevel3Coins,
			GenerateLevel4Coins
		};
	}

	override public GameObject Generate(DangerousMapPiece dangerPiece1, DangerousMapPiece dangerPiece2, float generationSeed, int currLevel)
	{
		return coinGenerationFunctions [currLevel] (dangerPiece1, dangerPiece2, generationSeed);
	}

	// create a single coin at coordinates (xPos, 1f, zPos) with a parent
	protected GameObject CreateCoinAtPoint(float xPos, float zPos, Transform parent)
	{
		GameObject coin = Resources.Load<GameObject>("prefabs/Coin");
		coin = (GameObject)GameObject.Instantiate((Object)coin);
		coin.transform.position = new Vector3(xPos, .8f, zPos);			// all coins sit at y == .8f
		coin.transform.SetParent(parent);
		return coin;
	}
	
	// create a single coin at coordinates (xPos, 1f, zPos), does not set parent
	protected GameObject CreateCoinAtPoint(float xPos, float zPos)
	{
		GameObject coin = Resources.Load<GameObject>("prefabs/Coin");
		coin = (GameObject)GameObject.Instantiate((Object)coin);
		
		// single coin still needs a parent for deletion purposes later; if it's collected and an attempt to destroy again is made, bad things happen
		GameObject singleCoin = new GameObject();
		singleCoin.name = "SingleCoin";
		singleCoin.transform.position = new Vector3(xPos, .8f, zPos);			// all coins sit at y == .8f
		coin.transform.SetParent(singleCoin.transform);
		coin.transform.localPosition = new Vector3(0f, 0f, 0f);
		return singleCoin;
	}
	
	protected GameObject CreateCoinLine(DangerousMapPiece dangerPiece1, DangerousMapPiece dangerPiece2, int numCoins)
	{
		GameObject coinLine = new GameObject();
		
		coinLine.name = "CoinLine";
		
		// x, z vector to create a path of coins along
		Vector2 point1 = dangerPiece1.GetCoinPoint();
		Vector2 point2 = dangerPiece2.GetCoinPoint();
		float deltaX = (point2.x - point1.x) / numCoins;
		float deltaZ = (point2.y - point1.y) / numCoins;
		
		for (int i = 0; i < numCoins; i++)
		{
			GameObject coin = CreateCoinAtPoint(point1.x + deltaX * i, point1.y + deltaZ * i, coinLine.transform);
			coin.transform.Rotate(Vector3.forward * i * 20);
		}
		
		return coinLine;
	}

	protected GameObject GenerateLevel1Coins(DangerousMapPiece dangerPiece1, DangerousMapPiece dangerPiece2, float generationSeed)
	{
		if (generationSeed < .8f)
		{
			if (generationSeed < .3f)
				return null;
			
			// single coin at dangerPiece1's location, .3 < generationSeed < .8
			else
			{
				Vector2 coinPoint = dangerPiece1.GetCoinPoint();
				return CreateCoinAtPoint(coinPoint.x, coinPoint.y);
			}
		}
		
		else
			return CreateCoinLine(dangerPiece1, dangerPiece2, Random.Range(3, 8));
	}
	
	protected GameObject GenerateLevel2Coins(DangerousMapPiece dangerPiece1, DangerousMapPiece dangerPiece2, float generationSeed)
	{
		if (generationSeed < .7f)
		{
			if (generationSeed < .25f)
				return null;
			
			// single coin at dangerPiece1's location, .25 < generationSeed < .7
			else
			{
				Vector2 coinPoint = dangerPiece1.GetCoinPoint();
				return CreateCoinAtPoint(coinPoint.x, coinPoint.y);
			}
		}
		
		else
			return CreateCoinLine(dangerPiece1, dangerPiece2, Random.Range(4, 8));
	}
	
	protected GameObject GenerateLevel3Coins(DangerousMapPiece dangerPiece1, DangerousMapPiece dangerPiece2, float generationSeed)
	{
		if (generationSeed < .6f)
		{
			if (generationSeed < .2f)
				return null;
			
			// single coin at dangerPiece1's location, .2 < generationSeed < .6
			else
			{
				Vector2 coinPoint = dangerPiece1.GetCoinPoint();
				return CreateCoinAtPoint(coinPoint.x, coinPoint.y);
			}
		}
		
		else
			return CreateCoinLine(dangerPiece1, dangerPiece2, Random.Range(5, 8));
	}
	
	protected GameObject GenerateLevel4Coins(DangerousMapPiece dangerPiece1, DangerousMapPiece dangerPiece2, float generationSeed)
	{
		if (generationSeed < .4f)
		{
			Vector2 coinPoint = dangerPiece1.GetCoinPoint();
			return CreateCoinAtPoint(coinPoint.x, coinPoint.y);
		}
		
		else
			return CreateCoinLine(dangerPiece1, dangerPiece2, Random.Range(5, 8));
	}
}
