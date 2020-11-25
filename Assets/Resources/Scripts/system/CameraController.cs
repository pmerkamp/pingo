using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{	
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	public void shiftNorth(){ transform.position = new Vector3(transform.position.x, transform.position.y+10, -10); }
	public void shiftSouth(){ transform.position = new Vector3(transform.position.x, transform.position.y-10, -10); }
	public void shiftEast(){ transform.position = new Vector3(transform.position.x+14, transform.position.y, -10); }
	public void shiftWest(){ transform.position = new Vector3(transform.position.x-14, transform.position.y, -10); }
}
