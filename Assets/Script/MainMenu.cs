// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// public class MainMenu : MonoBehaviour
// {
//    public void Play () {
//         // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
//    }
//    public void Quit () {
//     Application.Quit();
//    }
// }


using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    // Use two variables to track the last pressed button in each set.
    private int lastPressedSet1 = 0; // For buttons 1-4 (formerly A-D)
    private int lastPressedSet2 = 0; // For buttons 1-8

    // Map combinations of Set1 and Set2 buttons to scene names.
    private int[,] sceneMap = new int[4, 8]; // 4 buttons in set 1, 8 buttons in set 2

    void Start()
    {
        int z = 1;
        for (int i = 0; i < 4; i++)
        {
            for(int j = 0; j < 8; j++) {
                if((i==2&j==3)||(i==2&j==7)){
                    continue;
                }
                sceneMap[i,j]=z;
                // Debug.Log(i+"  "+j+"  "+z);
                z++;
            }
        }

        }

    // Method for buttons 1-4 (formerly A-D)
    public void Set1ButtonPressed(int buttonId)
    {
        lastPressedSet1 = buttonId;
        CheckCombination();
    }

    // Method for buttons 1-8
    public void Set2ButtonPressed(int buttonId)
    {
        lastPressedSet2 = buttonId;
        CheckCombination();
    }

    private void CheckCombination()
    {
        if (lastPressedSet1 > 0 && lastPressedSet2 > 0)
        {
            // Adjust indices to match zero-based array indexing
            int index1 = lastPressedSet1 - 1;
            int index2 = lastPressedSet2 - 1;

            int sceneName = sceneMap[index1, index2];

            if (sceneName>0)
            {
                SceneManager.LoadScene(sceneName);
            }

            // Reset after checking
            lastPressedSet1 = 0;
            lastPressedSet2 = 0;
        }
    }
}
