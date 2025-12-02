using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

//*123 load scene by short cut keys and build index
public class QuickSceneLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [MenuItem("My Commands/scene 0 #0")]
    static void scene0()
    {
        Debug.Log("shift + 0 pressed");
        //EditorSceneManager.OpenScene("Assets/0_a/Scenes/SplashScene.unity");
        getAndOpenSceneAtIndex(0);
    }
    [MenuItem("My Commands/scene 1 #1")]
    static void scene1()
    {
        Debug.Log("shift + 1 pressed");
        //EditorSceneManager.OpenScene("Assets/0_a/Scenes/NewGameplay_ORG.unity");
        getAndOpenSceneAtIndex(1);
    }

    [MenuItem("My Commands/scene 2 #2")]
    static void scene2()
    {
        Debug.Log("shift + 2 pressed");
        //EditorSceneManager.OpenScene("Assets/0_a/Scenes/Guns And Inapps.unity");
        getAndOpenSceneAtIndex(2);
    }

    [MenuItem("My Commands/scene 3 #3")]
    static void scene3()
    {
        Debug.Log("shift + 3 pressed");
        ////EditorSceneManager.OpenScene("Assets/0_a/Scenes/GamePlayData Loading Scene.unity");
        getAndOpenSceneAtIndex(3);
    }

    [MenuItem("My Commands/scene 4 #4")]
    static void scene4()
    {
        Debug.Log("shift + 4 pressed");
        //EditorSceneManager.OpenScene("Assets/0_a/Scenes/Map_v2_.unity");
        getAndOpenSceneAtIndex(4);
    }

    [MenuItem("My Commands/scene 5 #5")]
    static void scene5()
    {
        Debug.Log("shift + 5 pressed");
        //EditorSceneManager.OpenScene("Assets/0_a/Scenes/TDM Mode.unity");
        getAndOpenSceneAtIndex(5);
    }

    [MenuItem("My Commands/scene 6 #6")]
    static void scene6()
    {
        Debug.Log("shift + 6 pressed");
        //EditorSceneManager.OpenScene("Assets/0_a/Scenes/TDM Mode 1.unity");
        getAndOpenSceneAtIndex(6);
    }

    [MenuItem("My Commands/scene 7 #7")]
    static void scene7()
    {
        Debug.Log("shift + 7 pressed");
        //EditorSceneManager.OpenScene("Assets/0_a/Scenes/TDM Mode 3.unity");
        getAndOpenSceneAtIndex(7);
    }

    [MenuItem("My Commands/scene 8 #8")]
    static void scene8()
    {
        Debug.Log("shift + 8 pressed");
        //EditorSceneManager.OpenScene("Assets/0_a/Scenes/TDM Mode 2.unity");
        getAndOpenSceneAtIndex(8);
    }

    [MenuItem("My Commands/scene 9 #9")]
    static void scene9()
    {
        Debug.Log("shift + 9 pressed");
        getAndOpenSceneAtIndex(9);
    }

    //[MenuItem("My Commands/scene 9 #9")]
    //static void scene9()
    //{
    //    Debug.Log("shift + 9 pressed");
    //    getAndOpenSceneAtIndex(9);
    //}

    //*123 getting scene through build index in build settings...
    [MenuItem("My Commands/scene 10 &0")]
    static void scene10()
    {

        Debug.Log("alt + 0 pressed");


        getAndOpenSceneAtIndex(10);
        //EditorSceneManager.OpenScene("Assets/0_a/Scenes/Ads Scene.unity");
    }

    [MenuItem("My Commands/scene 11 &1")]
    static void scene11()
    {
        Debug.Log("alt + 11 pressed");
        getAndOpenSceneAtIndex(11);
    }

    [MenuItem("My Commands/scene 12 &2")]
    static void scene12()
    {
        Debug.Log("alt + 12 pressed");
        getAndOpenSceneAtIndex(12);
    }

    [MenuItem("My Commands/scene 13 &3")]
    static void scene13()
    {
        Debug.Log("alt + 13 pressed");
        getAndOpenSceneAtIndex(13);
    }

    [MenuItem("My Commands/scene 14 &4")]
    static void scene14()
    {
        Debug.Log("alt + 14 pressed");
        getAndOpenSceneAtIndex(14);
    }

    [MenuItem("My Commands/scene 15 &5")]
    static void scene15()
    {
        Debug.Log("alt + 15 pressed");
        getAndOpenSceneAtIndex(15);
    }

    [MenuItem("My Commands/scene 16 &6")]
    static void scene16()
    {
        Debug.Log("alt + 16 pressed");
        getAndOpenSceneAtIndex(16);
    }

    [MenuItem("My Commands/scene 17 &7")]
    static void scene17()
    {
        Debug.Log("alt + 17 pressed");
        getAndOpenSceneAtIndex(17);
    }


    [MenuItem("My Commands/scene 18 &8")]
    static void scene18()
    {
        Debug.Log("alt + 18 pressed");
        getAndOpenSceneAtIndex(18);
    }

    [MenuItem("My Commands/scene 19 &9")]
    static void scene19()
    {
        Debug.Log("alt + 19 pressed");
        getAndOpenSceneAtIndex(19);
    }

    [MenuItem("My Commands/scene 20 &#0")]
    static void scene20()
    {
        Debug.Log("alt+shift + 20 pressed");
        getAndOpenSceneAtIndex(20);
    }

    [MenuItem("My Commands/scene 21 &#1")]
    static void scene21()
    {
        Debug.Log("alt+shift + 21 pressed");
        getAndOpenSceneAtIndex(21);
    }

    [MenuItem("My Commands/scene 22 &#2")]
    static void scene22()
    {
        Debug.Log("alt+shift + 22 pressed");
        getAndOpenSceneAtIndex(22);
    }

    [MenuItem("My Commands/scene 23 &#3")]
    static void scene23()
    {
        Debug.Log("alt+shift + 23 pressed");
        getAndOpenSceneAtIndex(23);
    }

    [MenuItem("My Commands/scene 24 &#4")]
    static void scene24()
    {
        Debug.Log("alt+shift + 24 pressed");
        getAndOpenSceneAtIndex(24);
    }
    public static void pingTestModeObject()
    {
        //GamePlayTesting tmo = GameObject.FindObjectOfType<GamePlayTesting>(true);


//        if (tmo)
//        {
//#if UNITY_EDITOR
//            Debug.Log("alt + t pressed to ping Test Mode object");
//            EditorGUIUtility.PingObject(tmo);
//            Selection.activeGameObject = tmo.gameObject;
//#endif
//        }

    }

   

    static void getAndOpenSceneAtIndex(int index)
    {
        var path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(index);
        Debug.Log("requested scene path: " + path);
        EditorSceneManager.OpenScene(path);
    }


    [MenuItem("My Commands/currentScene #&c")]
    static void sceneCurrent()
    {

        var path = EditorSceneManager.GetActiveScene().path;
        Debug.Log("Active scene path: " + path);
        //var scenePathProperty = edtr.serializedObject.FindProperty("scenePath");
        //scenePathProperty.stringValue = path;
        Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
        Selection.activeObject = obj;

        EditorGUIUtility.PingObject(obj);
        //var scenePathProperty = serializedObject.FindProperty("scenePath");
        //var newPath = AssetDatabase.GetAssetPath(path);

    }
}
