using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

namespace PotDong.BloodBound {
    public class GameUiManager : MonoBehaviourPunCallbacks {

        #region Private Serializable Fields

        [Tooltip("Player's Name Tag")]
        [SerializeField]
        private GameObject bloodBoundPlayer;

        [Tooltip("MyCard")]
        [SerializeField]
        private GameObject MyCard;

        [Tooltip("MyCard(Vis)")]
        [SerializeField]
        private GameObject Charactor;
/*
        [Tooltip("Next Player Color")]
        [SerializeField]
        private GameObject NextPlayerColor;
*/
        [Tooltip("Next Color")]
        [SerializeField]
        private GameObject NextColor;

        [Tooltip("Blood")]
        [SerializeField]
        private GameObject PlayerSprite;
        #endregion

        #region Private Fields
        private bool _isRendered_MyCard = false;
        //private bool _isRendered_NextPlayerColor = false;
        private bool _isRendered_NextPlayerColor = true;
        private Dictionary<string,GameObject> AllPlayers = new Dictionary<string,GameObject>();
        #endregion

        #region Private Methods
        void instantiatePlayersName() {
            Player nextPlayer = PhotonNetwork.LocalPlayer;
            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            float radius = Mathf.Min(this.GetComponent<RectTransform>().rect.height, this.GetComponent<RectTransform>().rect.width)/2-50.0f;
            
            for(int i=0; i<playerCount ; i++) {
                float angle = i * Mathf.PI*2f / playerCount;
                angle -= (float)0.5*Mathf.PI;
                Vector2 newPos = new Vector3(Mathf.Cos(angle)*radius,Mathf.Sin(angle)*radius);
                GameObject g = Instantiate(bloodBoundPlayer, newPos, Quaternion.identity, this.transform);
                g.GetComponent<RectTransform>().anchoredPosition = newPos;
                g.GetComponent<Text>().text = nextPlayer.NickName;
                g.GetComponent<BloodBoundPlayer>().player = nextPlayer;
                AllPlayers.Add(nextPlayer.UserId, g);
                nextPlayer = nextPlayer.GetNext();
            }
        }

        void test_instantiatePlayersName() {
            int playerCount = 8;
            float radius = Mathf.Min(this.GetComponent<RectTransform>().rect.height, this.GetComponent<RectTransform>().rect.width)/2-50.0f;
            for(int i=0; i<playerCount ; i++) {
                float angle = i * Mathf.PI*2f / playerCount;
                angle -= (float)0.5*Mathf.PI;
                Vector2 newPos = new Vector3(Mathf.Cos(angle)*radius,Mathf.Sin(angle)*radius);
                GameObject g = Instantiate(bloodBoundPlayer, newPos, Quaternion.identity, this.transform);
                g.GetComponent<RectTransform>().anchoredPosition = newPos;
            }
        }
        
        #endregion

        #region Public Methods

        public void CostBlood(string strBlood) {
            Player player = PhotonNetwork.LocalPlayer;
            Blood blood;
            if (strBlood == "Number") {
                blood = Blood.Number;
            } else if(strBlood == "Question") {
                blood = Blood.Question;
            } else if(strBlood == "Color") {
                blood = Blood.Color;
            } else {
                Debug.LogError("This Kind Of Blood Is Not Exist!! Check the inspector of button onClick()");
                return;
            }
            int BloodIdx = (int) player.CustomProperties[Constants.key_blood_idx];
            int[] intBloodArr = (int[]) player.CustomProperties[Constants.key_blood_arr];
            Blood[] BloodArr = intBloodArr.Select(x => (Blood)x).ToArray();
            
            if (BloodIdx >= 3) {
                Debug.LogError("Blood is out of index!!");
                return;
            }
            BloodArr[BloodIdx] = blood;
            intBloodArr[BloodIdx] = (int) blood;
            ExitGames.Client.Photon.Hashtable t = new ExitGames.Client.Photon.Hashtable();
            BloodIdx +=1;
            t.Add(Constants.key_blood_idx, BloodIdx);
            t.Add(Constants.key_blood_arr, intBloodArr);
            player.SetCustomProperties(t);
            Debug.Log(player.NickName+" cost blood, idx = "+ ((int)player.CustomProperties[Constants.key_blood_idx]).ToString());
        }

