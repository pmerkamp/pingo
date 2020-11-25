using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Linq;

public class World : MonoBehaviour
{
	List<Room> rooms = new List<Room>();
	List<(int, int)> occupiedSpaces = new List<(int, int)>();
	
	ValueTuple<int, int> possiblePos = (0, 0);

	// custom values regarding world generation
	public int desiredAttempts = 20; // number of times it'll try to make a room before it gives up
	public int curlyRandomness = 50; // percent chance a branch will deviate from a straight path per iteration
	public int branchLength = 8;
	public int branchChance = 5;
	public int budChance = 10;
	public int connectChance = 20; // % chance on EACH potential new connection
	
	public GameObject roomPrefab; // room prefab
	public GameObject minimap;
	
	// custom values regarding difficulty, item generation, etc
	public int maxRarity = 100; // item prices scale based on this best possible value
	
    // Start is called before the first frame update
    void Start()
    {
        generateWorld(branchLength);
		
		createConnections();
		
		for (int i=0; i<rooms.Count; i++){
			Vector2 roomPosition = new Vector2((float)(rooms[i].getRoomPosX()*0.5), (float)(rooms[i].getRoomPosY()*0.5));
			GameObject roomObj = Instantiate(roomPrefab);
			rooms[i].roomGO = roomObj;
			roomObj.transform.parent = minimap.transform.GetChild(0).transform.GetChild(0);
			roomObj.transform.localPosition = roomPosition;
			if (rooms[i].getNorthRoom() == null) { roomObj.transform.GetChild(1).GetComponent<SpriteRenderer>().enabled = false; }
			if (rooms[i].getEastRoom() == null) { roomObj.transform.GetChild(2).GetComponent<SpriteRenderer>().enabled = false; }
			if (rooms[i].getSouthRoom() == null) { roomObj.transform.GetChild(3).GetComponent<SpriteRenderer>().enabled = false; }
			if (rooms[i].getWestRoom() == null) { roomObj.transform.GetChild(4).GetComponent<SpriteRenderer>().enabled = false; }
			roomObj.SetActive(false);
		}
		
		Room spawn = getRoomFromPos((0,0));
		spawn.setType(Room.RoomType.SPAWN);
		spawn.setNorthRoom(getRoomFromPos((0,1)));
		spawn.setSouthRoom(getRoomFromPos((0,-1)));
		spawn.setEastRoom(getRoomFromPos((1,0)));
		spawn.setWestRoom(getRoomFromPos((-1,0)));
		
		doCleanup();
				
		for (int i=0; i<rooms.Count; i++){
			drawRoom(rooms[i]);
		}
		
		fixSpawn();
    }

    // Update is called once per frame
    void Update()
    {
		
    }
	
	enum Direction {
		NORTH,
		EAST,
		SOUTH,
		WEST,
		NONE
	}
		
	System.Random rnd = new System.Random();
	
	void generateWorld(int branchSize){
		Room spawn = new Room("Spawn", 0, 0);
		Room spawnNorth = new Room("North 1", 0, 1);
		spawnNorth.setSouthRoom(spawn);
		Room spawnSouth = new Room("South 1", 0, -1);
		spawnSouth.setNorthRoom(spawn);
		Room spawnEast = new Room("East 1", 1, 0);
		spawnEast.setWestRoom(spawn);
		Room spawnWest = new Room("West 1", -1, 0);
		spawnWest.setEastRoom(spawn);
		
		spawn.setNorthRoom(spawnNorth);
		spawn.setSouthRoom(spawnSouth);
		spawn.setEastRoom(spawnEast);
		spawn.setWestRoom(spawnWest);
		spawnNorth.setSouthRoom(spawn);
		spawnSouth.setNorthRoom(spawn);
		spawnEast.setWestRoom(spawn);
		spawnWest.setEastRoom(spawn);
		
		occupiedSpaces.Add((0, 0));
		occupiedSpaces.Add((0, 1));
		occupiedSpaces.Add((0, -1));
		occupiedSpaces.Add((1, 0));
		occupiedSpaces.Add((-1, 0));
		
		rooms.Add(spawn);
		rooms.Add(spawnNorth);
		rooms.Add(spawnSouth);
		rooms.Add(spawnEast);
		rooms.Add(spawnWest);
			
		generateBranch(branchSize, Direction.NORTH, spawnNorth, false, Direction.NONE, 0);
		generateBranch(branchSize, Direction.SOUTH, spawnSouth, false, Direction.NONE, 0);
		generateBranch(branchSize, Direction.EAST, spawnEast, false, Direction.NONE, 0);
		generateBranch(branchSize, Direction.WEST, spawnWest, false, Direction.NONE, 0);
	}
	
