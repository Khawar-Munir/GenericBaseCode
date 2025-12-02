using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class Key : Editor
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [MenuItem("My Commands/GameManager _a")]
    static void pingAccessibleObjectHandler()
    {
        //Debug.Log("shift + c pressed to ping all canvases object");

        //Object Game_Manager_Object = GameObject.FindFirstObjectByType<Game_Manager>();
        //if (Game_Manager_Object)
        //{
        //    pingObject(Game_Manager_Object);
        //}
        //else
        //{
        //    Debug.LogError("Game_Manager object could not be found!!!");
        //}
    }
    public static void pingObject(Object obj)
    {
#if UNITY_EDITOR
        EditorGUIUtility.PingObject(obj);
        Selection.activeGameObject = obj as GameObject;
#endif
    }


    [MenuItem("My Commands/keyboard control _&k")]
    static void enableKeyboard()
    {
        //Debug.Log("shift + c pressed to ping all canvases object");

        //RCCCarControllerV2 rcc_car_controller = GameObject.FindFirstObjectByType<RCCCarControllerV2>();
        //if (rcc_car_controller)
        //{
        //    rcc_car_controller.mobileController=false;
        //    Debug.Log("control switched to keyboard");
        //}
        //else
        //{
        //    Debug.LogError("Game_Manager object could not be found!!!");
        //}
    }
    [MenuItem("My Commands/Reset All _&r")]
    static void resetData()
    {
        //Debug.Log("shift + c pressed to ping all canvases object");

        //Menu_Manager.instance?.ResetAllAndClear3Levels();
        //Menu_Manager.instance?.ResetAll();
    }

    [MenuItem("My Commands/complete _&c")]
    static void levelComplete()
    {
        //Debug.Log("shift + c pressed to ping all canvases object");
        Debug.Log("level completed with shortcut...");
        //Menu_Manager.instance?.ResetAllAndClear3Levels();
        //Menu_Manager.instance?.ResetAll();
        //Game_Manager.instance.current_Status = Game_Manager.GameStatus.Game_Complete;
    }
}
