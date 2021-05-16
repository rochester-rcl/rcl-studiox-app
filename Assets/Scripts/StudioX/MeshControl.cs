using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshControl : MonoBehaviour
{
    public bool meshAdded = false;
    public GameObject game;
    
    void Update(){
        game = GameObject.Find("Your Object");
        
    }
    public void MoveMeshLeft(){
        game.transform.position = new Vector3(game.transform.position.x - 2f,game.transform.position.y, game.transform.position.z );
    }
        public void MoveMeshRight(){
        game.transform.position = new Vector3(game.transform.position.x + 2f,game.transform.position.y, game.transform.position.z );
    }
        public void MoveMeshForward(){
        game.transform.position = new Vector3(game.transform.position.x ,game.transform.position.y, game.transform.position.z - 2f );
    }
        public void MoveMeshBackwards(){
        game.transform.position = new Vector3(game.transform.position.x,game.transform.position.y, game.transform.position.z + 2f);
    }
    public void Downsize(){
        game.transform.localScale = game.transform.localScale/2;
    }
    public void Grow(){
        game.transform.localScale = game.transform.localScale*2;
    }
}