	void generateBranch(int desiredBranchSize, Direction tendency, Room startingRoom, bool isOffshootBranch, Direction ancestorDirection, int ancestorRoomNumber){
		int currentBranchSize = 0;
		int roomsGenerated = 1;
		int offshootsGenerated = 1;
		Room operatingRoom = startingRoom;
		while (currentBranchSize < desiredBranchSize){
			// generate the room
			operatingRoom = generateRoom(operatingRoom, tendency, isOffshootBranch, desiredBranchSize, ref roomsGenerated, ref offshootsGenerated, ancestorDirection, ancestorRoomNumber);
			rooms.Add(operatingRoom);
			currentBranchSize++;
		}
	}
	
	Room generateRoom(Room parent, Direction tendency, bool isOffshoot, int desiredBranchSize, ref int r, ref int o, Direction ancestorDirection, int ancestorRoomNumber){
		Room newRoom = new Room("", 0, 0);
		bool roomGenerated = false;
		int attempts = 0;
		
		while (!roomGenerated && attempts < desiredAttempts){
			int seed = rnd.Next(1,100);
			attempts++;
			
			if (seed<curlyRandomness){ // we will mutate - now flip a coin to decide a "left" or "right" mutation
				int subseedYin = rnd.Next(0,1);
				int subseedYang;
				if (subseedYin == 0) {
					subseedYin = -1;
					subseedYang = 1;
				}
				else {
					subseedYin = 1;
					subseedYang = -1;
				}
				
				// a north- or south- tending leaf will either generate an east- or west- neighbor
				// an east- or west- tending leaf will either generate a north- or south- neighbor
				switch (tendency){
					case Direction.NORTH:
						possiblePos = (parent.getRoomPosX()+subseedYin, parent.getRoomPosY());
						if (!occupiedSpaces.Any(element => (element.Item1 == possiblePos.Item1 && element.Item2 == possiblePos.Item2))) {
							occupiedSpaces.Add(possiblePos);
							if (subseedYin==1){
								parent.setEastRoom(newRoom);
								newRoom.setWestRoom(parent);
							}
							else {
								parent.setWestRoom(newRoom);
								newRoom.setEastRoom(parent);
							}
							newRoom.setRoomPos(possiblePos);
							r++;
							roomGenerated = true;
						}
						break;
					case Direction.EAST:
						possiblePos = (parent.getRoomPosX(), parent.getRoomPosY()+subseedYang);
						if (!occupiedSpaces.Any(element => (element.Item1 == possiblePos.Item1 && element.Item2 == possiblePos.Item2))) {
							occupiedSpaces.Add(possiblePos);
							if (subseedYang==1){
								parent.setNorthRoom(newRoom);
								newRoom.setSouthRoom(parent);
							}
							else {
								parent.setSouthRoom(newRoom);
								newRoom.setNorthRoom(parent);
							}
							newRoom.setRoomPos(possiblePos);
							r++;
							roomGenerated = true;
						}
						break;
					case Direction.SOUTH:
						possiblePos = (parent.getRoomPosX()+subseedYang, parent.getRoomPosY());
						if (!occupiedSpaces.Any(element => (element.Item1 == possiblePos.Item1 && element.Item2 == possiblePos.Item2))) {
							occupiedSpaces.Add(possiblePos);
							if (subseedYang==1){
								parent.setEastRoom(newRoom);
								newRoom.setWestRoom(parent);
							}
							else {
								parent.setWestRoom(newRoom);
								newRoom.setEastRoom(parent);
							}
							newRoom.setRoomPos(possiblePos);
							r++;
							roomGenerated = true;
						}
						break;
					case Direction.WEST:
						possiblePos = (parent.getRoomPosX(), parent.getRoomPosY()+subseedYin);
						if (!occupiedSpaces.Any(element => (element.Item1 == possiblePos.Item1 && element.Item2 == possiblePos.Item2))) {
							occupiedSpaces.Add(possiblePos);
							if (subseedYin==1){
								parent.setNorthRoom(newRoom);
								newRoom.setSouthRoom(parent);
							}
							else {
								parent.setSouthRoom(newRoom);
								newRoom.setNorthRoom(parent);
							}
							newRoom.setRoomPos(possiblePos);
							r++;
							roomGenerated = true;
						}
						break;
				}
			}
			else { // go straight, no mutation
				switch (tendency){
					case Direction.NORTH:
						possiblePos = (parent.getRoomPosX(), parent.getRoomPosY()+1);
						if (!occupiedSpaces.Any(element => (element.Item1 == possiblePos.Item1 && element.Item2 == possiblePos.Item2))) {
							occupiedSpaces.Add(possiblePos);
							newRoom.setRoomPos(possiblePos);
							parent.setNorthRoom(newRoom);
							newRoom.setSouthRoom(parent);
							if (!isOffshoot) {
								r++;
							} else {
								o++;
							}
							roomGenerated = true;
						}
						break;
					case Direction.EAST:
						possiblePos = (parent.getRoomPosX()+1, parent.getRoomPosY());
						if (!occupiedSpaces.Any(element => (element.Item1 == possiblePos.Item1 && element.Item2 == possiblePos.Item2))) {
							occupiedSpaces.Add(possiblePos);
							newRoom.setRoomPos(possiblePos);
							parent.setEastRoom(newRoom);
							newRoom.setWestRoom(newRoom);
							if (!isOffshoot) {
								r++;
							} else {
								o++;
							}
							roomGenerated = true;
						}
						break;
					case Direction.SOUTH:
						possiblePos = (parent.getRoomPosX(), parent.getRoomPosY()-1);
						if (!occupiedSpaces.Any(element => (element.Item1 == possiblePos.Item1 && element.Item2 == possiblePos.Item2))) {
							occupiedSpaces.Add(possiblePos);
							newRoom.setRoomPos(possiblePos);
							parent.setSouthRoom(newRoom);
							newRoom.setNorthRoom(parent);
							if (!isOffshoot) {
								r++;
							} else {
								o++;
							}
							roomGenerated = true;
						}
						break;
					case Direction.WEST:
						possiblePos = (parent.getRoomPosX()-1, parent.getRoomPosY());
						if (!occupiedSpaces.Any(element => (element.Item1 == possiblePos.Item1 && element.Item2 == possiblePos.Item2))) {
							occupiedSpaces.Add(possiblePos);
							newRoom.setRoomPos(possiblePos);
							parent.setWestRoom(newRoom);
							newRoom.setEastRoom(newRoom);
							if (!isOffshoot) {
								r++;
							} else {
								o++;
							}
							roomGenerated = true;
						}
						break;
				}
			}
		}
		
		// attempt to generate offshoot rooms/branches
		if (!isOffshoot){
			int offshootSeed = rnd.Next(1,100);
			int offshootDirection = rnd.Next(0,1);
			
			int offshootRooms = r;
			int offshootOffshoots = o;
			
			Room offshootAncestor = newRoom;
			
			if (offshootSeed<=branchChance+budChance){ // generate a branch : the length of the branch cannot be >= remaining length of branch
				if (desiredBranchSize-r > 1) {
					int offshootLength;
					if (offshootSeed<=budChance) { offshootLength = 1; }
					else { offshootLength = rnd.Next(2,desiredBranchSize-r); }
					if (desiredBranchSize-r >=0 && newRoom.getRoomPosX() != 0 && newRoom.getRoomPosY() != 0){
						if (tendency == Direction.NORTH || tendency == Direction.SOUTH) {
							if (offshootDirection == 0) {
								generateBranch(offshootLength, Direction.WEST, newRoom, true, tendency, r);
							}
							else {
								generateBranch(offshootLength, Direction.EAST, newRoom, true, tendency, r);
							}
						}
						else if (tendency == Direction.EAST || tendency == Direction.WEST){
							if (offshootDirection == 0) {
								generateBranch(offshootLength, Direction.NORTH, newRoom, true, tendency, r);
							}
							else {
								generateBranch(offshootLength, Direction.SOUTH, newRoom, true, tendency, r);
							}
						}
					}
				}
			}
			// otherwise do nothing.
		}
		
		// name rooms correctly: North 1, East 7, etc
		// endpoints are labeled North's End... etc
		// offshoots get a decimal designation - might want to change / make optional this to create ambiguity
		string roomPrefix = "";
		string roomSuffix = "";
		string roomName = "";
		
		Direction t = tendency;
		if (isOffshoot) { t = ancestorDirection; }
		
		// TODO: offshoot ROOMS forget direction, but appear to be numbering correctly. hardcode direction somehow
		// TODO: offshoot BRANCHES forget direction, and the first digit of the number (x.1, x.2, x.3) resets to 1 each time. fix pls
		// standardize the solutions for these maybe?
		
		// TODO: hardcode spawn room doors since you just get locked in sometimes
		
		switch (t) {
			case Direction.NORTH:
				roomPrefix = "North";
				break;
			case Direction.EAST:
				roomPrefix = "East";
				break;
			case Direction.SOUTH:
				roomPrefix = "South";
				break;
			case Direction.WEST:
				roomPrefix = "West";
				break;
		}
		
		if (isOffshoot) {
			roomSuffix = " " + ancestorRoomNumber.ToString() + "." + (o-1).ToString();
		}
		else if (r == desiredBranchSize+1) {
			roomSuffix = "'s End";
		}
		else {
			roomSuffix = " " + r.ToString();
		}
		
		roomName = (roomPrefix + roomSuffix);
		newRoom.setRoomName(roomName);
		
		// determine room type
		if (r == desiredBranchSize+1) {
			newRoom.setType(Room.RoomType.ENDPOINT);
			Debug.Log("assigned endpoint");
		}
		
		return newRoom;
	}
	
