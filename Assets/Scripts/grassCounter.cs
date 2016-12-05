using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class grassCounter : MonoBehaviour {

	public Text countText;
	private float count;

	void Start ()
	{
		count = 0f;
		countText.text = "Count: 0";
	}

	void Update ()
	{
		//Debug.Log (baseFairway);
		//Debug.Log (baseRough);
		count += Time.deltaTime;
		int countInt = (int)count;
		countText.text = "Count:" + countInt;




	}


}