using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random=System.Random;
using Photon.Pun;
using Photon.Realtime;


namespace PotDong.BloodBound {
    public class GameManager : MonoBehaviourPunCallbacks {
        
        #region Private Serialize Fields
        [Tooltip("MyCard")]
        [SerializeField]
        private GameObject MyCard;
        #endregion
        #region Private Fields
        private static Random _random = new Random(); 
        List<Card> CardPile = new List<Card>();
        
        #endregion

        #region Public Fields
        
        #endregion

        #region Photon Callbacks
        #endregion

        
        #region MonoBehaviour Callbacks
        void Start() {
            if (PhotonNetwork.IsMasterClient) {
                InitCardPile(PhotonNetwork.CurrentRoom.PlayerCount);
                DealCards();
            }
        }
        #endregion

        #region Public Method

        public static void Shuffle<T>(IList<T> list)  {  
            int n = list.Count;  
            while (n > 1) {  
                n--;  
                int k = _random.Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }  
        }
        #endregion
        
        
        #region Private Method

        void InitCardPile (int playerCount) {
            List<int> nums = new List<int>();
            int half = playerCount/2;
            for (var i=1; i<=9; i++) {
                nums.Add(i);
            }
            Shuffle(nums);
            for (var i=0; i<half ;i++) {
                CardPile.Add(new Card(Color.Blue, nums[i]));
            }
            Shuffle(nums);
            for (var i=0; i< playerCount-half; i++) {
                CardPile.Add(new Card(Color.Red, nums[i]));
            }
            Shuffle(CardPile);
        }
        
        void DealCards () {
            int i=0;
            
            foreach (Player player in PhotonNetwork.PlayerList) {
                ExitGames.Client.Photon.Hashtable t = new ExitGames.Client.Photon.Hashtable();

                Card card = CardPile[i];
                t.Add(Constants.key_team, card.Team);
                t.Add(Constants.key_num, card.Num);
                t.Add(Constants.key_shown_color, card.ShownColor);
                int[] blood_arr = new int[3];
                t.Add(Constants.key_blood_arr, blood_arr);
                t.Add(Constants.key_blood_idx, 0);
                // Debug.Log(player.NickName + ": ("+bbPlayer.card.Team.ToString()+","+bbPlayer.card.Num.ToString()+")");
                
                player.SetCustomProperties(t);
                i++;
                
            }
        }
        
        

        #endregion

    }
}