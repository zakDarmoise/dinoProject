﻿using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using Windows.Kinect;
using Joint = Windows.Kinect.Joint;
using System.Linq;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BodySourceView : MonoBehaviour
{
    public BodySourceManager mBodySourceManager;
    public GameObject mJointObject;

    [SerializeField] private Material openMat;
    [SerializeField] private Material closedMat;
    [SerializeField] private Material lassoMat;
    [SerializeField] private Material unknownMat;
    private HandState handLeftPreviousState;
    private HandState handRightPreviousState;

    private Dictionary<ulong, GameObject> mBodies = new Dictionary<ulong, GameObject>();

    private List<JointType> _joints = new List<JointType>
    {
        JointType.HandLeft,
        JointType.HandRight,
    };

    void Update()
    {
        #region Get Kinect data

        Body[] data = mBodySourceManager.GetData();
        if (data == null)
            return;

        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in data)
        {
            if (body == null)
                continue;

            if (body.IsTracked)
                trackedIds.Add(body.TrackingId);
        }

        #endregion

        #region Delete Kinect bodies

        List<ulong> knownIds = new List<ulong>(mBodies.Keys);
        foreach (ulong trackingId in knownIds)
        {
            if (!trackedIds.Contains(trackingId))
            {
                // Destroy body object
                Destroy(mBodies[trackingId]);

                // Remove from list
                mBodies.Remove(trackingId);
            }
        }

        #endregion

        #region Create Kinect bodies

        foreach (var body in data)
        {
            // If no body, skip
            if (body == null)
                continue;

            if (body.IsTracked)
            {
                // If body isn't tracked, create body
                if (!mBodies.ContainsKey(body.TrackingId))
                    mBodies[body.TrackingId] = CreateBodyObject(body.TrackingId);

                // Update positions
                UpdateBodyObject(body, mBodies[body.TrackingId]);
            }
        }

        #endregion
    }

    private GameObject CreateBodyObject(ulong id)
    {
        // Create body parent
        GameObject body = new GameObject("Body:" + id);

        // Create joints
        foreach (JointType joint in _joints)
        {
            // Create Object
            GameObject newJoint = Instantiate(mJointObject);
            newJoint.name = joint.ToString();

            // Parent to body
            newJoint.transform.parent = body.transform;
        }

        return body;
    }

    private void UpdateBodyObject(Body body, GameObject bodyObject)
    {
        // Update joints
        foreach (JointType _joint in _joints)
        {
            // Get new target position
            Joint sourceJoint = body.Joints[_joint];
            Vector3 targetPosition = GetVector3FromJoint(sourceJoint);
            targetPosition.z = Camera.main.transform.position.z + 3;

            // Get joint, set new position
            Transform jointObject = bodyObject.transform.Find(_joint.ToString());
            jointObject.position = targetPosition;
        }

        if (body.HandLeftState == HandState.Closed)
        {
            var rend = bodyObject.GetComponentsInChildren<Renderer>()
                .SingleOrDefault(obj => obj.gameObject.name == "HandLeft");
            if (rend != null) rend.material = closedMat;
            if (handLeftPreviousState != HandState.Closed)
            {
                PointerEventData pointerData = new PointerEventData(EventSystem.current);
                pointerData.position = Camera.main.WorldToScreenPoint(bodyObject.transform.Find("HandLeft").position);
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);
                foreach (RaycastResult result in results)
                {
                    Debug.Log(results);
                    Button button = result.gameObject.GetComponent<Button>();
                    if (button != null)
                    {
                        ExecuteEvents.Execute(button.gameObject, pointerData, ExecuteEvents.pointerClickHandler);
                        break;
                    }
                }
            }

            handLeftPreviousState = HandState.Closed;
        }
        else if (body.HandLeftState == HandState.Open)
        {
            var rend = bodyObject.GetComponentsInChildren<Renderer>()
                .SingleOrDefault(obj => obj.gameObject.name == "HandLeft");
            if (rend != null) rend.material = openMat;
            handLeftPreviousState = HandState.Open;
        }
        else if (body.HandLeftState == HandState.Lasso)
        {
            var rend = bodyObject.GetComponentsInChildren<Renderer>()
                .SingleOrDefault(obj => obj.gameObject.name == "HandLeft");
            if (rend != null) rend.material = lassoMat;
            handLeftPreviousState = HandState.Lasso;
        }
        else
        {
            var rend = bodyObject.GetComponentsInChildren<Renderer>()
                .SingleOrDefault(obj => obj.gameObject.name == "HandLeft");
            if (rend != null) rend.material = unknownMat;
            handLeftPreviousState = HandState.Unknown;
        }

        if (body.HandRightState == HandState.Closed)
        {
            var rend = bodyObject.GetComponentsInChildren<Renderer>()
                .SingleOrDefault(obj => obj.gameObject.name == "HandRight");
            if (rend != null) rend.material = closedMat;
            if (handRightPreviousState != HandState.Closed)
            {
                PointerEventData pointerData = new PointerEventData(EventSystem.current);
                pointerData.position = Camera.main.WorldToScreenPoint(bodyObject.transform.Find("HandRight").position);
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);
                foreach (RaycastResult result in results)
                {
                    Debug.Log(results);
                    Button button = result.gameObject.GetComponent<Button>();
                    if (button != null)
                    {
                        ExecuteEvents.Execute(button.gameObject, pointerData, ExecuteEvents.pointerClickHandler);
                        break;
                    }
                }
            }

            handRightPreviousState = HandState.Closed;
        }
        else if (body.HandRightState == HandState.Open)
        {
            var rend = bodyObject.GetComponentsInChildren<Renderer>()
                .SingleOrDefault(obj => obj.gameObject.name == "HandRight");
            if (rend != null) rend.material = openMat;
            handRightPreviousState = HandState.Open;
        }
        else if (body.HandRightState == HandState.Lasso)
        {
            var rend = bodyObject.GetComponentsInChildren<Renderer>()
                .SingleOrDefault(obj => obj.gameObject.name == "HandRight");
            if (rend != null) rend.material = lassoMat;
            handRightPreviousState = HandState.Lasso;
        }
        else
        {
            var rend = bodyObject.GetComponentsInChildren<Renderer>()
                .SingleOrDefault(obj => obj.gameObject.name == "HandRight");
            if (rend != null) rend.material = unknownMat;
            handRightPreviousState = HandState.Unknown;
        }
    }

    private Vector3 GetVector3FromJoint(Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }
}