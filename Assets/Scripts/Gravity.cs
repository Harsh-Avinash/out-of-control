// Cannot change direction once gravity has been changed right now
using UnityEngine;
using System.Collections;

public class Gravity : MonoBehaviour
{
    Vector3 gravity;
    public bool GravityXMinus = false;
    public bool GravityYMinus = false;
    public bool GravityZMinus = false;
    public bool GravityXPlus = false;
    public bool GravityYPlus = false;
    public bool GravityZPlus = false;
 
 void Start()
 {
     gravity = Physics.gravity;
 }

 void FixedUpdate()
 {
     Physics.gravity = gravity;
     
     if(GravityZMinus)
     {
         gravity.x = 0;
         gravity.y = 0;
         gravity.z = -25;
     }
     if(GravityXMinus)
     {
         gravity.x = -25;
         gravity.y = 0;
         gravity.z = 0;
     }
      if(GravityYMinus)
     {
         gravity.x = 0;
         gravity.y = -25;
         gravity.z = 0;
     }
     if(GravityZPlus)
     {
         gravity.x = 0;
         gravity.y = 0;
         gravity.z = 25;
     }
     if(GravityXPlus)
     {
         gravity.x = 25;
         gravity.y = 0;
         gravity.z = 0;
     }
      if(GravityYPlus)
     {
         gravity.x = 0;
         gravity.y = 25;
         gravity.z = 0;
     }
 }    
}