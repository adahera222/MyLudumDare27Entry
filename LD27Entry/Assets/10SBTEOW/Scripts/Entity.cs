using UnityEngine;
using System.Collections;
using Holoville.HOTween;
using Holoville.HOTween.Plugins;

public class Entity : MonoBehaviour {
	
	private ArrayList positions;
	private ArrayList rotations;
	private int index = 0;
	private Sequence seq;
	private int curr_index = 0;
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
	
	public void PositionsToString(){
		string result = "";
		foreach(Vector3 vec in positions) {
			result += vec.ToString() + ", ";
		}
		Debug.Log (result);
	}
	
	public void Backward(float interval) {
		if(curr_index <= 0) return;
		curr_index--;
		if(tweener != null && !tweener.isComplete)
			tweener.Complete();
		tweener = HOTween.To (transform, interval, new TweenParms().Prop("position", positions[curr_index]).Prop("rotation", rotations[curr_index]).Ease(EaseType.Linear));
	}
	
	public void Forward(float interval) {
		if(curr_index >= 100) return;
		curr_index++;
		if(tweener != null && !tweener.isComplete)
			tweener.Complete();
		tweener = HOTween.To (transform, interval, new TweenParms().Prop("position", positions[curr_index]).Prop("rotation", rotations[curr_index]).Ease(EaseType.Linear));
	}
	
	public void JumpToForward(int index, float interval) {
		if(curr_index > index) return;
		if(index > 100) return;
		
		transform.position = (Vector3)positions[curr_index];
		transform.rotation = (Quaternion)rotations[curr_index];
		seq = new Sequence();
		for(int i=curr_index+1; i<index+1; i++)
			seq.Append(HOTween.To (transform, interval / (index - curr_index), new TweenParms().Prop("position", positions[i]).Prop("rotation", rotations[i]).Ease(EaseType.Linear)));
		seq.Play();
		curr_index = index;
	}
	
	public void JumpToBackward(int index, float interval) {
		if(curr_index < index) return;
		if(index < 0) return;
		transform.position = (Vector3)positions[curr_index];
		transform.rotation = (Quaternion)rotations[curr_index];
		seq = new Sequence();
		for(int i=curr_index-1; i>index-1; i--) {
			seq.Append(HOTween.To (transform, interval / (curr_index - index), new TweenParms().Prop("position", positions[i]).Prop("rotation", rotations[i]).Ease(EaseType.Linear)));
		}
		seq.Play();
		curr_index = index;
	}
}
