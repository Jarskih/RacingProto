using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CheckpointCollection : MonoBehaviour
{
    public List<Checkpoint> Checkpoints;
    
    // Start is called before the first frame update
    void Awake()
    {
        Checkpoints = new List<Checkpoint>(GetComponentsInChildren<Checkpoint>());
    }
}
