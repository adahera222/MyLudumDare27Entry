using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween;
using Holoville.HOTween.Plugins;

public class Entity : MonoBehaviour {
	
	public enum Type {
		NORMAL,
		BILLBOARD,
		EXPLOSION,
		CAR
	}
	
	public Type type = Type.NORMAL;
	public float arg = 0.0f;
	public Vector3 arg2;
	private List<Vector3> positions;
	private List<Quaternion> rotations;
	private List<float> velocities;
	
	private int index = 0;
	private Sequence seq;
	private Tweener tweener;
	
	private List<int> frames;
	private GameObject sprite;
	private tk2dSpriteAnimator animator;
	private tk2dSprite sprite2;
	private List<Color> colors;
	
	void Start() {
		positions = new List<Vector3>();
		rotations = new List<Quaternion>();
		velocities = new List<float>();
		
		if(type == Type.EXPLOSION) {
			frames = new List<int>();
			sprite = transform.Find("explos").gameObject;
			animator = sprite.GetComponent<tk2dSpriteAnimator>();
			sprite.SetActive(false);
		}
		if(type == Type.CAR) {
			colors = new List<Color>();
			sprite2 = GetComponent<tk2dSprite>();
		}
	}
	
	public void SavePosition(){
		positions.Insert(index, transform.position);
		rotations.Insert(index, transform.rotation);
		
		if(rigidbody != null)
			velocities.Insert(index, rigidbody.velocity.magnitude);
		else {
			velocities.Insert(index, 0);
		}
		
		if(type == Type.EXPLOSION) {
			if(animator.IsPlaying("Explosion"))
				frames.Insert(index, animator.CurrentFrame);
			else
				frames.Insert(index, -1);
		}
		
		if(type == Type.CAR) {
			colors.Insert(index, sprite2.color);
		}
		
		if(index < 100)
			index++;
		else {
			Destroy(rigidbody);
			transform.position = positions[0];
			transform.rotation = rotations[0];
		}
	}
	
	public void UpdatePosition(int index) {
		transform.position = positions[index];
		transform.rotation = rotations[index];
		
		if(type == Type.EXPLOSION) {

			if(frames[GameEngine.cur] > -1 && !sprite.activeSelf)
				sprite.SetActive(true);
			else if(frames[GameEngine.cur] < 0 && sprite.activeSelf)
				sprite.SetActive(false);
			
			if(frames[GameEngine.cur] > -1)
				animator.SetFrame(frames[GameEngine.cur]);
		}
		
		if(type == Type.CAR) {
			sprite2.color = colors[GameEngine.cur];
		}
	}
	
	public void Forward(float interval) {
		if(GameEngine.cur >= 100) return;

		if(tweener != null && !tweener.isComplete)
			tweener.Complete();
		tweener = HOTween.To (transform, interval, new TweenParms().Prop("position", positions[GameEngine.cur]).Prop("rotation", rotations[GameEngine.cur]).Ease(EaseType.Linear));
		if(type == Type.EXPLOSION) {

			if(frames[GameEngine.cur] > -1 && !sprite.activeSelf)
				sprite.SetActive(true);
			else if(frames[GameEngine.cur] < 0 && sprite.activeSelf)
				sprite.SetActive(false);
			
			if(frames[GameEngine.cur] > -1)
				animator.SetFrame(frames[GameEngine.cur]);
		}
		if(type == Type.CAR) {
			sprite2.color = colors[GameEngine.cur];
		}
	}
	
	public void JumpToForward(int index, float interval) {
		if(GameEngine.cur > index) return;
		if(index > 100) return;
		
		transform.position = positions[GameEngine.cur];
		transform.rotation = rotations[GameEngine.cur];
		seq = new Sequence();
		for(int i=GameEngine.cur+1; i<index+1; i++)
			seq.Append(HOTween.To (transform, interval / (index - GameEngine.cur), new TweenParms().Prop("position", positions[i]).Prop("rotation", rotations[i]).Ease(EaseType.Linear)));
		seq.Play();
		GameEngine.cur = index;
	}
	
	public void JumpToBackward(int index, float interval) {
		if(GameEngine.cur < index) return;
		if(index < 0) return;
		transform.position = positions[GameEngine.cur];
		transform.rotation = rotations[GameEngine.cur];
		seq = new Sequence();
		for(int i=GameEngine.cur-1; i>index-1; i--) {
			seq.Append(HOTween.To (transform, interval / (GameEngine.cur - index), new TweenParms().Prop("position", positions[i]).Prop("rotation", rotations[i]).Ease(EaseType.Linear)));
		}
		seq.Play();
	}
	
	public IEnumerator EntityEvent() {
		switch(type) {
		case Type.NORMAL:
			yield return new WaitForSeconds(arg);
			rigidbody.useGravity = true;
			rigidbody.velocity = arg2;
			break;
		case Type.BILLBOARD:
			HOTween.To (transform, 2.0f, new TweenParms().Prop("localEulerAngles", new Vector3(0, 0, -40.4f)).Ease(EaseType.EaseInBounce));
			yield return new WaitForSeconds(2.0f);
			rigidbody.useGravity = true;
			rigidbody.velocity = new Vector3(0, -10.0f, 0);
			break;
		case Type.EXPLOSION:
			yield return new WaitForSeconds(arg);
			sprite.SetActive(true);
			animator.Play();
			while(animator.IsPlaying("Explosion")) {
				yield return null;
			}
			sprite.SetActive(false);
			break;
		case Type.CAR:
			yield return new WaitForSeconds(arg - 0.3f);
			HOTween.To (sprite2, 0.5f, new TweenParms().Prop ("color", Color.red).OnComplete(()=>{sprite2.color = Color.black;}));
			break;
		default:
			rigidbody.useGravity = true;
			break;
		}
		yield return null;
	}
	
	public float GetVelocity() {
		if(velocities.Count <= GameEngine.cur)
			return velocities[velocities.Count-1];
		return velocities[GameEngine.cur];
	}
}
