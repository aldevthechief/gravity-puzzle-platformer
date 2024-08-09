using UnityEngine;

public class MoveCamera : MonoBehaviour 
{
    public Transform playerhead;

    void Update() 
    {
        transform.position = playerhead.transform.position;
    }
}
