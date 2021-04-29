using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckCollision : MonoBehaviour
{
    private RLAgent _agent;
    private RLAgentSparse _gailAgent;
    
    public void OnCollisionEnter(Collision other)
    {
        _agent?.OnCollision(other);
        _gailAgent?.OnCollision(other);
    }

    public void SetAgent(RLAgent rlAgent)
    {
        _agent = rlAgent;
    }
    
    public void SetAgent(RLAgentSparse gailAgent)
    {
        _gailAgent = gailAgent;
    }
    
    public void Trigger(Checkpoint checkpoint)
    {
        _agent?.CheckpointHit(checkpoint);
        _gailAgent?.CheckpointHit(checkpoint);
    }
}
