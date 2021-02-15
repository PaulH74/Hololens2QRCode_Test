using UnityEngine;
using Photon.Pun;
//using Photon.Voice.Unity;

namespace Hydac_QR
{
    /// <summary>
    /// This class managers the local player's instance over the PUN network and local player's inputs, sending the Transform data of the local player's VR hardware to other
    /// networked players and receiving their data in turn to animate their VR Avatar on the local player's instance
    /// </summary>
    public class PlayerHololensPUN : MonoBehaviourPun, IPunObservable
    {
        #region Public and Private Attributes
        [Tooltip("The local player instance. Use this to know if local player is represented in the scene")]
        public static GameObject localPlayerInstance;

        // VR Avatar Elements
        [Header("Player Avatar (Displayed to other networked players):")]
        public GameObject headAvatar;
        public GameObject mouthAnimated;
        public GameObject mouthStatic;
        public GameObject leftHandAvatar;
        public GameObject rightHandAvatar;
        public SkinnedMeshRenderer poseNormalLH;
        public SkinnedMeshRenderer poseFingerPointLH;
        public SkinnedMeshRenderer poseThumbUpLH;
        public SkinnedMeshRenderer poseNormalRH;
        public SkinnedMeshRenderer poseFingerPointRH;
        public SkinnedMeshRenderer poseThumbUpRH;
        [Tooltip("Order: Thumb, Index, Middle, Ring, Small")]
        public GameObject[] leftFingers;
        [Tooltip("Order: Thumb, Index, Middle, Ring, Small")]
        public GameObject[] rightFingers;
        private FollowTrackedFingers _Hands;
        private Transform localH2CameraTF;
        private bool _HandsTracked;

        // Smoothing Variables For Remote Player's Motion
        [Header("Player Avatar Motion Smoothing:")]
        [Tooltip("0: no smoothing, > 0: increased smoothing \n(note: smoothing reduces positional accuracy and increases latency)")]
        [Range(0, 3)]
        public int smoothingFactor;     // Set to 2 as default (based on CUBE use-case tests)
        [Tooltip("Maximum distance (metres) for which to apply smoothing")]
        [Range(0, 3)]
        public float appliedDistance;   // Set to 1 as default (based on CUBE use-case tests)
        private Vector3 correctPlayerHeadPosition = Vector3.zero;
        private Quaternion correctPlayerHeadRotation = Quaternion.identity;

        // Hololens Camera Element
        private Camera _CameraRig;

        // Voice Element
        //private Recorder _RecorderPUN;
        #endregion

        #region Unity Methods
        private void Awake()
        {
            // Important:
            // used in RoomManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronised
            if (photonView.IsMine)
            {
                localPlayerInstance = gameObject;

                // Assign Hololens Camera to MRTK Camera in local player's scene
                _CameraRig = Camera.main;
                localH2CameraTF = _CameraRig.transform;                 // Get transform data from local VR Headset

                // Don't display our own "player" avatar to ourselves (except for map icon)
                headAvatar.SetActive(false);
                leftHandAvatar.SetActive(false);
                rightHandAvatar.SetActive(false);
                for (int i = 0; i < leftFingers.Length; i++)
                {
                    leftFingers[i].SetActive(false);
                    rightFingers[i].SetActive(false);
                }

                // Initialise Hands Tracking
                _Hands = GetComponentInChildren<FollowTrackedFingers>();

                // Initialise Voice Elements
                //_RecorderPUN = GetComponent<Recorder>();
                mouthAnimated.SetActive(false);
            }

            // Critical
            // Don't Destroy on load to prevent player from being destroyed when another player joins / leaves the room
            DontDestroyOnLoad(gameObject);
        }

        // Update each frame
        private void Update()
        {
            if (photonView.IsMine)
            {
                // AUDIO GROUPS: 
                // Allow user to set local group
                // Sets next available group.
                // Remote group players add that group to their listen list.

                if (_CameraRig != null)
                {
                    // Update local player's camera transform data
                    localH2CameraTF.position = _CameraRig.transform.position;
                    localH2CameraTF.rotation = _CameraRig.transform.rotation;
                }
                else
                {
                    _CameraRig = Camera.main;
                    localH2CameraTF = _CameraRig.transform;
                }
            }
            else
            {
                // Smooth Remote player's motion on local machine
                SmoothPlayerMotion(ref headAvatar, ref correctPlayerHeadPosition, ref correctPlayerHeadRotation);
            }
        }
        #endregion

        #region Avatar Related Methods
        /// <summary>
        /// Gets a synced integer value from the network and assigns the approprate left or right hand pose (normal, point, thumbs up)
        /// </summary>
        /// <param name="pose"></param>
        private void GetCurrentHandPose(HandPoses pose)
        {
            switch (pose)
            {
                case HandPoses.LEFTNORMAL:
                    SetLeftHandPose(true, false, false);
                    break;
                case HandPoses.LEFTPOINT:
                    SetLeftHandPose(false, true, false);
                    break;
                case HandPoses.LEFTTHUMBUP:
                    SetLeftHandPose(false, false, true);
                    break;
                case HandPoses.RIGHTNORMAL:
                    SetRightHandPose(true, false, false);
                    break;
                case HandPoses.RIGHTPOINT:
                    SetRightHandPose(false, true, false);
                    break;
                case HandPoses.RIGHTTHUMBUP:
                    SetRightHandPose(false, false, true);
                    break;
            }
        }

