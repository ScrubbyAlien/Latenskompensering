using UnityEngine;

[CreateAssetMenu(menuName = "Infos/Court", fileName = "CourtInfo")]
public class CourtInfo : ScriptableObject
{
    [SerializeField]
    public Bounds ASide, BSide;
    [SerializeField]
    public float netHeight, ceilingHeight;
    
    private void OnValidate() {
        if (ceilingHeight < netHeight) ceilingHeight = netHeight;
    }

    public float GetRandomHeight() {
        return Random.Range(netHeight, ceilingHeight);
    }
    
    public Vector3 NetIntersection(Vector3 a, Vector3 b) {
        float distanceToIntersection = a.z * Vector3.Distance(a.XZ(), b.XZ()) / a.z + b.z;
        return a.XZ() + distanceToIntersection * (b.XZ() - a.XZ()).normalized;
    }
    
    /// true = A side, false = B side
    public Vector3 GetRandomPositionOnSide(bool aSide) {
        Bounds side = aSide ? ASide : BSide;
        return RandomPositionInBounds(side);
    }
    
    private static Vector3 RandomPositionInBounds(Bounds bounds) {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

}
