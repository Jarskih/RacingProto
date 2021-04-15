using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class RLAgent_1 : Agent
{
    Vector3 _localStartingPosition;
    private Quaternion _startingRotation;
    private Vector3 _ballStartingPosition;
    private Rigidbody _ballRigidbody;
    [SerializeField] private float _speed = 1;
    [SerializeField] private float _turningSpeed = 1;
    [SerializeField] private float _maxVelocity = 1;
    [SerializeField] private float _gravityForce = 10;
    
    private Checkpoints _checkpoints;
    private float _turningInput;
    private float _moveInput;
    [SerializeField] private float _timeLeftBeforeRestart = 0;
    private float _timeBetweenCheckpoints = 30;
    private float _maxDistanceToWaypoint = 20;


    public override void Initialize()
    {
        _ballRigidbody = GetComponentInChildren<Rigidbody>();
       // _ballRigidbody.GetComponent<CheckCollision>().SetAgent(this);
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
        
        _ballRigidbody.AddForce(-Vector3.up * _gravityForce);
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
        Vector3 velocity = _ballRigidbody.velocity;
        float normalizedX = velocity.x / _maxVelocity; // [0,1]
        float normalizedZ = velocity.z / _maxVelocity; // [0,1]
        Vector3 normalizedRotation = transform.localRotation.eulerAngles / 360.0f;  // [0,1]
        Vector3 distanceToNextCheckpoint = transform.position - _checkpoints.NextCheckpointPosition;
        Vector3 normalizedDistanceToCheckPoint = distanceToNextCheckpoint / _maxDistanceToWaypoint;
        
        //sensor.AddObservation(normalizedX);
        //sensor.AddObservation(normalizedZ);
        //sensor.AddObservation(normalizedRotation);
        sensor.AddObservation(transform.forward);
        sensor.AddObservation(normalizedDistanceToCheckPoint);
        sensor.AddObservation(distanceToNextCheckpoint.normalized);
        
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
        _moveInput = Mathf.Clamp01(_moveInput); // [0,1]
        _turningInput = _turningSpeed * actionBuffers[1]; // [-1,1]
    }

    public void OnCollisionEnter(Collision other)
    {
        OnCollision(other);
    }

    public void OnCollision(Collision other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
           // Debug.Log("Wall");
           // EndEpisode();
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
