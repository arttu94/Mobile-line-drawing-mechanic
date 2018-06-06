using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private List<Vector3> wayPoints;

    public float distanceBetweenPoints;

    public int waypointsLimit, minimumWaypoints;

    bool pawnSelected;
    PlayerUnit pawn;

    void Start()
    {
        pawn = null;
        pawnSelected = false;
        wayPoints = new List<Vector3>();
    }

    void SendInfoToPawn()
    {
        if (pawnSelected && wayPoints.Count > minimumWaypoints) //check pawn for null value and waypoints for a minimum
        {
            pawnSelected = false;

            if (wayPoints.Count > waypointsLimit)
                wayPoints.RemoveRange(waypointsLimit, wayPoints.Count - waypointsLimit);

            if (!pawn.GetComponent<PlayerUnit>().isWaiting)
            {
                pawn.steerigBhvr.PathNodes = wayPoints.ToArray();
                pawn.steerigBhvr.SetNodeLineRenderPoints();
                pawn.steerigBhvr.currentNode = 0;
                pawn.GetComponentInChildren<SkinnedMeshRenderer>().material.SetColor("_OutlineColor", Color.black);
            }
            pawn = null;
        }
        ClearWayPointData();
    }

    void ClearWayPointData()//this includes line renderer points data
    {
        wayPoints.Clear();
        GetComponent<LineRenderer>().positionCount = 0;
    }

    void UserLineInput()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = new RaycastHit[3];
            hits = Physics.RaycastAll(r, 300);

            if (wayPoints.Count > 0)
            {
                Vector3 pointToAdd = Vector3.zero;
                foreach (var h in hits)
                {
                    //if the user goes out of the platform while touching it will send the info until that point to the pawn 
                    if (h.transform.tag == "Platform")
                        pointToAdd = h.point;
                }
                if (pointToAdd == Vector3.zero)
                {
                    //user went out of the platform while drawing, collected info until then, will be sent to the pawn
                    SendInfoToPawn();
                }
                else
                {
                    if ((Vector3.Distance(pointToAdd, wayPoints[wayPoints.Count - 1]) > distanceBetweenPoints && wayPoints.Count <= waypointsLimit))
                    {
                        wayPoints.Add(pointToAdd);
                        GetComponent<LineRenderer>().positionCount++;
                        GetComponent<LineRenderer>().SetPosition(wayPoints.Count - 1, new Vector3(pointToAdd.x, 0.1f, pointToAdd.z));
                    }
                }
            }
            else //only for the first touch, if it's a pawn it will be passed on after the touch event exits.
            {
                foreach (var h in hits)
                {
                    if (h.transform.tag == "Platform")
                    {
                        wayPoints.Add(h.point);
                        GetComponent<LineRenderer>().positionCount = 1;
                        GetComponent<LineRenderer>().SetPosition(0, h.point);
                    }
                    if (h.transform.tag == "Pawn" && !pawnSelected)
                    {
                        pawnSelected = true;
                        pawn = h.transform.GetComponent<PlayerUnit>();
                        pawn.GetComponentInChildren<SkinnedMeshRenderer>().material.SetColor("_OutlineColor", new Color(1f, 0.5f, 0f));
                    }
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            SendInfoToPawn();
        }
    }

    void Update()
    {
        UserLineInput();
    }

}
