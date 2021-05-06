using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class TaxiAgent : Agent
{
    Vector3 _localStartingPosition;
    private Quaternion _startingRotation;
    private Vector3 _ballStartingPosition;
    private Rigidbody _ballRigidbody;
    private float _acceleration = 700;
    private float _turningSpeed = 100;
    private float _maxMagnitudeVelocity = 20;

    [Header("Rewards")] 
    private float _speedReward = 0.0001f;
    private float _penaltyPerTick = -0.0001f;
    
    private float _turningInput;
    private float _moveInput;
    private ReturnSpot _returnSpot;
    private Passengers _passengers;
    private CheckCollision _check;
    private bool _hasPassenger;


    public override void Initialize()
    {
        _check = GetComponentInChildren<CheckCollision>();
        _check.SetAgent(this);
        _ballRigidbody = GetComponentInChildren<Rigidbody>();
        _ballRigidbody.transform.SetParent(transform.parent);
        _localStartingPosition = transform.localPosition;
        _startingRotation = transform.localRotation;
        _ballStartingPosition = _ballRigidbody.transform.localPosition;
        _returnSpot = FindObjectOfType<ReturnSpot>();
        _returnSpot.gameObject.SetActive(false);
        _passengers = GetComponent<Passengers>();
        _returnSpot.SetAgent(this);
    }

    private void Update()
    {
        transform.Rotate(transform.up, _turningSpeed * _turningInput * Time.deltaTime);
        transform.localPosition = _ballRigidbody.transform.localPosition;
    }

    private void FixedUpdate()
    {
        var magnitude = _ballRigidbody.velocity.magnitude;
        var newVelocity = transform.forward * magnitude;
        _ballRigidbody.velocity = newVelocity;

        if (_ballRigidbody.velocity.magnitude < _maxMagnitudeVelocity)
        {
            _ballRigidbody.AddForce(transform.forward * _moveInput * _acceleration * Time.deltaTime);
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
        _returnSpot.gameObject.SetActive(false);
        
        _passengers.NewPassenger();
        _hasPassenger = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var speed = _ballRigidbody.velocity.magnitude / _maxMagnitudeVelocity;
        sensor.AddObservation(speed);
        sensor.AddObservation(_hasPassenger);
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
        
        AddReward(_penaltyPerTick);
        
        var speed = _ballRigidbody.velocity.magnitude / _maxMagnitudeVelocity;
        AddReward(speed * _speedReward);
        AddReward(_penaltyPerTick);
    }

    public void OnCollisionEnter(Collision other)
    {
        OnCollision(other.gameObject);
    }

    public void OnCollision(GameObject other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            AddReward(-1f);
            EndEpisode();
            return;
        }

        
        if (other.gameObject.CompareTag("Checkpoint"))
        {
            AddReward(1f);
            other.gameObject.SetActive(false);
            _returnSpot.gameObject.SetActive(true);
            _hasPassenger = true;
        }
    }

    public void GoalHit()
    {
        AddReward(5f);
        EndEpisode();
    }
}
