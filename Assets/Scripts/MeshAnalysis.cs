using UnityEngine;

public class MeshAnalysisTool : MonoBehaviour
{
    public int maxDepth = 4; 

    void OnDrawGizmosSelected()
    {
        var r = GetComponent<Renderer>();
        if (r == null) return;

        Bounds bounds = r.bounds;
        Gizmos.color = Color.green;
        
        DrawOctree(bounds.center, bounds.size, 0);
    }

    void DrawOctree(Vector3 center, Vector3 size, int depth)
    {
        if (depth >= maxDepth) return;

        Gizmos.DrawWireCube(center, size);

        Vector3 newSize = size / 2f;
        int nextDepth = depth + 1;

        DrawOctree(center + new Vector3(newSize.x/2, newSize.y/2, newSize.z/2), newSize, nextDepth);
    }
}