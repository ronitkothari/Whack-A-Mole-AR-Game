using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayPauseToggle : MonoBehaviour {

    public GameObject pause;
    public GameObject pauseScreen;
    public GameObject homeButton;

    
	// Use this for initialization
	public void Toggle(bool switcher)
    {
        //Debug.Log("Switcher is "+switcher);
        pause.SetActive(!switcher);
        pauseScreen.SetActive(switcher);
        homeButton.SetActive(switcher);
        
    }
    
}
