using UnityEngine;

public class RotateController : MonoBehaviour
{
    public Transform tamXoay;
    public float speed; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(tamXoay.position, Vector3.forward, speed * Time.deltaTime);
    }
}
