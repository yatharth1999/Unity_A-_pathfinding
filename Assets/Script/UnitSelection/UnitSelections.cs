using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelections : MonoBehaviour
{
    public List<GameObject> unitList = new List<GameObject>();
    public List<GameObject> unitSelected = new List<GameObject>();

    private static UnitSelections _instance;
    public static UnitSelections Instance {
        get{
            return _instance;
        }
    }

    void Awake() {
        if (_instance != null && (_instance != this)){
            Destroy(this.gameObject);
        }
        else{
            _instance = this;
        }
    }
    void Update(){
        if (Input.GetKey(KeyCode.L)){
            DeselectAll();
        }
    }
    public void ClickSelect (GameObject unitToAdd){
        DeselectAll();
        unitSelected.Add(unitToAdd);
        unitToAdd.transform.GetChild(2).gameObject.SetActive(true);
        Seeker seekerComponent = unitToAdd.GetComponent<Seeker>();
        if (seekerComponent != null) {
        
        seekerComponent.isActive = true;
        
    }
        PotentialField potential = unitToAdd.GetComponent<PotentialField>();
            if (potential != null) {
        
            potential.isActive = true;
        }
        PFAstar pfa = unitToAdd.GetComponent<PFAstar>();
            if (pfa != null) {
        
            pfa.isActive = true;
        }
    }
    public void ControlClickSelect (GameObject unitToAdd){
        if(!unitSelected.Contains(unitToAdd)){
            unitSelected.Add(unitToAdd);
            unitToAdd.transform.GetChild(2).gameObject.SetActive(true);
            Seeker seekerComponent = unitToAdd.GetComponent<Seeker>();
            if (seekerComponent != null) {
        
            seekerComponent.isActive = true;
            }
            PotentialField potential = unitToAdd.GetComponent<PotentialField>();
            if (potential != null) {
        
            potential.isActive = true;
            }
            PFAstar pfa = unitToAdd.GetComponent<PFAstar>();
            if (pfa != null) {
        
            pfa.isActive = true;
        }
            
        }
        else{
            unitToAdd.transform.GetChild(2).gameObject.SetActive(false);
            unitSelected.Remove(unitToAdd);
            Seeker seekerComponent = unitToAdd.GetComponent<Seeker>();
            if (seekerComponent != null) {
        
            seekerComponent.isActive = false; 
            }
            PotentialField potential = unitToAdd.GetComponent<PotentialField>();
            if (potential != null) {
        
            potential.isActive = false;
            }
            PFAstar pfa = unitToAdd.GetComponent<PFAstar>();
            if (pfa != null) {
        
            pfa.isActive = false;
        }
        }
    }
    public void DragSelect(GameObject unitToAdd){
        if(!unitSelected.Contains(unitToAdd)){
            unitSelected.Add(unitToAdd);
            unitToAdd.transform.GetChild(2).gameObject.SetActive(true);
            Seeker seekerComponent = unitToAdd.GetComponent<Seeker>();
            if (seekerComponent != null) {
        
            seekerComponent.isActive = true;
            }            
            
            PotentialField potential = unitToAdd.GetComponent<PotentialField>();
            if (potential != null) {
        
            potential.isActive = true;
            }
            PFAstar pfa = unitToAdd.GetComponent<PFAstar>();
            if (pfa != null) {
        
            pfa.isActive = true;
            }
        }
    }
    public void DeselectAll(){
        foreach(var unit in unitSelected){
            unit.transform.GetChild(2).gameObject.SetActive(false);
            Seeker seekerComponent = unit.GetComponent<Seeker>();
            if (seekerComponent != null) {
                seekerComponent.isActive = false;
                
            }
            PotentialField potential = unit.GetComponent<PotentialField>();
            if (potential != null) {
        
            potential.isActive =false;
            }
            PFAstar pfa = unit.GetComponent<PFAstar>();
            if (pfa != null) {
        
            pfa.isActive = false;
            }
        }
        unitSelected.Clear();
    }
    public void Deselect(GameObject unitToDeslect){
        
    }
}