	public Tilemap wallTileMap;
	public Tilemap groundTileMap;
	public Tile wallTile;
	public Tile groundTile;
	
	void drawRoom(Room r){
		int[,] roomArray = r.generateTileArray();
		for (int i = 0; i<10; i++){
			for (int j = 0; j<14; j++){
				if (roomArray[i,j] == 1){
					// set the groundtilemap at pos to ground tile
					// groundTileMap.SetTile(new Vector3Int((-5 + i) + (14 * r.getRoomPosX()), (4 - j) + (10 * r.getRoomPosY()), 0), groundTile);
					wallTileMap.SetTile(new Vector3Int(j-5 + 14 * r.getRoomPosX(), i-5 + 10 * r.getRoomPosY(), 0), wallTile);
					groundTileMap.SetTile(new Vector3Int(j-5 + 14 * r.getRoomPosX(), i-5 + 10 * r.getRoomPosY(), 0), null);
				}
				else {
					// set walltilemap at pos to wall tile
					groundTileMap.SetTile(new Vector3Int(j-5 + 14 * r.getRoomPosX(), i-5 + 10 * r.getRoomPosY(), 0), groundTile);
					wallTileMap.SetTile(new Vector3Int(j-5 + 14 * r.getRoomPosX(), i-5 + 10 * r.getRoomPosY(), 0), null);
				}
			}
		}
	}
	
