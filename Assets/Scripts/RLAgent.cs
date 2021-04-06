using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using TreeEditor;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class RLAgent : Agent
{
    private Rigidbody _rigidbody;
    private Vector3 _localStartingPosition;
    private Quaternion _startingRotation;
    [SerializeField] private float _speed = 1;
    [SerializeField] private float _turningSpeed = 1;

    private List<GameObject> _checkPointsVisited = new List<GameObject>();
    private float _turningInput;
    private float _moveInput;
    private float _timeLeftBeforeRestart = 30;

    void Start()
    {
        _localStartingPosition = transform.localPosition;
        _startingRotation = transform.localRotation;
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        _timeLeftBeforeRestart -= Time.deltaTime;
        transform.Rotate(transform.up, _turningSpeed * _turningInput * Time.deltaTime);

        if (transform.localPosition.y < -1)
        {
            EndEpisode();
        }

        if (_timeLeftBeforeRestart < 0)
        {
            EndEpisode();
        }
    }

    private void FixedUpdate()
    {
        _rigidbody.AddForce(transform.forward * _moveInput * _speed);
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = _localStartingPosition;
        transform.rotation = _startingRotation;
        _checkPointsVisited.Clear();
        _rigidbody.velocity = Vector3.zero;
        _timeLeftBeforeRestart = 30;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(_rigidbody.velocity);
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(transform.localRotation);
        AddReward(-0.0001f);
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Vertical");
        actionsOut[1] = Input.GetAxis("Horizontal");
    }

    public override void OnActionReceived(float[] actionBuffers)
    {
        _moveInput = actionBuffers[0];
        _moveInput = Mathf.Clamp01(_moveInput);
        _turningInput = _turningSpeed * actionBuffers[1];
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            if (!_checkPointsVisited.Contains(other.gameObject))
            {
                _checkPointsVisited.Add(other.gameObject);
                AddReward(1f);
                _timeLeftBeforeRestart = 30;
            }
        }
        
        if(other.CompareTag("Goal"))
        {
            if (_checkPointsVisited.Count > 0)
            {
                AddReward(1);
                EndEpisode();
            }
        }
    }

    public void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            EndEpisode();
        }
    }
}
