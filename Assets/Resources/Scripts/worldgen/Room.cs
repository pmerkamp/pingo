using System;
using UnityEngine;

public class Room
{
	private int xpos = 0;
	private int ypos = 0;
	public ValueTuple<int, int> roomPosition;
	public string name;
	public bool discovered = false;
	
	public GameObject roomGO;
	
	public Room() { }
	
	public Room(string n, int x, int y) {
		name = n;
		xpos = x;
		ypos = y;
		setRoomPos((xpos, ypos));
	}
	public Room(string n, int x, int y, Room north, Room south, Room east, Room west) {
		name = n;
		xpos = x;
		ypos = y;
		setRoomPos((xpos, ypos));
		northRoom = north;
		southRoom = south;
		eastRoom = east;
		westRoom = west;
	}
		
	public enum RoomType {
		SPAWN,
		BLANK,
		ENDPOINT,
		UNIQUE_A,
		UNIQUE_B,
		UNIQUE_C
	}
	
	public RoomType type;
	
	public Room northRoom;
	public Room southRoom;
	public Room eastRoom;
	public Room westRoom;
	
	public void setType(RoomType rt){ type = rt; }
	
	public void setNorthRoom(Room r){ northRoom = r; }
	public void setSouthRoom(Room r){ southRoom = r; }
	public void setWestRoom(Room r){ westRoom = r; }
	public void setEastRoom(Room r){ eastRoom = r; }
	
	public Room getNorthRoom(){ return northRoom; }
	public Room getSouthRoom(){ return southRoom; }
	public Room getWestRoom(){ return westRoom; }
	public Room getEastRoom(){ return eastRoom; }
	
	public ValueTuple<int, int> getRoomPos(){ return roomPosition; }
	public int distanceFromSpawn = 0;
	
	public void setRoomPos(ValueTuple<int, int> pos) {
		xpos = pos.Item1;
		ypos = pos.Item2;
		roomPosition = (xpos, ypos);
		
		distanceFromSpawn = Math.Abs(xpos) + Math.Abs(ypos); // TODO: maybe do some pathfinding to make this true distance
		setRoomRarity();
	}

	public int getRoomPosX() { return roomPosition.Item1; }
	public int getRoomPosY() { return roomPosition.Item2; }
	
	public string getRoomName() { return name; }
	public void setRoomName(string n) { name = n; }
	
	public int[,] generateTileArray(){
		// 1's are walls, 0's are floor
		int doNorth;
		int doSouth;
		int doEast;
		int doWest;
		
		if (getNorthRoom() == null){
			doNorth = 1;
		}
		else { doNorth = 0; }
		if (getSouthRoom() == null){
			doSouth = 1;
		}
		else { doSouth = 0; }
		if (getEastRoom() == null){
			doEast = 1;
		}
		else { doEast = 0; }
		if (getWestRoom() == null){
			doWest = 1;
		}
		else { doWest = 0; }
		
		int[,] tileArray = new int[10,14] {
		{ 1, 1, 1, 1, 1, 1, doSouth, doSouth, 1, 1, 1, 1, 1, 1 },
		{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
		{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
		{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
		{ doWest, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, doEast },
		{ doWest, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, doEast },
		{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
		{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
		{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
		{ 1, 1, 1, 1, 1, 1, doNorth, doNorth, 1, 1, 1, 1, 1, 1 }
		};
		
		return tileArray;
	}
	
	// the following is only relevant to item generation in the room
	
	// item rarities in this room
	// the following are percentages - not # of chances inside global max rarity
	// if determiner is less than 100-(rare+epic+legendary+mythic) it generates as common
	public int itemdist_rare; // % chance of an item generating as rare
	public int itemdist_epic; // % chance of an item generating as epic
	public int itemdist_legendary; // % chance of an item generating as legendary
	public int itemdist_root; // % chance of an item generating as root
	
	// this is where to fiddle with rarity settings
	public void setRoomRarity(){
		if (distanceFromSpawn <= 10){ doRarity(23, 2, 0, 0); }
		else if (distanceFromSpawn <= 20){ doRarity(25, 4, 1, 0); }
		else if (distanceFromSpawn <= 30){ doRarity(30, 8, 2, 0); }
		else { doRarity(30, 10, 4, 1); }
	}
	public void doRarity(int r1, int e, int l, int r2){
		itemdist_rare = r1;
		itemdist_epic = e;
		itemdist_legendary = l;
		itemdist_root = r2;
	}
}