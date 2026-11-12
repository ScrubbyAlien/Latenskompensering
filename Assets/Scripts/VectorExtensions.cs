using UnityEngine;

public static class VectorExtensions
{
    public static Vector3 XZ(this Vector3 vector3) {
        return new Vector3(vector3.x, 0, vector3.z);
    }
    public static Vector2 XZ2(this Vector3 vector3) {
        Vector3 xz = vector3.XZ();
        return new Vector2(xz.x, xz.z);
    }                                
}
