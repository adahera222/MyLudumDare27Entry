using UnityEngine;
using System.Collections;

public class GameEngine : MonoBehaviour {
	public tk2dTextMesh timerText;
	public float timerSpeed = 1.0f;
	public Entity[] entities;
	
	float timer = 10.0f;
	float last_timer = 10.1f;
	
	private bool isInitialized = false;
	
	private int cur_index = 0;
	private bool isPaused = false;
	
	IEnumerator Start () {
		timerText.text = timer.ToString("00.0") + "s";
		timerText.Commit();
		
		yield return StartCoroutine(InitializeEntities2());
		isInitialized = true;
		cur_index = 0;
		isPaused = false;
		timer = 10.0f;
		last_timer = 10.1f;
	}
	
	IEnumerator InitializeEntities2() {
		while(timer >= 0.0f) {
			if(last_timer - 0.1f >= timer && last_timer >= 0.0f) {
				last_timer -= 0.1f;
				foreach(Entity e in entities){
					e.SavePosition();
				}
			}
			timer -= timerSpeed * Time.deltaTime;
			yield return null;
		}
		foreach(Entity e in entities){
			e.SavePosition();
		}
	}
	
	void Update () {
		if(!isInitialized) return;
		
		if(!isPaused) {
			UpdateTimer();
		}
		//UpdateTimer();
	}
	
	void UpdateTimer() {
		timer -= timerSpeed * Time.deltaTime;
		timerText.text = timer.ToString("0.0") + "s";
		timerText.Commit();
	}
	
	void UpdateEntities() {
		
	}
}