	// fix inaccuracies of world creation
	// forces spawn exits open
	// checks for any void-leading exits from rooms
	void doCleanup(){		
		// fix void-leading exits
		for (int i=0; i<rooms.Count(); i++){
			if (rooms[i].getEastRoom() == null) {
				wallTileMap.SetTile(new Vector3Int(8 + 14 * rooms[i].getRoomPosX(), 0 + 10 * rooms[i].getRoomPosY(), 0), wallTile);
				wallTileMap.SetTile(new Vector3Int(8 + 14 * rooms[i].getRoomPosX(), -1 + 10 * rooms[i].getRoomPosY(), 0), wallTile);
			}
			if (rooms[i].getWestRoom() == null) {
				wallTileMap.SetTile(new Vector3Int(-5 + 14 * rooms[i].getRoomPosX(), 0 + 10 * rooms[i].getRoomPosY(), 0), wallTile);
				wallTileMap.SetTile(new Vector3Int(-5 + 14 * rooms[i].getRoomPosX(), -1 + 10 * rooms[i].getRoomPosY(), 0), wallTile);
			}
			if (rooms[i].getNorthRoom() == null) {
				wallTileMap.SetTile(new Vector3Int(1 + 14 * rooms[i].getRoomPosX(), 4 + 10 * rooms[i].getRoomPosY(), 0), wallTile);
				wallTileMap.SetTile(new Vector3Int(2 + 14 * rooms[i].getRoomPosX(), 4 + 10 * rooms[i].getRoomPosY(), 0), wallTile);
			}
			if (rooms[i].getSouthRoom() == null) {
				wallTileMap.SetTile(new Vector3Int(1 + 14 * rooms[i].getRoomPosX(), -5 + 10 * rooms[i].getRoomPosY(), 0), wallTile);
				wallTileMap.SetTile(new Vector3Int(2 + 14 * rooms[i].getRoomPosX(), -5 + 10 * rooms[i].getRoomPosY(), 0), wallTile);
			}
		}
	}
	
