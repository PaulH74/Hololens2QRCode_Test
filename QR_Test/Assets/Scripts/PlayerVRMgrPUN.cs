using UnityEngine;
using Photon.Pun;

namespace Hydac_QR
{
    /// <summary>
    /// This class managers the local player's instance over the PUN network and local player's inputs, sending the Transform data of the local player's VR hardware to other
    /// networked players and receiving their data in turn to animate their VR Avatar on the local player's instance
    /// </summary>
    public class PlayerVRMgrPUN : MonoBehaviourPun, IPunObservable
    {
        #region Public and Private Attributes
        [Tooltip("The local player instance. Use this to know if local player is represented in the scene")]
        public static GameObject localPlayerInstance;

        // VR Avatar Elements
        [Header("Player Avatar (Displayed to other networked players):")]
        public GameObject headAvatar;
        public GameObject leftHandAvatar;
        public GameObject rightHandAvatar;
        public GameObject mouthAnimated;
        public GameObject mouthStatic;

        // Hand Gestures
        [Header("Avatar Hand Poses:")]
        public SkinnedMeshRenderer poseNormalLH;
        public SkinnedMeshRenderer poseThumbUpLH;
        public SkinnedMeshRenderer poseFingerPointLH;
        public SkinnedMeshRenderer poseNormalRH;
        public SkinnedMeshRenderer poseThumbUpRH;
        public SkinnedMeshRenderer poseFingerPointRH;

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
        #endregion

        #region Unity Methods
        //private void Awake()
        //{
        //    // Important:
        //    // used in RoomManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronised
        //    if (photonView.IsMine)
        //    {
        //        localPlayerInstance = gameObject;

        //        // Don't display our own "player" avatar to ourselves (except for map icon)
        //        headAvatar.SetActive(false);
        //        leftHandAvatar.SetActive(false);
        //        rightHandAvatar.SetActive(false);

        //        // Hand Gestures (default state)
        //        SetLeftHandPose(true, false, false);
        //        SetRightHandPose(true, false, false);
        //    }

        //    // Critical
        //    // Don't Destroy on load to prevent player from being destroyed when another player joins / leaves the room
        //    DontDestroyOnLoad(gameObject);
        //}

        // Update each frame
        private void Update()
        {
            if (!photonView.IsMine)
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
            poseNormalRH.enabled = normal;
            poseFingerPointRH.enabled = point;
            poseThumbUpRH.enabled = thumbUp;
        }

        /// <summary>
        /// Enables the appropriate skinned mesh renderer (left-hand pose) according to the currently synced pose over the network
        /// </summary>
        /// <param name="normal"></param>
        /// <param name="point"></param>
        /// <param name="thumbUp"></param>
        private void SetLeftHandPose(bool normal, bool point, bool thumbUp)
        {
            poseNormalLH.enabled = normal;
            poseFingerPointLH.enabled = point;
            poseThumbUpLH.enabled = thumbUp;
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
            if (stream.IsReading)
            {
                // Receive networked player's VR Headset position and rotation data
                correctPlayerHeadPosition = (Vector3)stream.ReceiveNext();
                correctPlayerHeadRotation = (Quaternion)stream.ReceiveNext();
                leftHandAvatar.transform.position = (Vector3)stream.ReceiveNext();
                leftHandAvatar.transform.rotation = (Quaternion)stream.ReceiveNext();
                rightHandAvatar.transform.position = (Vector3)stream.ReceiveNext();
                rightHandAvatar.transform.rotation = (Quaternion)stream.ReceiveNext();
                GetCurrentHandPose((HandPoses)stream.ReceiveNext());
                GetCurrentHandPose((HandPoses)stream.ReceiveNext());
                ToggleMouthState((bool)stream.ReceiveNext());
            }
        }
        #endregion
    }
}