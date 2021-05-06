using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Passengers : MonoBehaviour
{
    private List<Passenger> _passengers = new List<Passenger>();
    private TaxiAgent _agent;
    void Start()
    {
        _passengers = FindObjectsOfType<Passenger>().ToList();
        _agent = GetComponent<TaxiAgent>();

        foreach (var passenger in _passengers)
        {
            passenger.gameObject.SetActive(false);
        }
    }

    public void NewPassenger()
    {
        foreach (var p in _passengers)
        {
            p.gameObject.SetActive(false);
        }
        var passenger = _passengers[Random.Range(0, _passengers.Count)];
        passenger.gameObject.SetActive(true);
    }

    public void Collision(GameObject other)
    {
        _agent.OnCollision(other);
    }
}
