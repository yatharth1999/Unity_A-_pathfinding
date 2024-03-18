using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Seeker : MonoBehaviour {

    Grid grid;
    public Transform target;
    public float speed ;
    Vector3[] path;
    int targetIndex;
    private Vector3 lastTransformPos;
    private Vector3 lastTargetPos;
    public List<Vector3> waypoints = new List<Vector3>();
    public bool isActive = false;
    void Start() {
        lastTransformPos = transform.position;
        lastTargetPos = target.position;
        target.GetComponent<Renderer>().enabled = false;
        UnitSelections.Instance.unitList.Add(this.gameObject);
    }

    void Update() {
        if (Input.GetKeyUp(KeyCode.C))
            {
                Invoke("RequestPath",.1f);
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
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Plane"))){
                    CheckWaypointReached();
                    waypoints.Add(hit.point);
                    Invoke("RequestPath",.1f);
                }
            }
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

            Vector3 directionToWaypoint = (currentWaypoint - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(directionToWaypoint);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5* Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
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
}
