
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Grid : MonoBehaviour
{
  public int obstacles;
  Node[,] grid;
  public GameObject cubePrefab;
  public bool pathGizmosShow = true;
  private List<GameObject> spawnedObjects = new List<GameObject>();
  public Vector2 gridWorldSize;
  public LayerMask unwalkableMask;
  public float nodeRadius;
  float nodeDiameter;
  int gridSizeX,gridSizeY;

  public GameObject[] innerPlanes; // Array of inner planes
  public GameObject outerPlane;


  void Start() {
    nodeDiameter = nodeRadius * 2;
    gridSizeX = Mathf.RoundToInt(gridWorldSize.x/nodeDiameter);
    gridSizeY = Mathf.RoundToInt(gridWorldSize.y/nodeDiameter);
    SpawnCubes();
    CreateGrid();

     
  }
    void Update(){
        if (Input.GetKeyUp(KeyCode.C)){
            foreach (GameObject obj in spawnedObjects){
            Destroy(obj);
            }
            spawnedObjects.Clear();
            SpawnCubes();
            CreateGrid();            
        }
        if (Input.GetKeyDown(KeyCode.C)){
            foreach (GameObject obj in spawnedObjects){
            Destroy(obj);
            }
            spawnedObjects.Clear();
            CleanGrid();
            CreateGrid();
        }
    }
   public int MaxSize{
    get{
        return gridSizeX*gridSizeY;
    }
  }
  void CreateGrid() {
		grid = new Node[gridSizeX,gridSizeY];
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x/2 - Vector3.forward * gridWorldSize.y/2;

		for (int x = 0; x < gridSizeX; x ++) {
			for (int y = 0; y < gridSizeY; y ++) {
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
				bool walkable = !Physics.CheckSphere(worldPoint,nodeRadius,unwalkableMask);
				grid[x,y] = new Node(walkable,worldPoint, x,y);
                Debug.Log(walkable+ "  " +worldPoint+ "  " + x+ "  " +y);
			}
		}
	}

    public void CleanGrid(){
        foreach (Node n in grid){
            n.walkable = true;
        }
        for (int i = 0; i < gridSizeX; i++){
            for (int j = 0; j < gridSizeY; j++){
                grid[i, j].walkable = true;
            }
        }
    }
  public Node NodeFromWorldPoint(Vector3 worldPosition){
    float percentX = (worldPosition.x + gridWorldSize.x/2)/gridWorldSize.x;
    float percentY = (worldPosition.z + gridWorldSize.y/2)/gridWorldSize.y;
    percentX = Mathf.Clamp01(percentX);
    percentY = Mathf.Clamp01(percentY);
    int x = Mathf.RoundToInt((gridSizeX - 1)*percentX);
    int y = Mathf.RoundToInt((gridSizeY - 1)*percentY);
    return grid[x,y];
  }
  public List<Node> GetNeighbours(Node node){
    List<Node> neighbours = new List<Node>();
    for(int i = -1; i <=1; i++) {
      for(int j = -1;j<=1;j++){
        if(i==0&&j==0)
          continue;
        int checkX = node.gridX + i;
        int CheckY = node.gridY + j;
        if(checkX>=0 && checkX<gridSizeX && CheckY>=0 && CheckY<gridSizeY)
          neighbours.Add(grid[checkX,CheckY]);
        }   
    }
    return neighbours;
  }

    void SpawnCubes(){
    Bounds outerBounds = outerPlane.GetComponent<Renderer>().bounds;
    float gapDistance = 15f; 
    int i = 0;
    while (i < obstacles)
    {
        Vector3 randomPosition = new Vector3(Random.Range(outerBounds.min.x+10, outerBounds.max.x-10),0,Random.Range(outerBounds.min.z+10, outerBounds.max.z-10));
        bool intersectsInnerPlane = false;
        foreach (GameObject innerPlane in innerPlanes)
        {
            if (innerPlane.GetComponent<Renderer>().bounds.Contains(randomPosition))
            {
                intersectsInnerPlane = true;
                break;
            }
        }
        bool withinOuterPlaneBounds = outerBounds.Contains(randomPosition);
        if (!intersectsInnerPlane && withinOuterPlaneBounds){
            bool hasEnoughGap = true;
            foreach (GameObject innerPlane in innerPlanes){
                float distanceToInnerPlane = Vector3.Distance(randomPosition, innerPlane.transform.position);
                if (distanceToInnerPlane < gapDistance){
                    hasEnoughGap = false;
                    break;
                }
            }
            if (hasEnoughGap){
                GameObject obj = Instantiate(cubePrefab, randomPosition, Quaternion.identity);
                i++;
                float randomSize = UnityEngine.Random.Range(8, 10);
                obj.transform.localScale = new Vector3(randomSize, randomSize, randomSize);
                spawnedObjects.Add(obj);
            }
        }
    }
}  
    void OnDrawGizmos() {
        Gizmos.DrawWireCube(transform.position,new Vector3(gridWorldSize.x,1,gridWorldSize.y)); 
        if(grid!=null&&pathGizmosShow){
            foreach(Node n in grid){
                Gizmos.color = n.walkable?Color.white:Color.blue;
                Gizmos.DrawCube(n.worldPosition,Vector3.one*(nodeDiameter-.1f));
            }
        }
    }

}