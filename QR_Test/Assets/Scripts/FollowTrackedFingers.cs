using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;

namespace Hydac_QR
{
    public class FollowTrackedFingers : MonoBehaviour
    {
        #region Public and Private Attributes
        // Public Attributes
        public GameObject fingerObject;
        public GameObject indexFingerObject;
        public GameObject wristObject;
        public float dist = 0f;

        // Private Attributes
        private Vector3[] _FingerTipVectorsLeft;
        private Vector3[] _FingerTipVectorsRight;
        private List<GameObject> _FingerObjsLeft;
        private List<GameObject> _FingerObjsRight;
        private GameObject _WristObjLeft;
        private GameObject _WristObjRight;
        private MixedRealityPose pose;

        // Read-only Properties
        private Vector3 _WristLeftPosition;
        public Vector3 WristLeftPosition
        {
            get { return _WristLeftPosition; }              // Accessed by PlayerHololensMgrPUN.cs
        }

        private Vector3 _WristRightPosition;
        public Vector3 WristRightPosition
        {
            get { return _WristRightPosition; }             // Accessed by PlayerHololensMgrPUN.cs
        }

        private Quaternion _WristLeftRotation;
        public Quaternion WristLeftRotation
        {
            get { return _WristLeftRotation; }              // Accessed by PlayerHololensMgrPUN.cs
        }

        private Quaternion _WristRightRotation;
        public Quaternion WristRightRotation
        {
            get { return _WristRightRotation; }             // Accessed by PlayerHololensMgrPUN.cs
        }

        private HandPoses _LeftHandPose;
        public HandPoses LeftHandPose
        {
            get { return _LeftHandPose; }                   // Accessed by PlayerHololensMgrPUN.cs
        }

        private HandPoses _RightHandPose;
        public HandPoses RightHandPose
        {
            get { return _RightHandPose; }                  // Accessed by PlayerHololensMgrPUN.cs
        }

        private bool _IsTracking;
        public bool IsTracking
        {
            get { return _IsTracking; }                     // Accessed by PlayerHololensMgrPUN.cs
        }
        #endregion

        #region Unity Methods
        private void Awake()
        {
            _FingerTipVectorsLeft = new Vector3[5];
            _WristLeftPosition = Vector3.zero;
            _WristLeftRotation = Quaternion.identity;
            _FingerTipVectorsRight = new Vector3[5];
            _WristRightPosition = Vector3.zero;
            _WristRightRotation = Quaternion.identity;
            _FingerObjsLeft = new List<GameObject>();
            _FingerObjsRight = new List<GameObject>();
        }

        private void Start()
        {
            GenerateHands();
            HideHandMesh();
        }

