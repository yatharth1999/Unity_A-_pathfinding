using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PFAstar : MonoBehaviour
{
    Grid grid;
    public Transform target;
    public float speed ;
    Vector3[] path;
    int targetIndex;
    private Vector3 lastTransformPos;
    private Vector3 lastTargetPos;
    public List<Vector3> waypoints = new List<Vector3>();
    public bool isActive = false;
    public Vector3 goalPosition;
    public float moveSpeed = 5.0f;
    private bool isGoalReached = false;
    public float repulsionRadius = 1.5f;
    public float repulsionStrengthFactor = 4.0f;
    private float conditionMetTimer = 0f;
    private const float conditionDurationThreshold = 1f; 

    void Start() {
        goalPosition = transform.position;
        lastTransformPos = transform.position;
        lastTargetPos = target.position;
        target.GetComponent<Renderer>().enabled = false;
        UnitSelections.Instance.unitList.Add(this.gameObject);
    }

    void Update() {      
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Input.GetMouseButtonDown(0)){
            if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)){
                SetWaypoint();
            }
        }
        if (waypoints.Count > 0)
        {
            MoveTowardsGoal();
        }

        if (Input.GetMouseButtonDown(0)) {
            
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
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Target"))) {
                target.GetComponent<Renderer>().enabled = true;
                target.position = hit.point;
                RequestPath();
            }
            // if(Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift)){
            //     if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Plane"))){
            //         CheckWaypointReached();
            //         waypoints.Add(hit.point);
            //         Invoke("RequestPath",.1f);
            //     }
            // }
        }   
    }
    
    void RequestPath()
    {   
        if (target.GetComponent<Renderer>().enabled && isActive) {
            foreach (GameObject cube in GameObject.FindGameObjectsWithTag("PathCube")){
                Destroy(cube);
            }
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

            // Vector3 directionToWaypoint = (currentWaypoint - transform.position).normalized;
            // Quaternion lookRotation = Quaternion.LookRotation(directionToWaypoint);
            // transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5* Time.deltaTime);
            // transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
            
            Vector3 directionToWaypoint = (currentWaypoint - transform.position).normalized;
            Vector3 avoidanceForce = AvoidAgentCollisions(); // Calculate avoidance force
            Vector3 repulsiveForce = CalculateRepulsiveForce();
            
            Vector3 combinedDirection = (directionToWaypoint + avoidanceForce+repulsiveForce).normalized; // Combine with direction to waypoint
            // Adjust rotation to face combined direction
            Quaternion lookRotation = Quaternion.LookRotation(combinedDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5 * Time.deltaTime);

        // Move towards the waypoint, adjusted by avoidance
            transform.position = Vector3.MoveTowards(transform.position, transform.position + combinedDirection, speed * Time.deltaTime);
            yield return null;
        }
    }

    void CheckWaypointReached() {
        if (waypoints.Count == 0) return;

        Vector3 currentWaypoint = waypoints[0];
        float distanceToWaypoint = Vector3.Distance(transform.position, currentWaypoint);
        if (distanceToWaypoint < 1.0f)
        {
            waypoints.RemoveAt(0); 
        }
    }

   
    void HandleInput()
    {
        
    }

    void SetWaypoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Plane")))
        {
            Vector3 newGoal = new Vector3(hit.point.x, 0, hit.point.z);
            waypoints.Add(newGoal);
            Debug.Log($"New waypoint added at: {newGoal}");
            if (waypoints.Count == 1) goalPosition = waypoints[0];
            isGoalReached = false;
        }
    }

    void MoveTowardsGoal()
    {
        Vector3 attractiveForce = CalculateAttractiveForce();
        Vector3 repulsiveForce = CalculateRepulsiveForce();
        Vector3 avoidanceForce = AvoidAgentCollisions();
        if ((repulsiveForce + attractiveForce).magnitude < 0.5f)
    {
        // Increment the timer by the time elapsed since the last frame
        conditionMetTimer += Time.deltaTime;

        // Check if the condition has been true for more than 3 seconds
        if (conditionMetTimer >= conditionDurationThreshold)
        {
            // Skip the waypoint and reset the timer
            SkipCurrentWaypoint();
            conditionMetTimer = 0f; // Reset the timer
        }
    }
    else
    {
        // Reset the timer if the condition is not met
        conditionMetTimer = 0f;
    }
        // Vector3 potentialforce = CalculatePotentialFieldForce();
        Vector3 totalForce = attractiveForce + repulsiveForce + avoidanceForce;
        totalForce = new Vector3(totalForce.x,0,totalForce.z);
        Debug.Log($"Attractive Force: {attractiveForce}, Repulsive Force: {repulsiveForce}, Total Force: {totalForce.normalized}");

        Quaternion targetRotation = Quaternion.LookRotation(totalForce.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

        transform.position += totalForce.normalized * moveSpeed * Time.deltaTime;

        if (Vector3.Distance(transform.position, goalPosition) < 2f)
        {
            SkipCurrentWaypoint();
        }
    }

    void SkipCurrentWaypoint()
    {
        waypoints.RemoveAt(0); // Remove current waypoint
        if (waypoints.Count > 0) goalPosition = waypoints[0]; // Set next waypoint as goal
        else isGoalReached = true;
    }

    private Vector3 CalculateAttractiveForce()
    {
        Vector3 agentToGoal = goalPosition - transform.position;
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
                if (hit.collider.gameObject != gameObject)
                {
                    Vector3 agentToObstacle = transform.position - hit.point;
                    float additionalRepulsion = 1.0f - (hit.distance / repulsionRadius);
                    repulsiveForce += agentToObstacle.normalized * additionalRepulsion * repulsionStrengthFactor;
                }
            }
        }

        return repulsiveForce.normalized;
    }

    
    Vector3 AvoidAgentCollisions()
    {
        float avoidanceRadius = .5f;
        float avoidanceForceFactor = 5.0f;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, avoidanceRadius);
        Vector3 avoidanceForce = Vector3.zero;

        foreach (Collider collider in hitColliders)
        {
            if (collider.gameObject != gameObject && collider.CompareTag("Seeker"))
            {
                Vector3 agentToOtherAgent = transform.position - collider.transform.position;
                float distance = agentToOtherAgent.magnitude;

                if (distance < avoidanceRadius)
                {
                    float avoidanceStrength = 1.0f - (distance / .5f);
                    avoidanceForce += agentToOtherAgent.normalized * avoidanceStrength;
                }
            }
        }

        return avoidanceForce.normalized * avoidanceForceFactor;
    }
}
