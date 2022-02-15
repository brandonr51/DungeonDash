using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    public bool DoesStun;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (DoesStun)
            {
                other.gameObject.GetComponent<CCPlayerController_2D>().OnStun(2);
            }

            //Enter other trap effects here
        }
    }
}
