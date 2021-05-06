using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnSpot : MonoBehaviour
{
    private TaxiAgent _taxiAgent;

    public void SetAgent(TaxiAgent taxiAgent)
    {
        _taxiAgent = taxiAgent;
    }

    private void OnCollisionEnter(Collision other)
    {
        _taxiAgent.GoalHit();
    }
}
