using UnityEngine;
using System.Collections;

public class Zone : MonoBehaviour {
	
	[@System.NonSerialized]
	public Entity[] entities;
	[@System.NonSerialized]
	public bool isInitialized = false;
	[@System.NonSerialized]
	public Transform spawnPoint;
	[@System.NonSerialized]
	public int entryIndex = 0;
	[@System.NonSerialized]
	public float entryTime = 0.0f;
	void Start () {
		isInitialized = false;
		entities = GetComponentsInChildren<Entity>();
		spawnPoint = transform.FindChild("Spawn");
	}
	
	public IEnumerator InitZone() {
		if(isInitialized) yield break;
		
		if(entities.Length == 0)
		{
			isInitialized = true;
			yield break;
		}
		
		foreach(Entity e in entities) {
			StartCoroutine(e.EntityEvent());			
		}
		
		float timer = 10.0f;
		float last_timer = 10.1f;
		while(timer >= 0.0f) {
			if(last_timer - 0.1f >= timer && last_timer >= 0.0f) {
				last_timer -= 0.1f;
				foreach(Entity e in entities){
					e.SavePosition();
				}
			}
			timer -= Time.deltaTime;
			yield return null;
		}
		foreach(Entity e in entities){
			e.SavePosition();
		}
		isInitialized = true;
	}
	
	public void UpdateZone () {
		foreach(Entity e in entities) {
			e.Forward(0.1f);
		}
	}
	
	public void UpdateIndex(int index) {
		foreach(Entity e in entities) {
			e.UpdatePosition(index);
		}
	}
	
	public IEnumerator JumpBackward(int index) {
		foreach(Entity e in entities) {
			e.JumpToBackward(index, 1.0f);
		}
		yield return new WaitForSeconds(1.0f);
	}
}
