using UnityEngine;
using System.Collections.Generic;

public class SeekerManager : MonoBehaviour
{
   
    private List<Seeker> seekers = new List<Seeker>();

    // Method to add seeker to the list
    public void AddSeeker(Seeker seeker)
    {
        seekers.Add(seeker);
    }

    public void SetActiveSeeker(Seeker activeSeeker)
    {
        foreach (Seeker seeker in seekers)
        {
            seeker.isActive = (seeker == activeSeeker);
        }
    }
}
