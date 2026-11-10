using UnityEngine;

public class Court : MonoBehaviour
{
    [SerializeField]
    private CourtInfo courtInfo;
    
    private void OnDrawGizmos() {
        Gizmos.color = Color.darkRed;
        Gizmos.DrawWireCube(courtInfo.ASide.center, courtInfo.ASide.size);
        Gizmos.color = Color.cornflowerBlue;
        Gizmos.DrawWireCube(courtInfo.BSide.center, courtInfo.BSide.size);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(new Vector3(-7.5f, courtInfo.netHeight, 0), new Vector3(7.5f, courtInfo.netHeight, 0));
        Gizmos.DrawLine(new Vector3(-7.5f, courtInfo.ceilingHeight, 0), new Vector3(7.5f, courtInfo.ceilingHeight, 0));
    }
}
