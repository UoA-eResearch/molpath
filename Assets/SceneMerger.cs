using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneMerger
{
    public static void CompareDifferences()
    {
        // iterate all game objs in scene:
        GameObject[] sceneGos = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject go in sceneGos)
        {
            Debug.Log(go.name);
        }
        SceneManager.LoadScene(sceneName: "Scene 00");


    }

    // Update is called once per frame
}
