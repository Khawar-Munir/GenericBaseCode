using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadNextScene : MonoBehaviour {
	public Slider slider;
	// Use this for initialization
	void Start () {
		StartCoroutine (loadLevelCoroutine ());
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	public IEnumerator loadLevelCoroutine()
	{
		while(slider.value<1)
		yield return new WaitForEndOfFrame();
		SceneManager.LoadScene("MainMenu");
	}
}
