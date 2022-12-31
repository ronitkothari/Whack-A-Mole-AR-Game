using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyByTime_2 : MonoBehaviour {

    public float lifeTime;
    private void OnEnable()
    {
        Invoke("Destroy", lifeTime);
    }
    private void Destroy()
    {
        gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        CancelInvoke();
    }
}
