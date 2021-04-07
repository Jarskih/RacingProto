using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoints : MonoBehaviour
{
    public Vector3 NextCheckpointPosition => _nextCheckpoint.transform.position;
    [SerializeField] private Checkpoint _nextCheckpoint;
    [SerializeField] private List<Checkpoint> _checkpoints;
    [SerializeField] private int _currentIndex;
    private RLAgent _agent;

    void Start()
    {
        _checkpoints = FindObjectOfType<CheckpointCollection>().Checkpoints;
        _agent = GetComponent<RLAgent>();
        Reset();
    }

    public void Reset()
    {
        _currentIndex = 0;
        _nextCheckpoint = _checkpoints[_currentIndex];
    }
    
    public bool CheckpointHit(Checkpoint checkpointHit)
    {
        if (_nextCheckpoint == checkpointHit)
        {
            _currentIndex++;
            if (_currentIndex >= _checkpoints.Count)
            {
                _agent.AddReward(1f);
                Debug.Log("Finished track");
                _agent.EndEpisode();
            }
            else
            {
                _agent.AddReward(1.0f/_checkpoints.Count);
                _nextCheckpoint = _checkpoints[_currentIndex];
            }
            return true;
        }
        return false;
    }
}
