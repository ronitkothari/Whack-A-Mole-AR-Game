using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Remove later
using UnityEngine.UI;

public class ThrowWeapon : MonoBehaviour
{

    public Camera playerCam;
    public GameObject weapon;
    public GameObject playerWeapon;
    public GameObject pooler;
    public Text DebugText; //remove later
    public float fireRate;
    public float speed;
    // float angle;
    public Vector3 offSet;

    private bool throwWeapon;
    private float nextFire;
    private Vector3 weaponTarget;
    private Vector3 initialPos;
    private Rigidbody hammer_rb;
    private Rigidbody player_rb;
    private ObjectPooler hammerPooler;
    
    
    // Use this for initialization
    void Start()
    {
        throwWeapon = false;
        hammerPooler = pooler.GetComponent<ObjectPooler>();
        player_rb = gameObject.GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount != 1 || Time.time < nextFire )
        {
            return;
        }
        nextFire = Time.time + fireRate;
        Debug.Log("Touch detected");
        //DebugText.text = "Touch detected";//remove later

        var ray = playerCam.ScreenPointToRay(Input.touches[0].position);
        Touch touch = Input.GetTouch(0);
        RaycastHit hitInfo;

        switch (touch.phase)
        {
            case TouchPhase.Began:
               
                if (Physics.Raycast(ray, out hitInfo))
                {
                    /*if (hitInfo.transform.name != "GameField" || !hitInfo.transform.name.Contains("Mole"))
                        return;       */             
                    weaponTarget = hitInfo.point;
                    
                    throwWeapon = true;
                }
                break;

            case TouchPhase.Ended:
                break;
        }

    }

    void FixedUpdate()
    {
        if (throwWeapon)
        {
            GetWeapon();
            hammer_rb.constraints = RigidbodyConstraints.None;
            if (weaponTarget == new Vector3(0, 0, 0))
                return;
            Vector3 offSetFinal = gameObject.transform.rotation * offSet;
            //Vector3 ForceVector = ( (weaponTarget-playerWeapon.transform.position) * 100f);
            //rb.AddForce(ForceVector.x,ForceVector.y,ForceVector.z);
            //DebugText.text = "Target Points: " + initialPos + " : " + weaponTarget; // remove later
            //rb.AddForce(ThrowingVec(weaponTarget, initialPos, offSet, speed));
            hammer_rb.velocity = ThrowingVec(weaponTarget, initialPos, offSetFinal, speed);
            Debug.Log("Weapon is thrown");
            throwWeapon = false;
        }
    }


    //void MakeWeapon()
    //{
    //    //Debug.Log("Weapon is being made");

    //    initialPos = playerWeapon.transform.position + (1f * new Vector3(0f, 0.25f, 0.75f));
    //    Quaternion weaponRot = playerWeapon.transform.rotation;

    //    GameObject newWeapon = Instantiate(weapon, initialPos, weaponRot);
    //    hammer_rb = newWeapon.GetComponent<Rigidbody>();
    //    hammer_rb.constraints = RigidbodyConstraints.FreezeAll;
    //    newWeapon.transform.SetParent(gameObject.transform);

    //    //Debug.Log("Weapon is made");        
    //}

    void GetWeapon()
    {
        initialPos = playerWeapon.transform.position + (0.1f * new Vector3(0f, 0.25f, 0.75f));
        Quaternion weaponRot = playerWeapon.transform.rotation;

        GameObject newWeapon = hammerPooler.GetPooledObject();

        newWeapon.transform.position =  initialPos;
        newWeapon.transform.rotation = weaponRot;

        hammer_rb = newWeapon.GetComponent<Rigidbody>();
        hammer_rb.constraints = RigidbodyConstraints.FreezeAll;
        newWeapon.transform.SetParent(gameObject.transform);

        newWeapon.SetActive(true);
    }

    /*Vector3 BallisticVel( Vector3 target, Vector3 initial, Vector3 offSet, float angle ) {
        Vector3 dir = target + offSet - initial;  // get target direction
        float h = dir.y;  // get height difference
        dir.y = 0;  // retain only the horizontal direction
        float dist = dir.magnitude;  // get horizontal distance
        float a = angle * Mathf.Deg2Rad;  // convert angle to radians
        dir.y = dist* Mathf.Tan(a);  // set dir to the elevation angle
        dist += h / Mathf.Tan(a);  // correct for small height differences
         // calculate the velocity magnitude
        float vel = Mathf.Sqrt(dist * Physics.gravity.magnitude / Mathf.Sin(2 * a));
        return vel* dir.normalized;
    }*/

    Vector3 ThrowingVec(Vector3 target, Vector3 initial, Vector3 offSet, float speed)
    {
            DebugText.text = "Target Points: " + target + " : " + initial + " : " + player_rb.velocity; // remove later
            return ((target + offSet - initial) - player_rb.velocity)*speed;

    }

}