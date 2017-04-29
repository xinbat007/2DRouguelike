﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;       //Allows us to use Lists. 
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public float levelStartDelay = 2f;
	public float turnDelay = .1f;
	public static GameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.
	private BoardManager boardScript;                       //Store a reference to our BoardManager which will set up the level.
	public int playerFoodPoints = 100;
	[HideInInspector] public bool playersTurn = true;

	private UnityEngine.UI.Text levelText;
	private GameObject levelImage;
	private int level = 1;                                  //Current level number, expressed in game as "Day 1".
	private List<Enemy> enemies;
	private bool enemiesMoving;
	private bool doingSetup;

	//Awake is always called before any Start functions
	void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);
		
		//Sets this to not be destroyed when reloading scene
		DontDestroyOnLoad(gameObject);
		enemies = new List<Enemy> ();
		boardScript = GetComponent<BoardManager>();
		InitGame();
	}

	//this is called only once, and the paramter tell it to be called only after the scene was loaded
	//(otherwise, our Scene Load callback would be called the very first load, and we don't want that)
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	static public void CallbackInitialization()
	{
		//register the callback to be called everytime the scene is loaded
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	//This is called each time a scene is loaded.
	static private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		instance.level++;
		instance.InitGame ();
	}
	
	//Initializes the game for each level.
	void InitGame()
	{
		doingSetup = true;
		levelImage = GameObject.Find ("LevelImage");
		levelText = GameObject.Find ("LevelText").GetComponent<Text>();
		levelText.text = "Day " + level;
		levelImage.SetActive (true);
		Invoke ("HideLevelImage", levelStartDelay);

		enemies.Clear ();
		boardScript.SetupScene(level);	
	}

	private void HideLevelImage()
	{
		levelImage.SetActive (false);
		doingSetup = false;
	}

	public void GameOver()
	{
		levelText.text = "After " + level + " days, you starved.";
		levelImage.SetActive (true);
		enabled = false;
	}	
	
	//Update is called every frame.
	void Update()
	{
		if (playersTurn || enemiesMoving || doingSetup)
			return;
		StartCoroutine(MoveEnemies ());
	}

	public void AddEnemyToList(Enemy script)
	{
		enemies.Add (script);
	}

	System.Collections.IEnumerator MoveEnemies()
	{
		enemiesMoving = true;
		yield return new WaitForSeconds (turnDelay);
		if (enemies.Count == 0) {
			yield return new WaitForSeconds (turnDelay);
		}
		for (int i = 0; i < enemies.Count; i++) {
			enemies [i].MoveEnemy ();
			yield return new WaitForSeconds (enemies [i].moveTime);
		}
		playersTurn = true;
		enemiesMoving = false;
	}
}