        public void ClearBlood() {
            Player player = PhotonNetwork.LocalPlayer;
            ExitGames.Client.Photon.Hashtable t = new ExitGames.Client.Photon.Hashtable();
            int[] blood_arr = new int[3];
            t.Add(Constants.key_blood_arr, blood_arr);
            t.Add(Constants.key_blood_idx, 0);
            player.SetCustomProperties(t);
        }

        

        public void DisplayMyCard() {
            if (!_isRendered_MyCard) {
                RenderMyCard();
                _isRendered_MyCard = true;
            }
            MyCard.SetActive(!MyCard.activeSelf);
        }

        public void DisplayNextPlayerCard() {
            /*
            if (!_isRendered_NextPlayerColor) {
                RenderNextPlayerColor();
                _isRendered_NextPlayerColor = true;
            }
            NextPlayerColor.SetActive(!NextPlayerColor.activeSelf);
            */
            NextColor.GetComponent<Image>().sprite = Resources.Load<Sprite>("UI_dong/questionblood");
            Player nextPlayer = PhotonNetwork.LocalPlayer.GetNext();
            Color nextPlayerColor = (Color) nextPlayer.CustomProperties[Constants.key_shown_color];
            if (nextPlayerColor == Color.Blue) {
                NextColor.GetComponent<Image>().sprite = Resources.Load<Sprite>("UI_dong/blueblood");
            } 
            else {
                NextColor.GetComponent<Image>().sprite = Resources.Load<Sprite>("UI_dong/redblood");
            }
            
        }

        public void HideNextPlayerCard() {
            NextColor.GetComponent<Image>().sprite = Resources.Load<Sprite>("UI_dong/eye");
        }

        #endregion

        #region MonoBehaviour Callbacks
        
        void Start() {
            MyCard.SetActive(false);
            //NextPlayerColor.SetActive(false);
            instantiatePlayersName();
            DisplayMyCard();
        }
        #endregion
        
        #region MonoBehaviourPunCallbacks
        public override void OnPlayerPropertiesUpdate(Player p, ExitGames.Client.Photon.Hashtable changedProps) {
            int BloodIdx = (int) changedProps[Constants.key_blood_idx];
            int[] intBloodArr = (int[]) changedProps[Constants.key_blood_arr];
            Blood[] BloodArr = intBloodArr.Select(x => (Blood)x).ToArray();
            AllPlayers[p.UserId].GetComponent<BloodBoundPlayer>().updateSprite(BloodIdx, BloodArr);
            Debug.Log(p.NickName+": idx="+BloodIdx.ToString());

        }
        #endregion

        #region Private Methods

        void RenderMyCard() {
            GameObject chColor = MyCard.transform.GetChild(0).gameObject;
            GameObject chNum = MyCard.transform.GetChild(1).gameObject;
            Color myTeam = (Color) PhotonNetwork.LocalPlayer.CustomProperties[Constants.key_team];
            int myNum = (int) PhotonNetwork.LocalPlayer.CustomProperties[Constants.key_num];
            Card my_card = new Card(myTeam, myNum);
            
            if (my_card.Team == Color.Blue) {
                chColor.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/BlueBlood");
            } else {
                chColor.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/RedBlood");
            }
            string loader = "Sprites/NumBlood" + my_card.Num.ToString();
            chNum.GetComponent<Image>().sprite = Resources.Load<Sprite>(loader);

            string cardname;
            if(my_card.Team == Color.Blue)  cardname = "UI_dong/blue"+myNum.ToString();
            else    cardname = "UI_dong/red"+myNum.ToString();
            Charactor.GetComponent<Image>().sprite = Resources.Load<Sprite>(cardname);
            Debug.Log("cardname");

        }

        /*
        void RenderNextPlayerColor() {
            Player nextPlayer = PhotonNetwork.LocalPlayer.GetNext();
            Color nextPlayerColor = (Color) nextPlayer.CustomProperties[Constants.key_shown_color];
            if (nextPlayerColor == Color.Blue) {
                NextPlayerColor.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/BlueBlood");
            } else {
                NextPlayerColor.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/RedBlood");
            }
            Debug.Log(nextPlayer.NickName+":"+nextPlayerColor.ToString());
        }*/

        #endregion
    }
}