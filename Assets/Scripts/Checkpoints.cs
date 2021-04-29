using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class Checkpoints : MonoBehaviour
{
    public string _tag;
    public Vector3 NextCheckpointPosition => _nextCheckpoint.transform.position;
    [SerializeField] private Checkpoint _nextCheckpoint;
    [SerializeField] private List<Checkpoint> _checkpoints;
    [SerializeField] private int _currentIndex;
    private RLAgent _agent;
    private static int _timesFinishedTrack = 0;

    void Start()
    {
        var checkpoints = FindObjectsOfType<CheckpointCollection>();
        foreach (var checkpoint in checkpoints)
        {
            if (checkpoint.gameObject.CompareTag(_tag))
            {
                _checkpoints = checkpoint.Checkpoints;
            }
        }
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
                var statsRecorder = Academy.Instance.StatsRecorder;
                _timesFinishedTrack++;
                statsRecorder.Add("Times finished track", _timesFinishedTrack);
                _agent.EndEpisode();
            }
            else
            {
                _agent.AddReward(0.5f);
                _nextCheckpoint = _checkpoints[_currentIndex];
            }
            return true;
        }
        return false;
    }
}
