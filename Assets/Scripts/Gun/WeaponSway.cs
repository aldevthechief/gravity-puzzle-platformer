using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    public float amount;
    public float maxamount;
    public float smoothamount;
    private Vector3 initialPos;
    void Start()
    {
        initialPos = transform.localPosition;
    }

    void Update()
    {
        Vector2 mousemovement = new Vector2(-Input.GetAxis("Mouse X") * amount, -Input.GetAxis("Mouse Y") * amount);
        Vector2 movement = new Vector2(-Input.GetAxis("Horizontal") * amount, -Input.GetAxis("Vertical") * amount);
        mousemovement = new Vector2(Mathf.Clamp(mousemovement.x, -maxamount, maxamount), Mathf.Clamp(mousemovement.y, -maxamount, maxamount));
        movement = new Vector2(Mathf.Clamp(movement.x, -maxamount, maxamount), Mathf.Clamp(movement.y, -maxamount, maxamount));
        Vector3 finalPosition = new Vector3(mousemovement.x, mousemovement.y, 0);
        Vector3 finalPosition1 = new Vector3(movement.x, movement.y, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition + initialPos, Time.fixedDeltaTime * smoothamount);
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition1 + initialPos, Time.fixedDeltaTime * smoothamount);
    }
}
