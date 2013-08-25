using UnityEngine;
using System.Collections;
using Holoville.HOTween;
using Holoville.HOTween.Plugins;

public class GameEngine : MonoBehaviour {
	public tk2dTextMesh logoText;
	public tk2dTextMesh timerText;
	public tk2dSprite explosion;
	public float timerSpeed = 1.0f;
	public Zone[] zones;
	public string[] scripts;
	public PlayerScript player;
	public CameraScript gameCamera;
	
	float timer = 10.0f;
	float last_timer = 10.1f;
	
	[@System.NonSerialized]
	public static int cur = 0;
	private int cur_zone = 0;
	
	public bool isStarted = false;
	private bool isPaused = false;
	private bool isGameOver = false;
	public bool isTransitioning = false;
	public bool isKilled = false;
	
	private bool startDebug = false;
	private static GameEngine instance;
	private Color trans = new Color(0.0f, 0.0f, 0.0f, 0.0f);
	public static GameEngine Instance
	{
		get {
			if(instance == null)
			{
				Debug.LogError("Error");
			}
			return instance;
		}
	}
	
	void Awake() {
		instance = this;
		explosion.color = trans;
		Physics.gravity = new Vector3(0.0f, -20.0f, 0.0f);
	}
	
	void Pause() {
		isPaused = !isPaused;
		player.canControl = isPaused;
		player.isFrozen = !isPaused;
	}
	
	void Pause(bool pauseit) {
		isPaused = pauseit;
		player.canControl = pauseit;
		player.isFrozen = !pauseit;
	}
	
	void Reset() {
		cur_zone = 0;
		cur = 0;
		timer = 10.0f;
		last_timer = 10.1f;
		timerText.text = timer.ToString("00.0") + "s";
		timerText.Commit();
		player.Reset();
	}
	
	IEnumerator Start () {
		yield return StartCoroutine(PlayLogo());
		StartCoroutine(InitZone(0));
		StartCoroutine(InitZone(1));
		StartCoroutine(InitZone(2));
		StartCoroutine(InitZone(3));
		// Debug
		
		startDebug = true;
		timerText.text = timer.ToString("00.0") + "s";
		timerText.Commit();
		
		yield return StartCoroutine(gameCamera.Fade(Color.black, trans, 3.0f));
		//yield return StartCoroutine(gameCamera.CutsceneOn());
		yield return new WaitForSeconds(5.0f);
		//yield return StartCoroutine(gameCamera.CutsceneOff());
		startDebug = false;
		isStarted = true;
		
		Reset();
	}
	
	IEnumerator PlayLogo() {
		yield return new WaitForSeconds(1.0f);
		logoText.text = "J_";
		logoText.Commit();
		yield return new WaitForSeconds(0.2f);
		logoText.text = "JW_";
		logoText.Commit();
		yield return new WaitForSeconds(1.0f);
		logoText.text = "J_";
		logoText.Commit();
		yield return new WaitForSeconds(0.1f);
		logoText.text = "_";
		logoText.Commit();
		yield return new WaitForSeconds(0.2f);
		Destroy(logoText);
		yield return null;
	}
	
	public IEnumerator PlayIntro() {
		yield return null;
	}
	
	public IEnumerator PlayDeath(int index) {
		isKilled = true;
		player.isKilled = true;
		gameCamera.isFollowing = false;
		yield return StartCoroutine(player.DeathAnimation());
		isPaused = true;
		yield return new WaitForSeconds(0.5f);
		yield return StartCoroutine(ReverseZone());
		gameCamera.isFollowing = true;
		player.Spawn(zones[cur_zone].spawnPoint.position);
		Pause(true);
		player.isKilled = false;
		isKilled = false;
		yield return null;
	}
	
	IEnumerator ReverseZone() {
		cur = zones[cur_zone].entryIndex;
		timer = zones[cur_zone].entryTime;
		last_timer = timer + 0.1f;
		timerText.text = timer.ToString("0.0") + "s";
		timerText.Commit();
		zones[cur_zone].UpdateIndex(cur);
		yield return null;
	}
	
	IEnumerator PlayGameOver() {
		explosion.color = new Color(1.0f, 0.64f, 0.50f, 0.9f);
		Tweener twn = HOTween.To (explosion, 0.4f, new TweenParms().Prop("scale", new Vector3(10.0f, 10.0f, 1.0f)));
		while(!twn.isComplete){
			yield return null;
		}
		yield return new WaitForSeconds(1.0f);
		yield return StartCoroutine(gameCamera.Fade(trans, Color.black, 1.0f));
		explosion.scale = new Vector3(0.0f, 0.0f, 1.0f);
		player.Spawn(zones[0].spawnPoint.position);
		Reset ();
		gameCamera.Warp();
		yield return StartCoroutine(gameCamera.Fade(Color.black, trans, 1.0f));
		isGameOver = false;
	}
	
	IEnumerator InitZone(int index){
		if(!zones[index].isInitialized)
			yield return StartCoroutine(zones[index].InitZone());
	}
	
	void Update () {
		if(startDebug)
			UpdateTimer();
		if(!isStarted || isGameOver) {
			return;
		}
		
		if(Input.GetButtonDown("Fire1") && !isKilled) {
			Pause();
		}
		
		if(!isPaused) {
			UpdateTimer();
			UpdateEntities();
		}
	}
	
	void UpdateTimer() {
		timer -= timerSpeed * Time.deltaTime;
		if(timer <= 0.0f)
		{
			timer = 0.0f;
			isGameOver = true;
			StartCoroutine(PlayGameOver());
		}
		timerText.text = timer.ToString("0.0") + "s";
		timerText.Commit();
		
	}
	
	void UpdateEntities() {
		if(last_timer - 0.1f >= timer && last_timer >= 0.0f) {
			last_timer -= 0.1f;
			zones[cur_zone].UpdateZone();
			cur++;
		}
	}
	
	public IEnumerator NextZone() {
		isTransitioning = true;
		player.Unspawn();
		yield return StartCoroutine(gameCamera.Fade(trans, Color.black, 1.0f));
		cur_zone = 3;
		player.Spawn(zones[cur_zone].spawnPoint.position);
		zones[cur_zone].entryIndex = cur;
		zones[cur_zone].entryTime = timer;
		gameCamera.Warp();
		zones[cur_zone].UpdateIndex(cur);
		yield return StartCoroutine(gameCamera.Fade(Color.black, trans, 1.0f));
		yield return StartCoroutine(InitZone(cur_zone + 1));
		isTransitioning = false;
	}
}
