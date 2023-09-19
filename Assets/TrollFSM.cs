using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TrollFSM : MonoBehaviour
{
    public enum TrollSate
    {
        REALIGN_WAYPOINT, SEEK_WAYPOINT, CHASE_ENEMY, FIGHT_ENEMY
    }

    public TrollSate currentStSate = TrollSate.REALIGN_WAYPOINT;
    public GameObject enemy;
    public float maxSpeed = 2; // 2 meters per second

    public float FOVinDeg = 70f;
    private float cosOfFOVover2InRad; // cutoff? valur for visibility check


    public Transform[] waypoints;
    public int currentWaypointIndex=0;

    public float maxAngularSpeedInDegPerSec = 60;// degrees per second
    public float maxAngularSpeedInRadPerSec = 60;// degrees per second
    private float angularSpeedInRadPerFrame; 


    // Start is called before the first frame update
    void Start()
    {
        cosOfFOVover2InRad = Mathf.Cos(FOVinDeg / 2f * Mathf.Deg2Rad);
        maxAngularSpeedInRadPerSec = (maxAngularSpeedInDegPerSec*Mathf.Deg2Rad);
    }

    // Update is called once per frame
    void Update()
    {
        FSM();
    }

    private void FSM()
    {
        switch (currentStSate)
        {
            case TrollSate.REALIGN_WAYPOINT:
                RealignWaypoint();
                break;
            case TrollSate.SEEK_WAYPOINT:
                SeekWaypoint();
                break;
            case TrollSate.CHASE_ENEMY:
                ChaseEnemy();
                break;
            case TrollSate.FIGHT_ENEMY:
                FightEnemy();
                break;
            default:
                //throw new Exception("State does not exist!!");
                print("current state is invalid: "+currentStSate);
                break;
                
        }
    }

    private void FightEnemy()
    {
        // default
        //DoFightEnemy();
        // check transitions
        //t5 enemy dead or lost sigth
        if (EnemyDeadOrLostSight())
        {
            ChangeState(TrollSate.SEEK_WAYPOINT);
        }
        //t6 dist>2
        if (!CheckDistanceLE(2))
        {

        }
    }

    private void ChaseEnemy()
    {
        // default 
        DoChaseEnemy();
        Vector3 T2E_heading = enemy.transform.position - this.transform.position; // heading
        float size = T2E_heading.magnitude;
        T2E_heading.Normalize();

        // check triggers/trnasitions
        // T3 check dist < 2
        if (CheckDistanceLE(2))
        {
            ChangeState(TrollSate.FIGHT_ENEMY);
        }
        // T5 check enemy deade, or lost from sight
        if (EnemyDeadOrLostSight())
        {
            ChangeState(TrollSate.SEEK_WAYPOINT);
        }

    }

    private bool EnemyDeadOrLostSight()
    {
        if (EnemyDead() || LostSight())
        {
            return true;
        }
        return false;
    }

    private bool LostSight()
    {
        return !SeeEnemy();
    }

    private bool EnemyDead()
    {
        return false;
    }


    private bool CheckDistanceLE(float distance)
    {
        if ((Vector3.Distance(this.transform.position, enemy.transform.position)<= distance))
        {
            return true;
        }

        return false;
    }

    private void DoChaseEnemy()
    {
        print("chase enemy");
    }

    private void SeekWaypoint()
    {
        
        // formalize unit vector and the cosine of the angle 

        // default
        DoSeekWaypoint();
        // check transitions

        // T4
        if (WaypointReached())
        {
            print("WaypointReached");
            ChangeState(TrollSate.REALIGN_WAYPOINT);
        }

        // T2
        if (SeeEnemy())
        {
            print("changing state to chase Enemy....");
            ChangeState(TrollSate.CHASE_ENEMY);
        }
    }

    private bool SeeEnemy()
    {
        Vector3 T2Eheading = enemy.transform.position-this.transform.position;
        //float size = T2Eheading.magnitude;
        T2Eheading.Normalize();
        float cosTheta = Vector3.Dot(this.transform.forward, T2Eheading);
        return (cosTheta > cosOfFOVover2InRad);
    }

    private bool WaypointReached()
    {
        if (Vector3.Distance(this.transform.position, waypoints[currentWaypointIndex].position) < float.Epsilon)
        {
            //currentWaypointIndex++;
            return true;
        }
        return false;
    }

    private void DoSeekWaypoint()
    {
        //throw new NotImplementedException();
        this.transform.position = Vector3.MoveTowards(this.transform.position, waypoints[currentWaypointIndex].position,maxSpeed*Time.deltaTime);
    }

    private void RealignWaypoint()
    {
        print("RealignWaypoint");
        // default
        DoRealign();

        // checking transitions
        // T1 aligned?
        if (IsAligned())
        {
            //print("currentWaypointIndex ::: "+ currentWaypointIndex);
            //print("waypoints.Length:: "+ waypoints.Length);
            //currentWaypointIndex = (currentWaypointIndex == waypoints.Length - 1) ? 0 : currentWaypointIndex++;
            if (currentWaypointIndex < waypoints.Length-1)
            {
                currentWaypointIndex++;
            }
            else
            {
                currentWaypointIndex = 0;
            }
            //print("currentWaypointIndex update::: " + currentWaypointIndex);
            ChangeState(TrollSate.SEEK_WAYPOINT);
        }

    }

    private void ChangeState(TrollSate newState)
    {
        currentStSate = newState;
    }

    private bool IsAligned()
    {
        print("IsAligned");
        int i1 = (currentWaypointIndex + 1) % waypoints.Length;
        //print("IsAligned i1: "+i1);//
        Vector3 headindToNextWaypointWP = waypoints[i1].position - this.transform.position;
        //Vector3 headindToNextWaypointWP = waypoints[i1].forward - this.transform.position;
        headindToNextWaypointWP.Normalize();
        /*
         * float distAux = Vector3.Distance(headindToNextWaypointWP, this.transform.forward);
            print("distAux:: "+ distAux);
            if ( distAux < float.Epsilon)
         */
        float distAux = Vector3.Dot(headindToNextWaypointWP, this.transform.forward);
        //print("distAux:: " + distAux);
        if (Math.Abs(distAux - 1f) < 0.1f)
        {
            //print("true..........");
            return true;
        }
        //print("false..........");
        return false;
    }

    private void DoRealign()
    {
        print("DoRealign");
        int i1 = (currentWaypointIndex + 1) % waypoints.Length;
        //print("DoRealign i1: "+i1);
        Vector3 headindToNextWaypointWP = waypoints[i1].position-this.transform.position;
        headindToNextWaypointWP.Normalize();
        
        float maxAngularSpeedInRadPerFrame = maxAngularSpeedInRadPerSec * Time.deltaTime;

        Vector3 targetDirection = waypoints[i1].position - transform.position;

        //print("DoRealign target direction maxAngularSpeedInRadPerFrame: " + maxAngularSpeedInRadPerFrame); 

        //Vector3.RotateTowards(this.transform.forward, headindToNextWaypointWP,
          //  maxAngularSpeedInRadPerFrame, 0);

        Vector3 newDir = Vector3.RotateTowards(this.transform.forward, targetDirection.normalized, maxAngularSpeedInRadPerFrame, 0);
        //Vector3 newDir = Vector3.RotateTowards(this.transform.forward, targetDirection, 1, 0);
        // Draw a ray pointing at our target in
        Debug.DrawRay(transform.position, newDir, Color.white, 5);
        transform.rotation = Quaternion.LookRotation(newDir);

    }

    /*private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for(int i=0; i<waypoints.Length; i++)
        {
            int i1=(i+1)%waypoints.Length;
            Gizmos.DrawLine(waypoints[i].transform.position, waypoints[i1].transform.position);
        }
        Gizmos.color = Color.red;
        //Gizmos.DrawLineList
    }*/

    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        //Gizmos.DrawLine(transform.position, waypoints[currentWaypointIndex].transform.position);
        for (int i = 0; i < waypoints.Length; i++)
        {
            int i1 = (i + 1) % waypoints.Length;
            Gizmos.DrawLine(waypoints[i].transform.position, waypoints[i1].transform.position);
        }
    }
}
