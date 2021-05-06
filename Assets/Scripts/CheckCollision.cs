using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckCollision : MonoBehaviour
{
    private RLAgent _agent;
    private RLAgentSparse _gailAgent;
    private TaxiAgent _taxi;

    public void OnCollisionEnter(Collision other)
    {
        _agent?.OnCollision(other);
        _gailAgent?.OnCollision(other);
        _taxi?.OnCollision(other.gameObject);
    }

    public void SetAgent(RLAgent rlAgent)
    {
        _agent = rlAgent;
    }
    
    public void SetAgent(RLAgentSparse gailAgent)
    {
        _gailAgent = gailAgent;
    }
    
    public void SetAgent(TaxiAgent taxi)
    {
        _taxi = taxi;
    }

    public void Trigger(Checkpoint checkpoint)
    {
        _agent?.CheckpointHit(checkpoint);
        _gailAgent?.CheckpointHit(checkpoint);
    }
}
