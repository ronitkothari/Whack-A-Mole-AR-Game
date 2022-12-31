using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreByContact : MonoBehaviour {

    public int scoreValue;
    public string otherTag;
    private GameController gameController;

    void Start()
    {
        GameObject gameControllerObject = GameObject.FindWithTag("GameController");
        if (gameControllerObject != null)
        {
            gameController = gameControllerObject.GetComponent<GameController>();
        }
        if (gameControllerObject == null)
        {
            Debug.Log("Cannot Find 'GameController' Script");
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == otherTag)
        {
            gameController.AddScore(scoreValue);
            col.gameObject.tag = "DeadMole";
        }
    }
}
