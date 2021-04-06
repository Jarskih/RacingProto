using System;
using System.Collections;
using System.Collections.Generic;
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

    void Start()
    {
        _localStartingPosition = transform.localPosition;
        _startingRotation = transform.localRotation;
        _rigidbody = GetComponent<Rigidbody>();
    }
    
    public override void OnEpisodeBegin()
    {
        transform.localPosition = _localStartingPosition;
        transform.rotation = _startingRotation;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Vertical");
        actionsOut[1] = Input.GetAxis("Horizontal");
    }

    public override void OnActionReceived(float[] actionBuffers)
    {
        _rigidbody.AddForce(transform.forward * actionBuffers[0] * _speed);
        _rigidbody.AddTorque(Vector3.up * actionBuffers[1] * _turningSpeed);
    }
}
