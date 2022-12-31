//-----------------------------------------------------------------------
// <copyright file="HelloARController.cs" company="Google">
//
// Copyright 2017 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCore.Examples.HelloAR
{
    using System.Collections;
    using System.Collections.Generic;
    using GoogleARCore;
    using GoogleARCore.Examples.Common;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

#if UNITY_EDITOR
    // Set up touch input propagation while using Instant Preview in the editor.
    using Input = InstantPreviewInput;
#endif

    /// <summary>
    /// Controls the HelloAR example.
    /// </summary>
    public class ArGameController : MonoBehaviour
    {
        public GameObject Mole;
        public GameObject pooler;
        public int numberOfMoles;
        public Vector3[] spawnValues = new Vector3[9];
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

        public Text DebugText;

        





        /// <summary>
        /// The first-person camera being used to render the passthrough camera image (i.e. AR background).
        /// </summary>
        public Camera FirstPersonCamera;

        /// <summary>
        /// A prefab for tracking and visualizing detected planes.
        /// </summary>
        public GameObject DetectedPlanePrefab;

        /// <summary>
        /// A model to place when a raycast from a user touch hits a plane.
        /// </summary>
        public GameObject GameField;

        

        /// <summary>
        /// A gameobject parenting UI for displaying the "searching for planes" snackbar.
        /// </summary>
        public GameObject SearchingForPlaneUI;

        /// <summary>
        /// The rotation in degrees need to apply to model when the Andy model is placed.
        /// </summary>
        private const float k_ModelRotation = 180.0f;

        /// <summary>
        /// A list to hold all planes ARCore is tracking in the current frame. This object is used across
        /// the application to avoid per-frame allocations.
        /// </summary>
        private List<DetectedPlane> m_AllPlanes = new List<DetectedPlane>();

        /// <summary>
        /// True if the app is in the process of quitting due to an ARCore connection error, otherwise false.
        /// </summary>
        private bool m_IsQuitting;

        private bool isFieldPlaced;
        private bool paused;
        private bool gameOver;
        private int score;

        void Start()
        {
            m_IsQuitting = false;
            isFieldPlaced = false;
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

        }


        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            //_UpdateApplicationLifecycle();

            if (!isFieldPlaced) { PlaceGameField(); }
            else
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

            
        }





        ///<summary>
        ///Places Game field
        /// </summary>
        private void PlaceGameField()
        {
            // Hide snackbar when currently tracking at least one plane.
            Session.GetTrackables<DetectedPlane>(m_AllPlanes);
            bool showSearchingUI = true;
            for (int i = 0; i < m_AllPlanes.Count; i++)
            {
                if (m_AllPlanes[i].TrackingState == TrackingState.Tracking)
                {
                    showSearchingUI = false;
                    break;
                }
            }

            SearchingForPlaneUI.SetActive(showSearchingUI);

            // If the player has not touched the screen, we are done with this update.
            Touch touch;
            if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            {
                return;
            }

            // Raycast against the location the player touched to search for planes.
            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                TrackableHitFlags.FeaturePointWithSurfaceNormal;

            if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
            {
                // Use hit pose and camera pose to check if hittest is from the
                // back of the plane, if it is, no need to create the anchor.
                if ((hit.Trackable is DetectedPlane) &&
                    Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position,
                        hit.Pose.rotation * Vector3.up) < 0)
                {
                    Debug.Log("Hit at back of the current DetectedPlane");
                }
                else
                {
                    // Choose the Andy model for the Trackable that got hit.
                    GameObject prefab = GameField;


                    // Instantiate Andy model at the hit pose.
                    var field = Instantiate(prefab, hit.Pose.position, hit.Pose.rotation);

                    // Compensate for the hitPose rotation facing away from the raycast (i.e. camera).
                    field.transform.Rotate(0, k_ModelRotation, 0, Space.Self);

                    // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
                    // world evolves.
                    var anchor = hit.Trackable.CreateAnchor(hit.Pose);

                    // Make Andy model a child of the anchor.
                    field.transform.parent = anchor.transform;
                    SetSpawnValues(field);
                    StartCoroutine(SpawnMoles());
                    isFieldPlaced = true;
                    Debug.Log("Field is Placed");
                }
            }
        }

        private void SetSpawnValues(GameObject Field)
        {
            spawnValues[0] = Field.transform.Find("Hole_1").position;
            spawnValues[1] = Field.transform.Find("Hole_2").position;
            spawnValues[2] = Field.transform.Find("Hole_3").position;
            spawnValues[3] = Field.transform.Find("Hole_4").position;
            spawnValues[4] = Field.transform.Find("Hole_5").position;
            spawnValues[5] = Field.transform.Find("Hole_6").position;
            spawnValues[6] = Field.transform.Find("Hole_7").position;
            spawnValues[7] = Field.transform.Find("Hole_8").position;
            spawnValues[8] = Field.transform.Find("Hole_9").position;

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
            Debug.Log("Is not running");
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
                //DebugText.text = "time scale is 0";
                Time.timeScale = 0;
            }
            else
            {
                //DebugText.text = "time scale is 1";
                Time.timeScale = 1;
            }

            //Debug.Log("paused is :" + isPaused);
            //DebugText.text = "Paused is :" + isPaused;


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


        /// <summary>
        /// Check and update the application lifecycle.
        /// </summary>
        private void _UpdateApplicationLifecycle()
        {
            // Pause the app when the 'back' button is pressed.
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

            // Only allow the screen to sleep when not tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                const int lostTrackingSleepTimeout = 15;
                Screen.sleepTimeout = lostTrackingSleepTimeout;
            }
            else
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            if (m_IsQuitting)
            {
                return;
            }

            // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
            if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
            {
                _ShowAndroidToastMessage("Camera permission is needed to run this application.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
            else if (Session.Status.IsError())
            {
                _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
        }

        /// <summary>
        /// Actually quit the application.
        /// </summary>
        private void _DoQuit()
        {
            Application.Quit();
        }

        /// <summary>
        /// Show an Android toast message.
        /// </summary>
        /// <param name="message">Message string to show in the toast.</param>
        private void _ShowAndroidToastMessage(string message)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                        message, 0);
                    toastObject.Call("show");
                }));
            }
        }
    }
}
