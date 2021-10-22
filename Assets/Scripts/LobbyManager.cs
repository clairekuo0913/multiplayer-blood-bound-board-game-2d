using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

namespace PotDong.BloodBound {

    public class LobbyManager : MonoBehaviourPunCallbacks {
        
        #region Private Serializable Fields

        [Tooltip("Game Start Button")]
        [SerializeField]
        private GameObject gameStartBtn;

        [Tooltip("text of list of players")]
        [SerializeField]
        private GameObject listOfPlayers;

        #endregion

        #region Private Fields
        private string playerList;
        #endregion
        
        #region MonoBehaviour Callbacks
        
        void Start() {
            if(PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount >=2) { 
                gameStartBtn.SetActive(true);
            } else {
                gameStartBtn.SetActive(false);
            }
            updatePlayerList();
            
        }

        #endregion

        #region Photon Callbacks
        
        /// <summary>
        /// Called when the local player left the room. We need to load the launcher scene.
        /// </summary>
        public override void OnLeftRoom() {
            SceneManager.LoadScene(0);
        }

        public override void OnPlayerEnteredRoom(Player other) {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}" , other.NickName); // not seen if you're the player connecting
            updatePlayerList();
            if (PhotonNetwork.IsMasterClient) {
                Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
                LoadLobby();
            }
        }

        public override void OnPlayerLeftRoom(Player other) {
            Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects
            updatePlayerList();
            if (PhotonNetwork.IsMasterClient) {
                Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
                LoadLobby();
            }   
        }

        #endregion
    
        #region Public Methods

        public void LeaveRoom() {
            PhotonNetwork.LeaveRoom();
        }

        public void GameStart() {
            PhotonNetwork.LoadLevel("CardTable");
        }

        #endregion

        #region Private Methods

        void LoadLobby() {
            if(!PhotonNetwork.IsMasterClient){
                Debug.LogError("PhotonNetwork : Trying to load but we are not the master client.");
            }
            Debug.LogFormat("PhotonNetwork : Player Count = {0}", PhotonNetwork.CurrentRoom.PlayerCount);
            
            
            PhotonNetwork.LoadLevel("Lobby");
        }

        void updatePlayerList() {
            playerList = "在線玩家 :\n";
            foreach (Player player in PhotonNetwork.PlayerList) {
                playerList += player.NickName+"\n";
            }
            listOfPlayers.GetComponent<Text>().text = playerList;
        }

        #endregion
    }
}