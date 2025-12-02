#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class BulkCopyVehicleComponents : EditorWindow
{
    [Header("Source prefab with tuned components")]
    [SerializeField] GameObject sourcePrefab;

    [SerializeField] bool overwriteExisting = true;
    [SerializeField] bool autoAssignWheelColliders = true;

    // Add/keep the components you want to copy
    static readonly Type[] kTypes =
    {
        //typeof(PathYawAssistSpline),
        //typeof(TrackKeeperStrictSpline),
        //typeof(RollStabilityController),
        //typeof(AirStabilizerLiteSpline),
        //typeof(AntiRollBars),
    };

    [MenuItem("Tools/CarStunts/Bulk Copy Vehicle Components...")]
    public static void Open() => GetWindow<BulkCopyVehicleComponents>("Bulk Copy Vehicle Components");

    void OnGUI()
    {
        EditorGUILayout.Space();
        sourcePrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Source Prefab"), sourcePrefab, typeof(GameObject), false);
        overwriteExisting = EditorGUILayout.ToggleLeft("Overwrite Existing Components", overwriteExisting);
        autoAssignWheelColliders = EditorGUILayout.ToggleLeft("Auto-assign WheelColliders (FL/FR/RL/RR)", autoAssignWheelColliders);

        EditorGUILayout.Space(6);
        using (new EditorGUI.DisabledScope(sourcePrefab == null || GetSelectedPrefabPaths().Count == 0))
        {
            if (GUILayout.Button($"Run on Selected Prefabs ({GetSelectedPrefabPaths().Count})", GUILayout.Height(32)))
                Run();
        }

        EditorGUILayout.HelpBox("Select target car prefabs in the Project window, then press the button.", MessageType.Info);
    }

    void Run()
    {
        var targets = GetSelectedPrefabPaths();
        if (sourcePrefab == null) { ShowNotification(new GUIContent("Pick a source prefab.")); return; }
        if (targets.Count == 0) { ShowNotification(new GUIContent("Select target prefabs in Project.")); return; }

        string srcPath = AssetDatabase.GetAssetPath(sourcePrefab);
        if (string.IsNullOrEmpty(srcPath)) { ShowNotification(new GUIContent("Source must be a prefab asset.")); return; }

        var srcRoot = PrefabUtility.LoadPrefabContents(srcPath);
        try
        {
            // Sanity: ensure source has each component at least once
            foreach (var t in kTypes)
                if (srcRoot.GetComponent(t) == null)
                    Debug.LogWarning($"[BulkCopy] Source is missing component: {t.Name}");

            int done = 0;
            foreach (var path in targets)
            {
                var dstRoot = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    TransferAll(srcRoot, dstRoot);
                    if (autoAssignWheelColliders)
                        TryAutoAssignWheels(dstRoot);

                    PrefabUtility.SaveAsPrefabAsset(dstRoot, path);
                    done++;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[BulkCopy] Failed on {path}\n{e}");
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(dstRoot);
                }
            }

            Debug.Log($"[BulkCopy] Updated {done} prefab(s).");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(srcRoot);
        }

        AssetDatabase.Refresh();
    }

    static void TransferAll(GameObject src, GameObject dst)
    {
        foreach (var t in kTypes)
        {
            var srcComp = src.GetComponent(t);
            if (!srcComp) continue;

            var dstComp = dst.GetComponent(t);
            if (!dstComp) dstComp = dst.AddComponent(t);

            // Copy serialized fields exactly as set on source
            ComponentUtility.CopyComponent(srcComp);
            ComponentUtility.PasteComponentValues(dstComp);
            EditorUtility.SetDirty(dstComp);
        }
    }

    static void TryAutoAssignWheels(GameObject root)
    {
        var wheels = root.GetComponentsInChildren<WheelCollider>(true);
        if (wheels == null || wheels.Length < 4) return;

        // Guess FL/FR/RL/RR by local position (z: front+, x: right+)
        var list = new List<(WheelCollider wc, Vector3 local)>(wheels.Length);
        foreach (var w in wheels)
            list.Add((w, root.transform.InverseTransformPoint(w.transform.position)));

        var frontPair = list.OrderByDescending(x => x.local.z).Take(2).OrderBy(x => x.local.x).ToArray();
        var rearPair = list.OrderBy(x => x.local.z).Take(2).OrderBy(x => x.local.x).ToArray();
        var FL = frontPair.First().wc; var FR = frontPair.Last().wc;
        var RL = rearPair.First().wc; var RR = rearPair.Last().wc;

        // AntiRollBars: struct fields -> copy, edit, assign back
        //var arb = root.GetComponent<AntiRollBars>();
        //if (arb)
        //{
        //    var front = arb.front; front.left = FL; front.right = FR; arb.front = front;
        //    var rear = arb.rear; rear.left = RL; rear.right = RR; arb.rear = rear;
        //    EditorUtility.SetDirty(arb);
        //}

        // RollStabilityController
        //var rsc = root.GetComponent<RollStabilityController>();
        //if (rsc)
        //{
        //    rsc.frontLeft = FL; rsc.frontRight = FR; rsc.rearLeft = RL; rsc.rearRight = RR;
        //    EditorUtility.SetDirty(rsc);
        //}

        // Wheel arrays on assist scripts
        var ordered = new WheelCollider[] { FL, FR, RL, RR };

        //var py = root.GetComponent<PathYawAssistSpline>();
        //if (py) { py.wheelColliders = ordered; EditorUtility.SetDirty(py); }

        //var tk = root.GetComponent<TrackKeeperStrictSpline>();
        //if (tk) { tk.wheelColliders = ordered; EditorUtility.SetDirty(tk); }

        //var air = root.GetComponent<AirStabilizerLiteSpline>();
        //if (air) { air.wheelColliders = ordered; EditorUtility.SetDirty(air); }
    }

    static List<string> GetSelectedPrefabPaths()
    {
        var paths = new List<string>();
        foreach (var obj in Selection.objects)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path)) continue;
            var type = PrefabUtility.GetPrefabAssetType(obj);
            if (type != PrefabAssetType.NotAPrefab) paths.Add(path);
        }
        return paths;
    }
}
#endif
