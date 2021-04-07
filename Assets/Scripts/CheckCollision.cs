using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckCollision : MonoBehaviour
{
    private RLAgent _agent;
    
    public void OnCollisionEnter(Collision other)
    {
        _agent.OnCollision(other);
    }

    public void SetAgent(RLAgent rlAgent)
    {
        _agent = rlAgent;
    }
    
    public void Trigger(Checkpoint checkpoint)
    {
        _agent.CheckpointHit(checkpoint);
    }
}
