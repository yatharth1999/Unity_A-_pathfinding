using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : MonoBehaviour {

    Grid grid;
    public Transform target;
    public Transform seeker ;
    public float speed = 0f;
    Vector3[] path;
    int targetIndex;
    public LineRenderer lineRenderer;
    public GameObject cubePrefab;
    private Vector3 lastTransformPos, lastTargetPos;
    public List<Vector3> waypoints = new List<Vector3>();

    void Start() {
        lastTransformPos = seeker.position;
        lastTargetPos = target.position;

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = lineRenderer.endWidth = 0.9f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.black;
        seeker.GetComponent<Renderer>().enabled = false;
        target.GetComponent<Renderer>().enabled = false;
    }

    void Update() {
       
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Seeker"))) {
                seeker.GetComponent<Renderer>().enabled = true;
                seeker.position = hit.point;
                RequestPathWithDelay();
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
                waypoints.Add(hit.point);
                Invoke("RequestPathWithDelay",.1f);
            }
        
        }
        GameObject[] pathCubes = GameObject.FindGameObjectsWithTag("PathCube");
            
        }
    
    void RequestPathWithDelay()
    {   
        if (target.GetComponent<Renderer>().enabled && GetComponent<Renderer>().enabled) {
            foreach (GameObject cube in GameObject.FindGameObjectsWithTag("PathCube")){
                Destroy(cube);
            }
            PathRequestManager.RequestPath(seeker.position, target.position, waypoints, OnPathFound);
        }
    }
    public void OnPathFound(Vector3[] newPath, bool pathSuccessful) {
        if (pathSuccessful) {
            path = newPath;
            targetIndex = 0;
            foreach (Vector3 point in path) {
                GameObject cube = Instantiate(cubePrefab, point, Quaternion.identity);
                cube.tag = "PathCube";
            }

            UpdateLineRenderer();
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    IEnumerator FollowPath() {
        while (true) {
            
            Vector3 currentWaypoint = path[targetIndex];
            if (Vector3.Distance(seeker.position, currentWaypoint) < 0.1f) {
                targetIndex++;
                if (targetIndex >= path.Length) {
                    break;
                }
                currentWaypoint = path[targetIndex];
            }

            Vector3 directionToWaypoint = (currentWaypoint - seeker.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(directionToWaypoint);
            seeker.rotation = Quaternion.Slerp(seeker.rotation, lookRotation, 2 * Time.deltaTime);
            seeker.position = Vector3.MoveTowards(seeker.position, currentWaypoint, speed * Time.deltaTime);
            yield return null;
        }
    }

    void CheckVisibility() {
        GameObject[] pathCubes = GameObject.FindGameObjectsWithTag("PathCube");
        foreach (GameObject cube in pathCubes) {
            if (Vector3.Distance(seeker.position, cube.transform.position) < 0.5f) {
                cube.SetActive(false);
            }
        }
    }

    public void UpdateLineRenderer() {
        if (path != null && path.Length > 0) {
            lineRenderer.positionCount = path.Length + 2;
            lineRenderer.SetPosition(0, seeker.position);
            for (int i = 1; i < path.Length; i++) {
                lineRenderer.SetPosition(i, path[i - 1]);
            }
            lineRenderer.SetPosition(path.Length, path[path.Length - 1]);
            lineRenderer.SetPosition(path.Length + 1, target.position);
        }
    }
}