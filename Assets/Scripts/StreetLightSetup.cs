#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class StreetLightAutoSetup
{
    [MenuItem("Tools/Street Lights/Add Point Lights to Pole_light")]
    public static void AddLights()
    {
        int added = 0;
        var allTransforms = Object.FindObjectsOfType<Transform>(true);

        foreach (var t in allTransforms)
        {
            if (!t.name.Equals("Pole_light")) continue;

            if (t.GetComponentInChildren<Light>(true) != null) continue;

            var go = new GameObject("StreetLight_Point");
            go.transform.SetParent(t, false);

            go.transform.localPosition = new Vector3(0f, 2.8f, 0f);

            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.range = 15f;
            l.intensity = 1500f;
            l.color = new Color(1f, 0.92f, 0.75f);

            Undo.RegisterCreatedObjectUndo(go, "Add Street Light");
            added++;
        }
    }
}
#endif
