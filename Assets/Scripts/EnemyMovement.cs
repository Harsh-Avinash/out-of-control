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

    void Start()
    {
        alive = true;
        it = 1;
    }

    void Update()
    {
        if(alive) {
        	transform.position += (player.transform.position - transform.position).normalized * speed * Time.deltaTime;
        	transform.rotation = Quaternion.Euler(-player.transform.rotation.eulerAngles);
        }

        if(it%40 == 0) {
        	GameObject bullet = Instantiate(bulletPrefab, transform.position, new Quaternion(0,0,0,0)) as GameObject;
        	Rigidbody rb = bullet.GetComponent<Rigidbody>();
        	rb.velocity = (player.transform.position - transform.position).normalized * 30;
        }

        it++;
        
    }
    public void SetAlive(bool a) {
    	alive = a;
    }
}
