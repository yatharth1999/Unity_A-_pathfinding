
using UnityEngine;

public class UnitClick : MonoBehaviour
{ 
    public LayerMask clickable;
    public LayerMask ground;
    void Update()
    {
        if(Input.GetMouseButtonDown(0)){
            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickable)) {
                
                if(Input.GetKey(KeyCode.LeftControl)){
                    UnitSelections.Instance.ControlClickSelect(hit.collider.gameObject);     
                }
                else{
                    UnitSelections.Instance.ClickSelect(hit.collider.gameObject);                
                }
            
            
        }
        
    }
}
}
