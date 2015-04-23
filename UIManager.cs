using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// lumps HUD display and game over screen together since those are the only UI in the game right now. 
// TODO: would be better separated out more between HUD and game over screen, the abstraction isn't really providing any potential benefits right now...
abstract public class AbstUIManager : MonoBehaviour
{
	protected AbstPlayerStatsManager playerInfo;		// so distanceTravelled and coinCount can be retrieved
	protected AbstMapManager mapGenerator;				// so that current level can be retrieved
	protected Text distanceTravelledText;				// both Texts are for quick text setting in the HUD
	protected Text coinsCollectedText;
	protected int currLevel;							// so that the UIManager knows when the level's changed from playerInfo and can display the change
	protected const float totalLevelUpDisplayTime = 3f;	// how long to display level up message for
	protected float currLevelUpDisplayTime = 0f;		// how long level up display has been on screen
	protected bool displayingLevelUp = false;			// is the UIManager currently displaying level up?
}

public class UIManager : AbstUIManager
{
	protected GameObject gameEndedPanel;
	protected GameObject levelUpPanel;

	// Use this for initialization
	void Start () 
	{
		playerInfo = GameObject.FindGameObjectWithTag("Player").GetComponent<AbstPlayerStatsManager>();
		mapGenerator = GameObject.FindGameObjectWithTag("MapGenerator").GetComponent<AbstMapManager>();
		currLevel = mapGenerator.FormattedCurrLevel;

		// grab child text components
		foreach (Transform child in transform)
		{
			if (child.gameObject.name == "CoinsCollectedText")
			{
				coinsCollectedText = child.gameObject.GetComponent<Text>();
			}

			else if (child.gameObject.name == "DistanceTravelledText")
			{
				distanceTravelledText = child.gameObject.GetComponent<Text>();
			}
		}

		// hide game over UI and level up UI
		gameEndedPanel = transform.parent.FindChild("GameEndedPanel").gameObject;
		levelUpPanel = transform.parent.FindChild("LevelUpPanel").gameObject;
		gameEndedPanel.SetActive(false);
		levelUpPanel.SetActive(false);
	}

	void OnGUI()
	{
		if (playerInfo.GameOver)
		{
			gameEndedPanel.SetActive(true);
			gameEndedPanel.transform.FindChild("CoinsCollectedText").GetComponent<Text>().text = playerInfo.CoinCount.ToString();
			gameEndedPanel.transform.FindChild("DistanceTravelledText").GetComponent<Text>().text = playerInfo.DistanceTravelled.ToString();
		}

		int level = mapGenerator.FormattedCurrLevel;

		if (level > currLevel)
		{
			currLevel = level;
			displayingLevelUp = true;
			currLevelUpDisplayTime = 0f;
			levelUpPanel.SetActive(true);
			levelUpPanel.GetComponentInChildren<Text>().text = "Level " + currLevel + "!";
		}

		else if (currLevelUpDisplayTime > totalLevelUpDisplayTime)
		{
			displayingLevelUp = false;
			levelUpPanel.SetActive(false);
		}

		coinsCollectedText.text = playerInfo.CoinCount.ToString();

		int distanceTravelled = (int)playerInfo.DistanceTravelled;
		distanceTravelledText.text = distanceTravelled.ToString();
	}

	void Update()
	{
		if (displayingLevelUp)
			currLevelUpDisplayTime += Time.deltaTime;
	}

	public void ResetGameButton()
	{
		Application.LoadLevel(0);
	}
}