        private void Update()
        {
            // Disable Render Each Frame (only render if hand is tracked)
            _IsTracking = false;
            //HideHandMesh();
            //for (int i = 0; i < 5; i++)
            //{
            //    _FingerObjsLeft[i].GetComponent<Renderer>().enabled = false;
            //    _FingerObjsRight[i].GetComponent<Renderer>().enabled = false;

            //}
            //_WristObjLeft.GetComponent<Renderer>().enabled = false;
            //_WristObjRight.GetComponent<Renderer>().enabled = false;

            // LEFT HAND
            // Attempt to track fingertips and wrist
            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, Handedness.Left, out pose))
            {
                _FingerTipVectorsLeft[0] = pose.Position;
                //_FingerObjsLeft[0].GetComponent<Renderer>().enabled = true;
            }

            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, Handedness.Left, out pose))
            {
                _FingerTipVectorsLeft[1] = pose.Position;
                //_FingerObjsLeft[1].GetComponent<Renderer>().enabled = true;
            }

            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleTip, Handedness.Left, out pose))
            {
                _FingerTipVectorsLeft[2] = pose.Position;
                //_FingerObjsLeft[2].GetComponent<Renderer>().enabled = true;
            }

            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.RingTip, Handedness.Left, out pose))
            {
                _FingerTipVectorsLeft[3] = pose.Position;
                //_FingerObjsLeft[3].GetComponent<Renderer>().enabled = true;
            }

            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyTip, Handedness.Left, out pose))
            {
                _FingerTipVectorsLeft[4] = pose.Position;
                //_FingerObjsLeft[4].GetComponent<Renderer>().enabled = true;
            }

            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, Handedness.Left, out pose))
            {
                _WristLeftPosition = pose.Position;
                _WristLeftRotation = pose.Rotation;
                //_WristObjLeft.GetComponent<Renderer>().enabled = true;
                _IsTracking = true;             // Set to true when left / right wrist is detected as being tracked (for network syncing)
            }

            // RIGHT HAND
            // Attempt to track fingertips and wrist
            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, Handedness.Right, out pose))
            {
                _FingerTipVectorsRight[0] = pose.Position;
                //_FingerObjsRight[0].GetComponent<Renderer>().enabled = true;
            }

            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, Handedness.Right, out pose))
            {
                _FingerTipVectorsRight[1] = pose.Position;
                //_FingerObjsRight[1].GetComponent<Renderer>().enabled = true;
            }

            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleTip, Handedness.Right, out pose))
            {
                _FingerTipVectorsRight[2] = pose.Position;
                //_FingerObjsRight[2].GetComponent<Renderer>().enabled = true;
            }

            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.RingTip, Handedness.Right, out pose))
            {
                _FingerTipVectorsRight[3] = pose.Position;
                //_FingerObjsRight[3].GetComponent<Renderer>().enabled = true;
            }

            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyTip, Handedness.Right, out pose))
            {
                _FingerTipVectorsRight[4] = pose.Position;
                //_FingerObjsRight[4].GetComponent<Renderer>().enabled = true;
            }

            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, Handedness.Right, out pose))
            {
                _WristRightPosition = pose.Position;
                _WristRightRotation = pose.Rotation;
                //_WristObjRight.GetComponent<Renderer>().enabled = true;

                _IsTracking = true;         // Set to true when left / right wrist is detected as being tracked (for network syncing)
            }

            // Update Fingertip and Wrist game objects Transform data
            for (int i = 0; i < 5; i++)
            {
                _FingerObjsLeft[i].transform.position = _FingerTipVectorsLeft[i] + (_FingerTipVectorsLeft[i] - _WristLeftPosition) * dist;
                _FingerObjsRight[i].transform.position = _FingerTipVectorsRight[i] + (_FingerTipVectorsRight[i] - _WristRightPosition) * dist;
            }

            _WristObjLeft.transform.position = _WristLeftPosition;
            _WristObjLeft.transform.rotation = _WristLeftRotation;
            _WristObjRight.transform.position = _WristRightPosition;
            _WristObjRight.transform.rotation = _WristRightRotation;

            // Determine and Update Hand Pose
            // LEFT HAND
            if (Vector3.Distance(_FingerObjsLeft[2].transform.position, _WristObjLeft.transform.position) >= 0.08f)
            {
                // Normal Pose
                _LeftHandPose = HandPoses.LEFTNORMAL;
            }
            else
            {
                if (Vector3.Distance(_FingerObjsLeft[1].transform.position, _WristObjLeft.transform.position) >= 0.08f)
                {
                    // Point Pose
                    _LeftHandPose = HandPoses.LEFTPOINT;
                }
                else
                {
                    // Thumb Up Pose
                    _LeftHandPose = HandPoses.LEFTTHUMBUP;
                }
            }

            // RIGHT HAND
            if (Vector3.Distance(_FingerObjsRight[2].transform.position, _WristObjRight.transform.position) >= 0.08f)
            {
                // Normal Pose
                _RightHandPose = HandPoses.RIGHTNORMAL;
            }
            else
            {
                if (Vector3.Distance(_FingerObjsRight[1].transform.position, _WristObjRight.transform.position) >= 0.08f)
                {
                    // Point Pose
                    _RightHandPose = HandPoses.RIGHTPOINT;
                }
                else
                {
                    // Thumb Up Pose
                    _RightHandPose = HandPoses.RIGHTTHUMBUP;
                }
            }
        }
        #endregion

        #region Finger-Related Methods
        /// <summary>
        /// Instantiates the fingertip and wrist game objects for left and right hands
        /// </summary>
        private void GenerateHands()
        {
            GameObject leftObj;
            GameObject rightObj;

            // Generate Finger Objects (5 per hand)
            for (int i = 0; i < 5; i++)
            {
                if (i == 1)
                {
                    // Instantiate Index Finger Object
                    leftObj = Instantiate(indexFingerObject, this.transform);
                    rightObj = Instantiate(indexFingerObject, this.transform);
                }
                else
                {
                    // Instantiate Generic Finger Object
                    leftObj = Instantiate(fingerObject, this.transform);
                    rightObj = Instantiate(fingerObject, this.transform);
                }

                _FingerObjsLeft.Add(leftObj);
                _FingerObjsRight.Add(rightObj);
            }

            // Generate Wrist Objects (1 per hand)
            _WristObjLeft = Instantiate(wristObject, this.transform);
            _WristObjRight = Instantiate(wristObject, this.transform);
        }

        /// <summary>
        /// Disables the render mesh for hands and fingers
        /// </summary>
        private void HideHandMesh()
        {
            for (int i = 0; i < 5; i++)
            {
                _FingerObjsLeft[i].GetComponent<Renderer>().enabled = false;
                _FingerObjsRight[i].GetComponent<Renderer>().enabled = false;

            }
            _WristObjLeft.GetComponent<Renderer>().enabled = false;
            _WristObjRight.GetComponent<Renderer>().enabled = false;
        }

        /// <summary>
        /// Gets and returns the selected left finger position in world space (for network syncing).
        /// </summary>
        public Vector3 GetLeftFinger(int finger)
        {
            return _FingerObjsLeft[finger].transform.position;
        }

        /// <summary>
        /// Gets and returns the selected right finger position in world space (for network syncing).
        /// </summary>
        public Vector3 GetRightFinger(int finger)
        {
            return _FingerObjsRight[finger].transform.position;
        }
        #endregion
    }
}