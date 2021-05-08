using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class TaxiAgent : Agent
{
    Vector3 _localStartingPosition;
    private Quaternion _startingRotation;
    private Vector3 _ballStartingPosition;
    private Rigidbody _ballRigidbody;
    float _acceleration = 0.5f;
    private float _turningSpeed = 100;
    float _maxMagnitudeVelocity = 300;

    [Header("Rewards")]
    private float _penalty = -1;
    
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
        _passengers = GetComponent<Passengers>();
        _returnSpot.SetAgent(this);
        _passengers.Init();
        _passengers.NewPassenger();
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = _localStartingPosition;
        transform.localRotation = _startingRotation;

        _ballRigidbody.transform.localPosition = _ballStartingPosition;
        _ballRigidbody.velocity = Vector3.zero;
        _ballRigidbody.angularVelocity = Vector3.zero;
        _hasPassenger = false;
        _passengers.NewPassenger();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(_hasPassenger);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            discreteActionsOut[0] = 1;
        }
        else if(Input.GetKey(KeyCode.RightArrow))
        {
            discreteActionsOut[0] = 2;
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            discreteActionsOut[0] = 4;
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var move = actionBuffers.DiscreteActions[0]; // [0,1,2,3,4]
        _moveInput = 0;
        _turningInput = 0;
        switch (move)
        {
            case 0:
                _moveInput = 0;
                break;
            case 1:
                _turningInput = -1;
                break;
            case 2:
                _turningInput = 1;
                break;
            case 3:
                _moveInput = 1;
                break;
            case 4:
                _moveInput = -1;
                break;
        }

        AddReward(_penalty / MaxStep);

        Move();
    }
    
    private void Move()
    {
        transform.Rotate(transform.up, _turningSpeed * _turningInput * Time.deltaTime);
        transform.localPosition = _ballRigidbody.transform.localPosition;
        
        var magnitude = _ballRigidbody.velocity.magnitude;
        var newVelocity = transform.forward * magnitude;
        _ballRigidbody.velocity = newVelocity;

        if (_ballRigidbody.velocity.sqrMagnitude < _maxMagnitudeVelocity)
        {
            _ballRigidbody.AddForce(transform.forward * _moveInput * _acceleration, ForceMode.VelocityChange);
        }
    }

    public void OnCollisionEnter(Collision other)
    {
        OnCollision(other.gameObject);
    }

    public void OnCollision(GameObject other)
    {
        if (other.gameObject.CompareTag("Checkpoint"))
        {
            var statsRecorder = Academy.Instance.StatsRecorder;
            statsRecorder.Add("Passengers picked up", 1, StatAggregationMethod.Sum);
            other.gameObject.SetActive(false);
            _hasPassenger = true;
        }
    }

    public void GoalHit()
    {
        if (_hasPassenger)
        {
            var statsRecorder = Academy.Instance.StatsRecorder;
            statsRecorder.Add("Passengers returned", 1, StatAggregationMethod.Sum);
            SetReward(2f);
            EndEpisode();
        }
    }
}
