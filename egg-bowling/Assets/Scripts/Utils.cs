using UnityEngine;

public class Utils : MonoBehaviour
{
    public static Vector3 ScreenToWorld(Camera camera, Vector3 position)
    {
        position.z = camera.nearClipPlane;
        // position.z = 6.5f;
        return camera.ScreenToWorldPoint(position);
    }

    public void Something()
    {
        
    }
}