using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public bool invincible;
    public float invincibilityLength;
	
	// movement variables
    public float speed = 5f;
	enum Direction { UP, DOWN, LEFT, RIGHT }
	Direction direction = Direction.DOWN;
	bool walking = false;
	
	// parameters used for the camera controller
	// current room expressed as (x, y)
	ValueTuple<int, int> currentRoomAsTuple = (0, 0);
	Room currentRoom = new Room();
	public GameObject camera;
	public GameObject minimapRooms;
	public World world;

	// physics components
    public Vector2 mvt = new Vector2();
    Rigidbody2D rb2d;
    public Collider2D currentCollider;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
		updateCurrentRoom();
    }

    // Update is called once per frame
    void Update()
    {
		// adjust camera and room number if in a new room
		if (transform.position.x > 9 + 14*currentRoomAsTuple.Item1) {
			currentRoomAsTuple.Item1 += 1;
			camera.GetComponent<CameraController>().shiftEast();
			minimapRooms.transform.position = new Vector3((float)(minimapRooms.transform.position.x-0.5), minimapRooms.transform.position.y, minimapRooms.transform.position.z);
			updateCurrentRoom();
		}
		else if (transform.position.x < -5 + 14*currentRoomAsTuple.Item1) {
			currentRoomAsTuple.Item1 -= 1;
			camera.GetComponent<CameraController>().shiftWest();
			minimapRooms.transform.position = new Vector3((float)(minimapRooms.transform.position.x+0.5), minimapRooms.transform.position.y, minimapRooms.transform.position.z);
			updateCurrentRoom();
		}
		else if (transform.position.y > 5 + 10*currentRoomAsTuple.Item2) {
			currentRoomAsTuple.Item2 += 1;
			camera.GetComponent<CameraController>().shiftNorth();
			minimapRooms.transform.position = new Vector3(minimapRooms.transform.position.x, (float)(minimapRooms.transform.position.y-0.5), minimapRooms.transform.position.z);
			updateCurrentRoom();
		}
		else if (transform.position.y < -5 + 10*currentRoomAsTuple.Item2) {
			currentRoomAsTuple.Item2 -= 1;
			camera.GetComponent<CameraController>().shiftSouth();
			minimapRooms.transform.position = new Vector3(minimapRooms.transform.position.x, (float)(minimapRooms.transform.position.y+0.5), minimapRooms.transform.position.z);
			updateCurrentRoom();
		}
		
		// apply movement to the mvt vector
		if (!Input.GetKey("w") && !Input.GetKey("s"))
		{
			mvt.y = 0;
		}
		else if (Input.GetKey("w"))
		{
			direction = Direction.UP;
			mvt.y = 1;
			walking = true;
		}
		else if (Input.GetKey("s"))
		{
			direction = Direction.DOWN;
			mvt.y = -1;
			walking = true;
		}

		if (!Input.GetKey("a") && !Input.GetKey("d"))
		{
			mvt.x = 0;
		}
		else if (Input.GetKey("a"))
		{
			mvt.x = -1;
			direction = Direction.LEFT;
			walking = true;
		}
		else if (Input.GetKey("d"))
		{
			mvt.x = 1;
			direction = Direction.RIGHT;
			walking = true;
		}
		if (!Input.GetKey("w") && !Input.GetKey("s") && !Input.GetKey("a") && !Input.GetKey("d")) { walking = false; }

		// ...and apply the mvt force to the actual player's velocity
        MovePlayer(mvt);

		Animator anim = GetComponent<Animator>();
		
		// determine direction the sprite should be facing		
		switch (direction) {
			case Direction.UP:
				anim.SetInteger("direction", 0);
				break;
			case Direction.DOWN:
				anim.SetInteger("direction", 1);
				break;
			case Direction.LEFT:
				anim.SetInteger("direction", 2);
				break;
			case Direction.RIGHT:
				anim.SetInteger("direction", 3);
				break;
		}

        if (walking) { GetComponent<Animator>().SetBool("walking", true); }
        else { GetComponent<Animator>().SetBool("walking", false); }
    }

    public void MovePlayer(Vector2 v)
    {
		// diagonal mvt should be the same speed as vertical/horizontal
		if (v.x != 0 && v.y != 0) {
			v.x *= (float)(1/Math.Sqrt(2));
			v.y *= (float)(1/Math.Sqrt(2));
		}
		v.x *= speed;
		v.y *= speed;
        rb2d.velocity = v;
    }
	
	public GameObject roomLabel;
	void updateCurrentRoom(){
		currentRoom = world.getRoomFromPos(currentRoomAsTuple);
		roomLabel.GetComponent<Text>().text = currentRoom.getRoomName();
		world.discoverRoom(currentRoom);
	}

    public void GameOver()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        /* if (other.CompareTag("BrainBase") || other.CompareTag("BrainSpawn"))
        {
            touchingBrain = true;
        } */
    }

/*
    IEnumerator TemporaryInvincibility()
    {
        invincible = true;
        transform.Find("Sprite").GetComponent<SpriteRenderer>().color = Color.red;

        yield return new WaitForSeconds(invincibilityLength);

        transform.Find("Sprite").GetComponent<SpriteRenderer>().color = Color.white;
        invincible = false;
    } */

    void OnTriggerExit2D(Collider2D other)
    {
		/*
        if (other.CompareTag("BrainBase") || other.CompareTag("BrainSpawn"))
        {
            touchingBrain = false;
        }*/
    }
}
