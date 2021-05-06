using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passenger : MonoBehaviour
{
    private Passengers _passengers;
    private void Awake()
    {
        _passengers = FindObjectOfType<Passengers>();
    }

    private void OnCollisionEnter(Collision other)
    {
        _passengers.Collision(this.gameObject);
    }
}
