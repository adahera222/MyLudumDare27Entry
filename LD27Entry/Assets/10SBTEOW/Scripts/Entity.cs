using UnityEngine;
using System.Collections;
using Holoville.HOTween;
using Holoville.HOTween.Plugins;

public class Entity : MonoBehaviour {
	
	public enum Type {
		NORMAL,
		BILLBOARD
	}
	
	public Type type = Type.NORMAL;
	private ArrayList positions;
	private ArrayList rotations;
	private int index = 0;
	private Sequence seq;
	private Tweener tweener;
	
	void Start() {
		positions = new ArrayList();
		rotations = new ArrayList();
	}
	
	public void SavePosition(){
		positions.Insert(index, transform.position);
		rotations.Insert(index, transform.rotation);
		if(index < 100)
			index++;
		else {
			Destroy(rigidbody);
			transform.position = (Vector3)positions[0];
			transform.rotation = (Quaternion)rotations[0];
		}
	}
	
	public void UpdatePosition(int index) {
		transform.position = (Vector3)positions[index];
		transform.rotation = (Quaternion)rotations[index];
	}
	
	public void Forward(float interval) {
		if(GameEngine.cur >= 100) return;

		if(tweener != null && !tweener.isComplete)
			tweener.Complete();
		tweener = HOTween.To (transform, interval, new TweenParms().Prop("position", positions[GameEngine.cur]).Prop("rotation", rotations[GameEngine.cur]).Ease(EaseType.Linear));
	}
	
	public void JumpToForward(int index, float interval) {
		if(GameEngine.cur > index) return;
		if(index > 100) return;
		
		transform.position = (Vector3)positions[GameEngine.cur];
		transform.rotation = (Quaternion)rotations[GameEngine.cur];
		seq = new Sequence();
		for(int i=GameEngine.cur+1; i<index+1; i++)
			seq.Append(HOTween.To (transform, interval / (index - GameEngine.cur), new TweenParms().Prop("position", positions[i]).Prop("rotation", rotations[i]).Ease(EaseType.Linear)));
		seq.Play();
		GameEngine.cur = index;
	}
	
	public void JumpToBackward(int index, float interval) {
		if(GameEngine.cur < index) return;
		if(index < 0) return;
		transform.position = (Vector3)positions[GameEngine.cur];
		transform.rotation = (Quaternion)rotations[GameEngine.cur];
		seq = new Sequence();
		for(int i=GameEngine.cur-1; i>index-1; i--) {
			seq.Append(HOTween.To (transform, interval / (GameEngine.cur - index), new TweenParms().Prop("position", positions[i]).Prop("rotation", rotations[i]).Ease(EaseType.Linear)));
		}
		seq.Play();
	}
	
	public IEnumerator EntityEvent() {
		switch(type) {
		case Type.NORMAL:
			rigidbody.useGravity = true;
			break;
		case Type.BILLBOARD:
			HOTween.To (transform, 3.0f, new TweenParms().Prop("localEulerAngles", new Vector3(0, 0, -40.4f)).Ease(EaseType.EaseInBounce));
			yield return new WaitForSeconds(3.0f);
			rigidbody.useGravity = true;
			rigidbody.velocity = new Vector3(0, -20.0f, 0);
			break;
		default:
			rigidbody.useGravity = true;
			break;
		}
		yield return null;
	}
}
