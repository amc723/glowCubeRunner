using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// abstract base class for map management contains all things any map manager would require
abstract public class AbstMapManager : MonoBehaviour
{
	protected AbstPlayerStatsManager playerStats;		// hang onto player controller script for distance travelled and player location
	protected Transform mainCamera;						// hang onto camera position so we know when to destroy parts of the level behind the player
	protected int currLevel = 0;						// current level, sent to map generators to determine generation params
	protected LinkedList<GameObject> mapObjects;		// used as a queue; needed so that map pieces can be destroyed after the camera's exited their view
	protected float lookAhead = 100f;					// how far to generate ahead of the player
	protected float lookBack = 10f;						// how far to wait until destroying objects behind the camera
	protected static readonly float mapPieceSize = BasicMapPiece.mapPieceSize;	// refers to x and z sizes
	protected AbstMapSegmentGenerator mapSegmentGenerator;						// class that actually handles generating sets of map pieces

	// keeps constantly moving forward from causing problems when the z value gets too large (really unlikely, but...)
	protected void PushAllObjectsBack(float distance)
	{
		foreach (Transform child in transform)
			child.position = new Vector3(child.position.x, child.position.y, child.position.z - distance);
		
		playerStats.PrevZPosition -= distance;
		mapSegmentGenerator.NextMapPieceLocZ -= distance;
	}

	// generate up to the lookahead using the currentLevel's map piece generator
	abstract protected void GenerateLevel();
	
	// destroy out of sight (behind the camera) map game objects
	abstract protected void Cleanup ();

	public int CurrLevel
	{
		get { return currLevel; }
	}

	// poor naming, "formatted" just means starting from index 1 instead of 0, for non-array based use
	public int FormattedCurrLevel
	{
		get { return currLevel + 1; }
	}
}

// map management refers to the higher level creation and destruction process for the map
public class MapManager : AbstMapManager
{
	// contains the distances travelled that the player levels up at, increasing map difficulty
	protected static readonly float[] levelChanges = new float[] {600f, 1400f, 2300f, -1f};

	// Use this for initialization
	void Start () 
	{
		playerStats = GameObject.FindGameObjectWithTag ("Player").GetComponent<AbstPlayerStatsManager> ();
		mainCamera = GameObject.FindGameObjectWithTag ("MainCamera").transform;
		mapSegmentGenerator = new MapSegmentGenerator();
		mapObjects = new LinkedList<GameObject> ();
		GenerateLevel ();
	}

	override protected void GenerateLevel()
	{
		// just to be extra safe, push all objects back every many thousand units
		if (mapSegmentGenerator.NextMapPieceLocZ > 50000f)
		{
			PushAllObjectsBack(50000f);
		}
		
		float playerLocZ = playerStats.transform.position.z;
		
		while (mapSegmentGenerator.NextMapPieceLocZ - playerLocZ < lookAhead)
		{
			// all map segment generators create safe pieces, followed by a dangerous piece, and then return a parent object containing all the pieces
			GameObject newMapSegment = mapSegmentGenerator.Generate(currLevel);
			newMapSegment.transform.SetParent(transform);
			
			// map segment was returned but the pieces should be deleted one at a time
			foreach (Transform newMapPiece in newMapSegment.transform)
			{
				mapObjects.AddLast(newMapPiece.gameObject);
			}

			// so that parent map segment can be deleted last later, catching any children that might have been missed somehow along the way
			mapObjects.AddLast(newMapSegment);
		}
	}

	override protected void Cleanup()
	{
		if (mapObjects.First == null)
			return;
		
		LinkedListNode<GameObject> curr = mapObjects.First;
		
		// objects are added to back of list, so objects created earlier and with lower z positions will be at front
		while (curr != null && curr.Value.transform.position.z < mainCamera.position.z - lookBack)
		{
			mapObjects.RemoveFirst();
			DestroyObject(curr.Value);
			curr = curr.Next;
		}
	}
	
	void Update () 
	{
		if (levelChanges[currLevel] != -1 && playerStats.DistanceTravelled > levelChanges [currLevel])
		{
			currLevel += 1;
		}
		
		GenerateLevel();
		Cleanup();
	}
}
