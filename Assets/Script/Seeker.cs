using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Seeker : MonoBehaviour {

    Grid grid;
    public Transform target;
    
    public float speed ;
    Vector3[] path;
    int targetIndex;
    public LineRenderer lineRenderer;
    public GameObject cubePrefab;
    private Vector3 lastTransformPos, lastTargetPos;
    public List<Vector3> waypoints = new List<Vector3>();
    public bool isActive = false; // Individual isActive flag for each seeker
    private SeekerManager seekerManager; 
    void Start() {
        lastTransformPos = transform.position;
        lastTargetPos = target.position;

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = lineRenderer.endWidth = 0.9f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.black;
        target.GetComponent<Renderer>().enabled = false;
        seekerManager = FindObjectOfType<SeekerManager>();
        if (seekerManager == null)
        {
            GameObject managerObject = new GameObject("SeekerManager");
            seekerManager = managerObject.AddComponent<SeekerManager>();
        }

        // Register this seeker with the SeekerManager
        seekerManager.AddSeeker(this);
    }

    void Update() {
       
        
        if (Input.GetMouseButtonDown(0))
        {
            // Check if this seeker was clicked
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    // Set this seeker as active
                    seekerManager.SetActiveSeeker(this);
                    if(isActive){
                        waypoints.Clear();
                        RequestPathWithDelay();
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
                RequestPathWithDelay();
            }
        }

       
             if (Input.GetKeyUp(KeyCode.C))
        {

            Invoke("RequestPathWithDelay",.1f);
        }

        if (Input.GetMouseButtonDown(0)&&Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift)) {
            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Plane"))){
                CheckWaypointReached();
                waypoints.Add(hit.point);
                Invoke("RequestPathWithDelay",.1f);
            }
        
        }
        GameObject[] pathCubes = GameObject.FindGameObjectsWithTag("PathCube");
            
        }
    
    void RequestPathWithDelay()
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
            // foreach (Vector3 point in path) {
            //     GameObject cube = Instantiate(cubePrefab, point, Quaternion.identity);
            //     cube.tag = "PathCube";
            // }

            //UpdateLineRenderer();
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

    void CheckVisibility() {
        GameObject[] pathCubes = GameObject.FindGameObjectsWithTag("PathCube");
        foreach (GameObject cube in pathCubes) {
            if (Vector3.Distance(transform.position, cube.transform.position) < 0.5f) {
                cube.SetActive(false);
            }
        }
    }

    public void UpdateLineRenderer() {
        if (path != null && path.Length > 0) {
            lineRenderer.positionCount = path.Length + 2;
            lineRenderer.SetPosition(0, transform.position);
            for (int i = 1; i < path.Length; i++) {
                lineRenderer.SetPosition(i, path[i - 1]);
            }
            lineRenderer.SetPosition(path.Length, path[path.Length - 1]);
            lineRenderer.SetPosition(path.Length + 1, target.position);
        }
    }
    void CheckWaypointReached() {
        if (waypoints.Count == 0) return;

        Vector3 currentWaypoint = waypoints[0];
        float distanceToWaypoint = Vector3.Distance(transform.position, currentWaypoint);
        if (distanceToWaypoint < 0.5f)
        {
            waypoints.RemoveAt(0); 
        }
    }
}