	void fixSpawn(){
		ValueTuple<int,int>[] forceGroundPositions =
		{
			(8, 0),
			(0, -1),
			(1, 4),
			(2, 4),
			(-5, 0),
			(-5, -1),
			(1, -5),
			(2, -5)
		};
		
		// force spawn open
		for (int i=0; i<forceGroundPositions.Count(); i++){
			groundTileMap.SetTile(new Vector3Int(forceGroundPositions[i].Item1, forceGroundPositions[i].Item2, 0), groundTile);
			wallTileMap.SetTile(new Vector3Int(forceGroundPositions[i].Item1, forceGroundPositions[i].Item2, 0), null);
		}
	}
	
	void createConnections(){
		int connectionSeed;
		for (int i=0; i<rooms.Count(); i++){
			Room relativeEastRoom = getRoomFromPos((rooms[i].getRoomPosX()+1, rooms[i].getRoomPosY()));
			Room relativeWestRoom = getRoomFromPos((rooms[i].getRoomPosX()-1, rooms[i].getRoomPosY()));
			Room relativeNorthRoom = getRoomFromPos((rooms[i].getRoomPosX(), rooms[i].getRoomPosY()+1));
			Room relativeSouthRoom = getRoomFromPos((rooms[i].getRoomPosX(), rooms[i].getRoomPosY()-1));
			
			if (rooms[i].getEastRoom() == null && relativeEastRoom != null) {
				connectionSeed = rnd.Next(1,100);
				if (connectionSeed<=connectChance) {
					rooms[i].setEastRoom(relativeEastRoom);
					relativeEastRoom.setWestRoom(rooms[i]);
				}
			}
			if (rooms[i].getWestRoom() == null && relativeWestRoom != null) {
				connectionSeed = rnd.Next(1,100);
				if (connectionSeed<=connectChance) {
					rooms[i].setWestRoom(relativeWestRoom);
					relativeWestRoom.setEastRoom(rooms[i]);
				}
			}
			if (rooms[i].getNorthRoom() == null && relativeNorthRoom != null) {
				connectionSeed = rnd.Next(1,100);
				if (connectionSeed<=connectChance) {
					rooms[i].setNorthRoom(relativeNorthRoom);
					relativeNorthRoom.setSouthRoom(rooms[i]);
				}
			}
			if (rooms[i].getSouthRoom() == null && relativeSouthRoom != null) {
				connectionSeed = rnd.Next(1,100);
				if (connectionSeed<=connectChance) {
					rooms[i].setSouthRoom(relativeSouthRoom);
					relativeSouthRoom.setNorthRoom(rooms[i]);
				}
			}
		}
	}
	
	public Room getRoomFromPos(ValueTuple<int,int> posTuple){
		for (int i=0; i<rooms.Count; i++){
			if (rooms[i].getRoomPos() == posTuple){
				return rooms[i];
			}
		}
		return null;
	}
	
	public void discoverRoom(Room room){
		if (room!=null) {
			room.discovered = true;
			room.roomGO.SetActive(true);
		}
	}
	
	// not currently implemented
	public void discoverPlus(Room room){
		discoverRoom(room);
		discoverRoom(room.getNorthRoom());
		discoverRoom(room.getSouthRoom());
		discoverRoom(room.getEastRoom());
		discoverRoom(room.getWestRoom());
	}
	
	public void populateRooms(){
		
	}
}
