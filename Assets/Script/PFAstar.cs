
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PFAstar : MonoBehaviour 
{
    public Vector3 goalPosition;
    public float repulsionRadius = 1.5f;
    public float repulsionStrengthFactor = 4.0f;
    public bool isActive = false;
    private float timeElapsedWithoutProgress = 0f;
    Grid grid;
    public Transform target;
    public float speed = 5.0f;
    Vector3[] path;
    int targetIndex;

    public List<Vector3> waypoints = new List<Vector3>();
    
    void Start() {
        target.GetComponent<Renderer>().enabled = false;
        UnitSelections.Instance.unitList.Add(this.gameObject);
    }
    void Update() {
         if (timeElapsedWithoutProgress > 1f) {
        // Reset the time elapsed
        timeElapsedWithoutProgress = 0f;

        // Request a new path
         if (target.GetComponent<Renderer>().enabled ) {
            PathRequestManager.RequestPath(transform.position, target.position, waypoints, OnPathFound);
        }
        }
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    if(isActive){
                        waypoints.Clear();
                        RequestPath();
                    }
                }
            }
           
        }
        if (Input.GetMouseButtonDown(1)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Target"))) {
                target.GetComponent<Renderer>().enabled = true;
                target.position = hit.point;
                RequestPath();
            }
            if(Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift)){
                if(isActive){
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Plane"))){
                        CheckWaypointReached();
                        waypoints.Add(hit.point);
                        Invoke("RequestPath",.1f);
                    }
                }
            }
        }
        CheckWaypointReached();

    }
    
    void RequestPath()
    {   
        if (target.GetComponent<Renderer>().enabled && isActive) {
            PathRequestManager.RequestPath(transform.position, target.position, waypoints, OnPathFound);
        }
    }
    
    public void OnPathFound(Vector3[] newPath, bool pathSuccessful) {
        if (pathSuccessful) {
            path = newPath;
            targetIndex = 0;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }
    IEnumerator FollowPath() {
        while (true) {
            Vector3 currentWaypoint = path[targetIndex];
            if (Vector3.Distance(transform.position, currentWaypoint) < 0.1f) {
                targetIndex++;
                if (targetIndex >= path.Length) {
                    break;
                }

                currentWaypoint = path[targetIndex];
            }
            Vector3 attractiveForce = CalculateAttractiveForce(currentWaypoint);
            Vector3 repulsiveForce = CalculateRepulsiveForce();
            Vector3 avoidanceForce = AvoidAgentCollisions();
            if ((repulsiveForce + attractiveForce).magnitude < 0.5f) {
            if (waypoints.Count > 0) {
                // If there are waypoints remaining, request a new path to the next waypoint
                RequestPath();
            } else {
                // If no waypoints remaining, break out of the loop
                break;
            }
        }
            if ((repulsiveForce + attractiveForce).magnitude < 0.5f) {
            
            // If the seeker reaches the current waypoint, reset the time elapsed
            timeElapsedWithoutProgress = 0f;

            targetIndex++;
            if (targetIndex >= path.Length) {
                break;
            }

            currentWaypoint = path[targetIndex];
             } else {
            // If the seeker is moving towards the waypoint, update the time elapsed
            timeElapsedWithoutProgress += Time.deltaTime;
            }
            Vector3 totalForce = attractiveForce + repulsiveForce + avoidanceForce;
            totalForce = new Vector3(totalForce.x, 0, totalForce.z);
            Quaternion targetRotation = Quaternion.LookRotation(totalForce.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            transform.position += totalForce.normalized * speed * Time.deltaTime;
            if (Vector3.Distance(transform.position, currentWaypoint) < 2f) {
                CheckWaypointReached();
            }
            yield return null;
        }
    }
    private Vector3 CalculateAttractiveForce(Vector3 goalPosition)
    {
        Vector3 agentToGoal = (goalPosition - transform.position)*2.0f;
        return agentToGoal.normalized;
    }

    private Vector3 CalculateRepulsiveForce()
    {
        Vector3 repulsiveForce = Vector3.zero;
        int numRays = 8;
        for (int i = 0; i < numRays; i++)
        {
            float angle = i * 2 * Mathf.PI / numRays;
            Vector3 rayDirection = new Vector3(Mathf.Cos(angle),0, Mathf.Sin(angle));

            RaycastHit hit;
            if (Physics.Raycast(transform.position, rayDirection, out hit, repulsionRadius))
            {
                if (hit.collider.gameObject != gameObject&&hit.collider.CompareTag("Obstacle"))
                {
                    Vector3 agentToObstacle = transform.position - hit.point;
                    float additionalRepulsion = 1.0f - (hit.distance / repulsionRadius);
                    repulsiveForce += agentToObstacle.normalized * additionalRepulsion * repulsionStrengthFactor;
                }
            }
        }
        return repulsiveForce.normalized;
    }

    private Vector3 AvoidAgentCollisions()
    {
        Vector3 avoidanceForce = Vector3.zero;
        int numRays = 100;
        float avoidanceRadius = 1f;
        float avoidanceForceFactor = 5.0f;

        for (int i = 0; i < numRays; i++)
        {
            float angle = i * 2 * Mathf.PI / numRays;
            Vector3 rayDirection = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));

            RaycastHit hit;
            if (Physics.Raycast(transform.position, rayDirection, out hit, avoidanceRadius))
            {
                if (hit.collider.gameObject != gameObject && hit.collider.CompareTag("Seeker"))
                {
                    Vector3 agentToOtherAgent = transform.position - hit.point;
                    float additionalRepulsion = 1.0f - (hit.distance / avoidanceRadius);
                    avoidanceForce += agentToOtherAgent.normalized * additionalRepulsion * avoidanceForceFactor;
                }
            }
        }

        return avoidanceForce.normalized;
    }

    void CheckWaypointReached() {
        if (waypoints.Count == 0) return;
        if (Vector3.Distance(transform.position,waypoints[0]) < 5.0f)
            waypoints.RemoveAt(0); 
    }


}

