using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StretchObjectBetweenTwoPoints : MonoBehaviour {

    public GameObject startPoint;
    public GameObject endPoint;
    public GameObject strechedObject;

	// Use this for initialization
    // Or getting parts of a prefab
	void Start () {

    }
	
	// Update is called once per frame
	void Update () {
        StretchObject();
    }

    void StretchObject()
    {
        // target - start
        Vector3 directionFromStartToEnd = (endPoint.transform.position - startPoint.transform.position).normalized;
        float halfMagnitude = (endPoint.transform.position - startPoint.transform.position).magnitude / 2;
        Vector3 halfWayPoint = (startPoint.transform.position) + directionFromStartToEnd * halfMagnitude;

        // set the strethed object to the center position
        strechedObject.transform.position = halfWayPoint;
        // Have the object look at either the start or end position, doesn't matter
        strechedObject.transform.LookAt(startPoint.transform);

        // Scale the object based on the distance between the two points
        strechedObject.transform.localScale = new Vector3(strechedObject.transform.localScale.x, 
            strechedObject.transform.localScale.y,
            halfMagnitude * 2);
    }

}
