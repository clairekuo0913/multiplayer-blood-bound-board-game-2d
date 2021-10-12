using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace PotDong.BloodBound {
    public class Launcher : MonoBehaviourPunCallbacks {
        
        #region Private Serializable Fields
        /// <summary>
        /// The maximum number of players per room. When a room is full, it can't be joined 
        /// by new players, and so new room will be created
        /// </summary>
        [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
        [SerializeField]
        private byte maxPlayerPerRoom = 16;

        [Tooltip("The UI Panel to let the user enter name, connect and play ")]
        [SerializeField]
        private GameObject controlPanel;

        [Tooltip("The UI Label to inform the user that the connection is in progress")]
        [SerializeField]
        private GameObject progressLabel;
        
        [Tooltip("The Text to remind user input their name")]
        [SerializeField]
        private GameObject noNameReminder;

        #endregion

        #region Private Constants

        // Store the PlayerPref Key to avoid typos
        const string playerNamePrefKey = "PlayerName";

        #endregion
        #region Private Fields

        /// <summary>
        /// This client's version number. Users are seperated from each 
        /// other by gameVersion (which allows you to make breaking changes)
        /// </summary>
        string gameVersion = "1";
        
        /// <summary>
        /// Keep track of the current process. Since connection is asynchronous and 
        /// is based on several callbacks from Photon, we need to keep track of this
        /// to properly adjust the behavior when we receive call back by Photon.
        /// Typically this is used for the OnConnectedToMaster() callback.
        /// </summary>
        bool isConnecting;
        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehavior method called on GameObject by Unity during early
        /// initilization phase.
        /// </summary>
        void Awake() {
            // #Critical
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master
            // client and all clients in the same room
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        /// <summary>
        /// Monobehavior method called on GameObject by Unity during initialization phase.
        /// </summary>
        void Start() {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
            noNameReminder.SetActive(false);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Start the connection process.
        /// - If already connected, we attempt joining a random room
        /// - If not yet connected, Connect this application instance to Photon Cloud Network
        /// </summary>
        public void Connect() {
            
            if (string.IsNullOrEmpty(PlayerPrefs.GetString(playerNamePrefKey))){
                noNameReminder.SetActive(true);
                return;
            }
            
            progressLabel.SetActive(true);
            controlPanel.SetActive(false);
            noNameReminder.SetActive(false);
            // we check if we are connect or not, we join if we are, else we initiate the connection to the server
            if (PhotonNetwork.IsConnected){
                // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
                PhotonNetwork.JoinRandomRoom();
            } else {
                // keep track of the will to join a room, because when we come back from the 
                // game we will get a callback that we are connected, so we need to know what to do then
                isConnecting = PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = gameVersion;
            }   
        }
        #endregion

        #region MonoBehaviourPunCallbacks
        
        public override void OnConnectedToMaster() {
            Debug.Log("Launcher: OnConnectedToMaster was called by PUN");
            if(isConnecting) {
                PhotonNetwork.JoinRandomRoom();
                isConnecting = false;
            }
        }

        public override void OnDisconnected(DisconnectCause cause) {
            Debug.LogWarningFormat("Launcher: OnDisconnected() was called by PUN");
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
            isConnecting = false;
        }

        public override void OnJoinRandomFailed(short returnCode, string message) {
            Debug.Log("Launcher: OnJoinRandomFailed() was called by PUN. No random room available, so we create one. \n Calling: PhotonNetwork.CreateRoom");
            PhotonNetwork.CreateRoom(null, new RoomOptions{ MaxPlayers = maxPlayerPerRoom,PublishUserId = true});
        }
        
        public override void OnJoinedRoom() {
            Debug.Log("Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1) {
                Debug.Log("We load Lobby.");
                PhotonNetwork.LoadLevel("Lobby");
            }
        }

        #endregion
    }
}