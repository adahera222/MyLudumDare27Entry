using UnityEngine;
using System.Collections;
using Holoville.HOTween;
using Holoville.HOTween.Plugins;

public class GameEngine : MonoBehaviour {
	public tk2dTextMesh logoText;
	public tk2dTextMesh timerText;
	public tk2dTextMesh dialogue1;
	public tk2dTextMesh dialogue2;
	public tk2dSprite explosion;
	public float timerSpeed = 1.0f;
	public Zone[] zones;
	public string[] scripts;
	public PlayerScript player;
	public CameraScript gameCamera;
	public tk2dSprite title;
	public tk2dSpriteAnimator assistant;
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
	private bool isEnding = false;
	
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
	
	IEnumerator Pause() {
		isPaused = !isPaused;
		player.canControl = isPaused;
		player.isFrozen = !isPaused;
		StartCoroutine(gameCamera.Flash(isPaused ? new Color(0.75f, 1.0f, 0.5f, 0.5f) : new Color(0.3f, 0.75f, 1.0f, 0.5f)));
		yield return StartCoroutine(player.Freeze());
	}
	
	IEnumerator Pause(bool pauseit) {
		isPaused = pauseit;
		player.canControl = pauseit;
		player.isFrozen = !pauseit;
		yield return StartCoroutine(player.Freeze());
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
		StartCoroutine(InitZone(5));
		yield return StartCoroutine(gameCamera.Fade(Color.black, trans, 3.0f));
		//yield return StartCoroutine(PlayIntro());
		StartCoroutine(player.PlayTransform());
		Reset();
		yield return StartCoroutine(Pause(true));
		
		yield return new WaitForSeconds(1.0f);
		isStarted = true;
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
		Tweener twn;
		foreach(string s in scripts) {
			dialogue1.text = "";
			dialogue1.Commit();
			dialogue2.text = "";
			dialogue2.Commit();
			
			if(s.Substring(0, 1) == "0")
				twn = HOTween.To (dialogue1, s.Length * 0.05f, new TweenParms().Prop("text", s.Substring(1)).Ease(EaseType.Linear).OnUpdate(()=>{ dialogue1.Commit(); }));
			else if(s.Substring(0, 1) == "1") {
				assistant.Play("Assist2");
				player.animator.Sprite.FlipX = true;
				twn = HOTween.To (dialogue2, s.Length * 0.05f, new TweenParms().Prop("text", s.Substring(1)).Ease(EaseType.Linear).OnUpdate(()=>{ dialogue2.Commit(); }));
			}
			else if(s.Substring(0, 1) == "2") {
				assistant.Play ("Assist3");
				twn = HOTween.To (dialogue1, s.Length * 0.05f, new TweenParms().Prop("text", s.Substring(1)).Ease(EaseType.Linear).OnUpdate(()=>{ dialogue1.Commit(); }));
			} else {
				assistant.Play ("Assist4");
				twn = HOTween.To (dialogue2, s.Length * 0.05f, new TweenParms().Prop("text", s.Substring(1)).Ease(EaseType.Linear).OnUpdate(()=>{ dialogue2.Commit(); }));
			}
			
			while(!twn.isComplete) {
				yield return null;
			}
			while(true) {
				if(Input.GetButtonDown("Fire1")) break;
				yield return null;
			}
		}
		dialogue2.text = "";
		dialogue2.Commit();
		twn = HOTween.To (title.transform, 3.0f, new TweenParms().Prop("position", new Vector3(0, -30.0f, 0), true));
		while(!twn.isComplete) {
			yield return null;
		}
		while(true) {
			if(Input.GetButtonDown("Fire1")) break;
			yield return null;
		}
		twn = HOTween.To (title.transform, 3.0f, new TweenParms().Prop("position", new Vector3(0, -30.0f, 0), true));
		while(!twn.isComplete) {
			yield return null;
		}
		Destroy(title);
		
		string s2 = "Press Space to pause and unpause the time!";
		twn = HOTween.To (dialogue1, s2.Length * 0.05f, new TweenParms().Prop("text", s2).Ease(EaseType.Linear).OnUpdate(()=>{ dialogue1.Commit(); }));
		StartCoroutine(player.PlayTransform());
		Reset();
		while(!twn.isComplete) {
			yield return null;
		}
		while(true) {
			if(Input.GetButtonDown("Fire1")) break;
			yield return null;
		}
		dialogue1.text = "";
		dialogue1.Commit();
		s2 = "Oh, there is one side effect to this time bending.";
		twn = HOTween.To (dialogue1, s2.Length * 0.05f, new TweenParms().Prop("text", s2).Ease(EaseType.Linear).OnUpdate(()=>{ dialogue1.Commit(); }));
		while(!twn.isComplete) {
			yield return null;
		}
		while(true) {
			if(Input.GetButtonDown("Fire1")) break;
			yield return null;
		}
		dialogue1.text = "";
		dialogue1.Commit();
		s2 = "I can only move when the time is stopped.";
		twn = HOTween.To (dialogue1, s2.Length * 0.05f, new TweenParms().Prop("text", s2).Ease(EaseType.Linear).OnUpdate(()=>{ dialogue1.Commit(); }));
		while(!twn.isComplete) {
			yield return null;
		}
		while(true) {
			if(Input.GetButtonDown("Fire1")) break;
			yield return null;
		}

		dialogue1.text = "";
		dialogue1.Commit();
	}
	
	public IEnumerator PlayDeath(int index) {
		isKilled = true;
		player.isKilled = true;
		player.PlayDeath();
		gameCamera.isFollowing = false;
		yield return StartCoroutine(player.DeathAnimation());
		isPaused = true;
		yield return new WaitForSeconds(0.5f);
		yield return StartCoroutine(ReverseZone());
		yield return new WaitForSeconds(0.5f);
		gameCamera.isFollowing = true;
		player.Spawn(zones[cur_zone].spawnPoint.position);
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
		if(index >= zones.Length)
			yield break;
		if(!zones[index].isInitialized)
			yield return StartCoroutine(zones[index].InitZone());
	}
	
	void Update () {
		if(isEnding || !isStarted || isGameOver) {
			return;
		}
		
		if(Input.GetButtonDown("Fire1") && !isKilled) {
			StartCoroutine(Pause());
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
		isTransitioning = false;
		cur_zone = 5;
		while(!zones[cur_zone].isInitialized)
			yield return null;
		player.Spawn(zones[cur_zone].spawnPoint.position);
		zones[cur_zone].entryIndex = cur;
		zones[cur_zone].entryTime = timer;
		gameCamera.Warp();
		zones[cur_zone].UpdateIndex(cur);
		yield return StartCoroutine(gameCamera.Fade(Color.black, trans, 1.0f));
		StartCoroutine(InitZone(cur_zone + 2));
		if(cur_zone == 5)
		{
			isEnding = true;
			player.canControl = false;
			StartCoroutine(PlayEnding());
		}
	}
	
	IEnumerator PlayEnding() {
		yield return null;
	}
}
