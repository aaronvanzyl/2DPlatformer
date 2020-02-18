using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointTraverser : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed;
    public int nextWaypoint = 1;

    private void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, waypoints[nextWaypoint].position, speed * Time.fixedDeltaTime);
        if (Vector3.Distance(transform.position, waypoints[nextWaypoint].position) < Mathf.Epsilon)
        {
            nextWaypoint = (nextWaypoint + 1) % waypoints.Length;
        }

    }
}
