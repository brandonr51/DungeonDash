using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILookAtCamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localEulerAngles = new Vector3(transform.eulerAngles.x * 0, transform.eulerAngles.y * 0, transform.eulerAngles.z * 0);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x * 0, transform.eulerAngles.y * 0, transform.eulerAngles.z * 0);
    }
}
