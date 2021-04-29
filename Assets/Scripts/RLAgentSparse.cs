using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class RLAgentSparse : Agent
{
    Vector3 _localStartingPosition;
    private Quaternion _startingRotation;
    private Vector3 _ballStartingPosition;
    private Rigidbody _ballRigidbody;
    private float _acceleration = 50;
    private float _turningSpeed = 100;
    private float _maxMagnitudeVelocity = 10;

    [Header("Rewards")] 
    [SerializeField] private float _rightDirectionReward = 0.1f;
    [SerializeField] private float _speedReward = 0.1f;
    [SerializeField] private float _penaltyPerTick = -0.0001f;
    
    private Checkpoints _checkpoints;
    private float _turningInput;
    private float _moveInput;


    public override void Initialize()
    {
        _ballRigidbody = GetComponentInChildren<Rigidbody>();
        _ballRigidbody.GetComponent<CheckCollision>().SetAgent(this);
        _ballRigidbody.transform.SetParent(transform.parent);
        _checkpoints = GetComponent<Checkpoints>();
        _localStartingPosition = transform.localPosition;
        _startingRotation = transform.localRotation;
        _ballStartingPosition = _ballRigidbody.transform.localPosition;
    }

    private void Update()
    {
        transform.Rotate(transform.up, _turningSpeed * _turningInput * Time.deltaTime);
        transform.localPosition = _ballRigidbody.transform.localPosition;
        
        if (transform.localPosition.y < -1)
        {
            EndEpisode();
            return;
        }
    }

    private void FixedUpdate()
    {
        var magnitude = _ballRigidbody.velocity.magnitude;
        var newVelocity = transform.forward * magnitude;
        _ballRigidbody.velocity = newVelocity;

        if (_ballRigidbody.velocity.magnitude < _maxMagnitudeVelocity)
        {
            _ballRigidbody.AddForce(transform.forward * _moveInput * _acceleration);
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
        _checkpoints.Reset();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var nextCheckPointDir = (_checkpoints.NextCheckpointPosition - transform.position).normalized;
        var facing = Vector3.Dot(nextCheckPointDir, _ballRigidbody.velocity.normalized);
        //sensor.AddObservation(facing);
        var speed = _ballRigidbody.velocity.magnitude / _maxMagnitudeVelocity;
        sensor.AddObservation(speed);
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = 0;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            actionsOut[0] = 1;
        }
        else if(Input.GetKey(KeyCode.RightArrow))
        {
            actionsOut[0] = 2;
        }
        
        actionsOut[1] = 0;
        if (Input.GetKey(KeyCode.UpArrow))
        {
            actionsOut[1] = 1;
        }
        
    }

    public override void OnActionReceived(float[] actionBuffers)
    {
        var turning = actionBuffers[0]; // [0,1,2]
        switch (turning)
        {
            case 0:
                _turningInput = 0;
                break;
            case 1:
                _turningInput = -1;
                break;
            case 2:
                _turningInput = 1;
                break;
        }

        _moveInput = actionBuffers[1]; // [0,1]

        var nextCheckPointDir = (_checkpoints.NextCheckpointPosition - transform.position).normalized;
        var facing = Vector3.Dot(nextCheckPointDir, _ballRigidbody.velocity.normalized);
        var speed = _ballRigidbody.velocity.magnitude / _maxMagnitudeVelocity;
        //AddReward(facing * _rightDirectionReward);
        AddReward(speed * _speedReward);
        // AddReward(_penaltyPerTick);
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
        }
    }
}