        /// <summary>
        /// Enables the appropriate skinned mesh renderer (left-hand pose) according to the currently synced pose over the network
        /// </summary>
        /// <param name="normal"></param>
        /// <param name="point"></param>
        /// <param name="thumbUp"></param>
        private void SetRightHandPose(bool normal, bool point, bool thumbUp)
        {
            if (_HandsTracked)
            {
                poseNormalRH.enabled = normal;
                poseFingerPointRH.enabled = point;
                poseThumbUpRH.enabled = thumbUp;
            }
            else
            {
                poseNormalRH.enabled = false;
                poseFingerPointRH.enabled = false;
                poseThumbUpRH.enabled = false;
            }
        }

        /// <summary>
        /// Enables the appropriate skinned mesh renderer (left-hand pose) according to the currently synced pose over the network
        /// </summary>
        /// <param name="normal"></param>
        /// <param name="point"></param>
        /// <param name="thumbUp"></param>
        private void SetLeftHandPose(bool normal, bool point, bool thumbUp)
        {
            if (_HandsTracked)
            {
                poseNormalLH.enabled = normal;
                poseFingerPointLH.enabled = point;
                poseThumbUpLH.enabled = thumbUp;
            }
            else
            {
                poseNormalLH.enabled = false;
                poseFingerPointLH.enabled = false;
                poseThumbUpLH.enabled = false;
            }
        }

        /// <summary>
        /// Toggles Animated / Static mouth on the player avatar when they are speaking / not speaking
        /// </summary>
        /// <param name="animateMouth"></param>
        private void ToggleMouthState(bool animateMouth)
        {
            mouthAnimated.SetActive(animateMouth);
            mouthStatic.SetActive(!animateMouth);
        }

        /// <summary>
        /// Applies LERP interpolation to smooth the remote player's game object motion over the network. 
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="gameObjectCorrectTransformPosition"></param>
        /// <param name="gameObjectCorrectTransformRotation"></param>
        private void SmoothPlayerMotion(ref GameObject gameObject, ref Vector3 gameObjectCorrectTransformPosition, ref Quaternion gameObjectCorrectTransformRotation)
        {
            // Smoothing variables
            float distance = Vector3.Distance(gameObject.transform.position, gameObjectCorrectTransformPosition);

            if (distance < appliedDistance)
            {
                gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, gameObjectCorrectTransformPosition, Time.deltaTime * smoothingFactor);
                gameObject.transform.rotation = Quaternion.Lerp(gameObject.transform.rotation, gameObjectCorrectTransformRotation, Time.deltaTime * smoothingFactor);
            }
            else
            {
                gameObject.transform.position = gameObjectCorrectTransformPosition;
                gameObject.transform.rotation = gameObjectCorrectTransformRotation;
            }
        }
        #endregion

        #region PUN RPCs and Serialize View Method
        /// <summary>
        /// Controls the exchange of data between local and remote player's VR data
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="info"></param>
        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // Send local VR Headset position and rotation data to networked player
                stream.SendNext(localH2CameraTF.position);
                stream.SendNext(localH2CameraTF.rotation);
                stream.SendNext(_Hands.IsTracking);
                stream.SendNext(_Hands.WristLeftPosition);
                stream.SendNext(_Hands.WristLeftRotation);
                stream.SendNext(_Hands.WristRightPosition);
                stream.SendNext(_Hands.WristRightRotation);
                //for (int i = 0; i < 5; i++)
                //{
                //    stream.SendNext(_Hands.GetLeftFinger(i));
                //    stream.SendNext(_Hands.GetRightFinger(i));
                //}
                stream.SendNext((int)_Hands.LeftHandPose);
                stream.SendNext((int)_Hands.RightHandPose);
                //stream.SendNext(_RecorderPUN.VoiceDetector.Detected);      // Toggle "Mouth Animation" on / off when speaking / quiet
            }
            else if (stream.IsReading)
            {
                // Receive networked player's VR Headset position and rotation data
                correctPlayerHeadPosition = (Vector3)stream.ReceiveNext();
                correctPlayerHeadRotation = (Quaternion)stream.ReceiveNext();
                _HandsTracked = (bool)stream.ReceiveNext();
                leftHandAvatar.transform.position = (Vector3)stream.ReceiveNext();
                leftHandAvatar.transform.rotation = (Quaternion)stream.ReceiveNext();
                rightHandAvatar.transform.position = (Vector3)stream.ReceiveNext();
                rightHandAvatar.transform.rotation = (Quaternion)stream.ReceiveNext();
                //for (int i = 0; i < 5; i++)
                //{
                //    leftFingers[i].transform.position = (Vector3)stream.ReceiveNext();
                //    rightFingers[i].transform.position = (Vector3)stream.ReceiveNext();
                //}
                GetCurrentHandPose((HandPoses)stream.ReceiveNext());
                GetCurrentHandPose((HandPoses)stream.ReceiveNext());
                //ToggleMouthState((bool)stream.ReceiveNext());
            }
        }
        #endregion
    }
}