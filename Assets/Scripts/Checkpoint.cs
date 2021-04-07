using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var car = other.GetComponent<CheckCollision>();
        if (car != null)
        {
            car.Trigger(this);
        }
    }
}
