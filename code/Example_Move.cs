using UnityEngine;

public class PlatformMoveX : MonoBehaviour
{
    public float scaleFactor = 0.001f;
    private float baseX;

    private Vector3 targetPosition;
    private bool hasTarget = false;

    void Start()
    {
        baseX = transform.localPosition.x;
        targetPosition = transform.localPosition;
    }

    public void setCoordinates(string coord)
    {
        Debug.Log(coord);
        if (float.TryParse(coord, out float mcx))
        {
            float xCoord = mcx * scaleFactor + baseX;
            targetPosition = new Vector3(xCoord, transform.localPosition.y, transform.localPosition.z);
            hasTarget = true;
        }
    }

    void Update()
    {
        if (hasTarget)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * 5f);
        }
    }
}
