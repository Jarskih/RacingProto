using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Networking;

public class RLAgent : Agent
{
    Vector3 _localStartingPosition;
    private Quaternion _startingRotation;
    private Vector3 _ballStartingPosition;
    private Rigidbody _ballRigidbody;
    [SerializeField] private float _speed = 1;
    [SerializeField] private float _turningSpeed = 1;
    [SerializeField] private float _maxVelocity = 1;
    [SerializeField] private float _gravityForce = 10;

    [Header("Rewards")] 
    [SerializeField] private float _rightDirectionReward = 0.1f;
    [SerializeField] private float _speedReward = 0.1f;
    [SerializeField] private float _penaltyPerTick = -0.0001f;
    
    private Checkpoints _checkpoints;
    private float _turningInput;
    private float _moveInput;
    [SerializeField] private float _timeLeftBeforeRestart = 0;
    private float _timeBetweenCheckpoints = 30;
    private float _maxDistanceToWaypoint = 20;


    public override void Initialize()
    {
        _ballRigidbody = GetComponentInChildren<Rigidbody>();
        _ballRigidbody.GetComponent<CheckCollision>().SetAgent(this);
        _ballRigidbody.transform.SetParent(null);
        _checkpoints = GetComponent<Checkpoints>();
        _timeLeftBeforeRestart = _timeBetweenCheckpoints;
        _localStartingPosition = transform.localPosition;
        _startingRotation = transform.localRotation;
        _ballStartingPosition = _ballRigidbody.transform.localPosition;
    }

    private void Update()
    {
        _timeLeftBeforeRestart -= Time.deltaTime;
        transform.Rotate(transform.up, _turningSpeed * _turningInput * Time.deltaTime);
        transform.localPosition = _ballRigidbody.transform.localPosition;
        
        if (transform.localPosition.y < -1)
        {
            EndEpisode();
            return;
        }

        if (_timeLeftBeforeRestart < 0)
        {
            AddReward(-1f);
            EndEpisode();
            return;
        }
    }

    private void FixedUpdate()
    {
        var magnitude = _ballRigidbody.velocity.magnitude;
        var newVelocity = transform.forward * magnitude;
        _ballRigidbody.velocity = newVelocity;
        
        if (_ballRigidbody.velocity.x < _maxVelocity && _ballRigidbody.velocity.z < _maxVelocity)
        {
            _ballRigidbody.AddForce(transform.forward * _moveInput * _speed);
        }
        
        //_ballRigidbody.AddForce(-Vector3.up * _gravityForce);
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = _localStartingPosition;
        transform.localRotation = _startingRotation;

        _ballRigidbody.transform.localPosition = _ballStartingPosition;
        _ballRigidbody.velocity = Vector3.zero;
        _ballRigidbody.angularVelocity = Vector3.zero;
        _timeLeftBeforeRestart = _timeBetweenCheckpoints;
        _checkpoints.Reset();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var nextCheckPointDir = (_checkpoints.NextCheckpointPosition - transform.position).normalized;
        var facing = Vector3.Dot(nextCheckPointDir, _ballRigidbody.velocity.normalized);
        sensor.AddObservation(facing);
        var speed = _ballRigidbody.velocity.magnitude / _maxVelocity;
        sensor.AddObservation(speed);
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Vertical");
        actionsOut[1] = Input.GetAxis("Horizontal");
    }

    public override void OnActionReceived(float[] actionBuffers)
    {
        _moveInput = actionBuffers[0];
        _moveInput = Mathf.Clamp01(_moveInput); // [0,1]
        _turningInput = _turningSpeed * actionBuffers[1]; // [-1,1]
        
        var nextCheckPointDir = (_checkpoints.NextCheckpointPosition - transform.position).normalized;
        var facing = Vector3.Dot(nextCheckPointDir, _ballRigidbody.velocity.normalized);
        var speed = _ballRigidbody.velocity.magnitude / _maxVelocity;
        AddReward(facing * _rightDirectionReward);
        AddReward(speed * _speedReward);
        AddReward(_penaltyPerTick);
    }

    public void OnCollisionEnter(Collision other)
    {
        OnCollision(other);
    }

    public void OnCollision(Collision other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            AddReward(-1f);
            EndEpisode();
        }
    }

    public void CheckpointHit(Checkpoint checkpoint)
    {
        if (_checkpoints.CheckpointHit(checkpoint))
        {
            _timeLeftBeforeRestart = _timeBetweenCheckpoints;
        }
    }
}
