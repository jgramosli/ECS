using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyMeSoon : MonoBehaviour
{
    public float countDown = 1;

    // Update is called once per frame
    void Update()
    {
        countDown -= Time.deltaTime;
        if (countDown < 0)
            GameObject.Destroy(gameObject);
    }
}
