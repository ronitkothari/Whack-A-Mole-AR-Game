using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{

    public GameObject Mole;
    public GameObject pooler;
    public int numberOfMoles;
    public Vector3[] spawnValues;
    public float startWait;
    public float spawnWait;
    public float timeLeftOver;
    public float forceOfPop;
    public float speedOfPop;

    private GameObject[] moles = new GameObject[9];
    private ObjectPooler molePooler;
    private bool[] isActive = new bool[9];
    private Rigidbody rbMole;
    private Vector3 moleOffSet = new Vector3(0, 0.5f, 0);


    public Text gameOverText;
    public Text scoreText;
    public Text timeText;
    public GameObject playPauseButton;
    public GameObject restartButton;
    public GameObject homeBotton;

    private bool paused;
    private bool gameOver;
    private int score;




    // Use this for initialization
    void Start()
    {

        gameOver = false;
        Pause(false);
        gameOverText.text = "";
        restartButton.SetActive(false);
        homeBotton.SetActive(false);
        score = 0;
        UpdateScore();
        molePooler = pooler.GetComponent<ObjectPooler>();


        //set all holes as inactive on start
        for (int x = 0; x < 9; x++)
        {
            isActive[x] = false;
        }


        StartCoroutine(SpawnMoles());
    }

    // Update is called once per frame
    void Update()
    {


        if (timeLeftOver > 0)
        {
            UpdateTime();
            timeLeftOver -= Time.deltaTime;
        }
        else
        {
            GameOver();
        }

    }

    IEnumerator SpawnMoles()
    {
        yield return new WaitForSeconds(startWait);
        int lastHoleNum = -1;

        while (true)
        {
            //Debug.Log("Is Running");
            //choose a hole to pop mole out of
            int holeNum = Random.Range(0, 9);

            if (holeNum != lastHoleNum)
            {
                lastHoleNum = holeNum;
                //create a mole a record where mole is
                //moles[holeNum] = Instantiate(Mole, spawnValues[holeNum], Quaternion.identity);
                moles[holeNum] = molePooler.GetPooledObject();
                moles[holeNum].transform.position = spawnValues[holeNum] - moleOffSet;
                moles[holeNum].transform.rotation = Quaternion.identity;
                moles[holeNum].SetActive(true);
                isActive[holeNum] = true;


                //Vector3 cameraPositionSameY = firstPersonCamera.transform.position;
                //cameraPositionSameY.y = -0.5f;
                //moles[holeNum].transform.LookAt(cameraPositionSameY, moles[holeNum].transform.up);

                //create rigidbody
                rbMole = moles[holeNum].GetComponent<Rigidbody>();
                rbMole.velocity = Vector3.zero;
                rbMole.angularVelocity = Vector3.zero;
                //poping animation
                rbMole.detectCollisions = false;
                rbMole.AddForce(transform.up * forceOfPop);
                yield return new WaitForSeconds(speedOfPop);
                rbMole.detectCollisions = true;

                //wait before spawning next mole and removing mole
                yield return new WaitForSeconds(spawnWait);
                //Debug.Log(rbMole.position);
                //Debug.Log(spawnValues[holeNum]);
                float dis = Vector3.Distance(rbMole.position, spawnValues[holeNum]);
                if (dis < 0.1f)
                {
                    //Debug.Log("same position");
                    StartCoroutine(RemoveMole(holeNum));
                }

                isActive[holeNum] = false;
            }
            if (gameOver)
            {
                break;
            }
            
        }
        //Debug.Log("Is not running");


    }


    IEnumerator RemoveMole(int holeNum)
    {
        rbMole.detectCollisions = false;
        yield return new WaitForSeconds(speedOfPop * 3);
        //Destroy(moles[holeNum]);
        moles[holeNum].SetActive(false);
    }

    public void UpdateTime()
    {
        timeText.text = "" + (int)timeLeftOver;
    }

    public void AddScore(int newScoreValue)
    {
        score += newScoreValue;
        UpdateScore();
    }

    void UpdateScore()
    {
        scoreText.text = "Score: " + score;
    }

    public void Pause(bool isPaused)
    {

        if (isPaused)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }

        //Debug.Log("paused is :" + isPaused);


    }

    public void GameOver()
    {
        playPauseButton.SetActive(false);
        restartButton.SetActive(true);
        homeBotton.SetActive(true);
        gameOverText.text = "Game Over!";
        gameOver = true;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
