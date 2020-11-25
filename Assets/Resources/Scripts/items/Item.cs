using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Item : MonoBehaviour
{
	public Room originRoom;
	public PlayerController player;
	
	System.Random rnd = new System.Random();
	
	ValueTuple<int, int> size = (0, 0);
	
	enum Rarity {
		COMMON,
		RARE,
		EPIC,
		LEGENDARY,
		ROOT
	}
	
	Rarity itemRarity;
	
    // Start is called before the first frame update
    void Start()
    {
		setRarity(rnd.Next(1,100));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	public int getWidth(){ return size.Item1; }
	public int getHeight(){ return size.Item2; }
	
	public void setRarity(int random){
		if (random<=originRoom.itemdist_rare) { itemRarity = Rarity.COMMON; }
		else if (random>originRoom.itemdist_rare+originRoom.itemdist_epic+originRoom.itemdist_legendary+originRoom.itemdist_root) { itemRarity = Rarity.RARE; }
		else if (random>originRoom.itemdist_epic+originRoom.itemdist_legendary+originRoom.itemdist_root) { itemRarity = Rarity.EPIC; }
		else if (random>originRoom.itemdist_legendary+originRoom.itemdist_root) { itemRarity = Rarity.LEGENDARY; }
		else if (random==originRoom.itemdist_root) { itemRarity = Rarity.ROOT; }
	}
	
	public string assembleName(){
		string name;
		List<String> adjectives = new List<String>();
		
		// The Royal Order of Adjectives
		
		// Determiner (number of things in the item)
		
		// Opinion (rarity)
			
		// Size
		if (size.Item1 == 0 || size.Item2 == 0) { return "ERROR"; }
		// Shape
		// Age
		// Color
		// Origin
		// Material
		// Qualifier (just part of noun - e.g. BASEBALL bat)
		// Noun
		return "ERROR";
	}
}
