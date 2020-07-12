using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float speed = 3.0f;
    public float obstacleRange = 5.0f;

    private bool alive;

    public GameObject bulletPrefab;
    public GameObject player;
    private int it;
    public GameObject enemyPrefab;
    public GameObject spawn;
    public static int enemyKills = 0;
    public GameObject winScreen;
    private bool game;

    void Start()
    {
        alive = true;
        it = 1;
        game = true;
    }

    void Update()
    {
	    if(game) {
	    	if(transform == null) {
	    		return;
	    		enemyKills++;
	    	}
	        if(alive) {
	        	transform.position += (player.transform.position - transform.position).normalized * speed * Time.deltaTime;
	        	transform.rotation = Quaternion.Euler(player.transform.rotation.eulerAngles);
	        }

	        if(it%50 == 0) {
	        	GameObject bullet = Instantiate(bulletPrefab, transform.position, new Quaternion(0,0,0,0)) as GameObject;
	        	Rigidbody rb = bullet.GetComponent<Rigidbody>();
	        	rb.velocity = (player.transform.position - transform.position).normalized * 30;
	        }

	        it++;
	        if(it%250 == 0 && it < 510) {
	        	GameObject enemy = Instantiate(enemyPrefab, spawn.transform.position, new Quaternion(0,0,0,0)) as GameObject;
	        }
		    if(enemyKills == 1) {
		    	winScreen.SetActive(true);
		    	game = false;
		    }
		    Debug.Log(enemyKills);
		}
        
    }
    public void SetAlive(bool a) {
    	alive = a;
    }
}
