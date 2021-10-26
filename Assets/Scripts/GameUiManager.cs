using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using UnityEngine.EventSystems;


namespace PotDong.BloodBound {
    public class GameUiManager : MonoBehaviourPunCallbacks,IOnEventCallback {

        #region Private Serializable Fields

        [Tooltip("Player's Name Tag")]
        [SerializeField]
        private GameObject bloodBoundPlayer;

        [Tooltip("MyCard")]
        [SerializeField]
        private GameObject Character;

        [Tooltip("Blood Space - Color Blood")]
        [SerializeField]
        private GameObject ColorBloodButton;

        [Tooltip("Blood Space - Number Blood")]
        [SerializeField]
        private GameObject NumberBloodButton;

        [Tooltip("Next Color")]
        [SerializeField]
        private GameObject NextColor;

        [Tooltip("Popup Panel")]
        [SerializeField]       
        private GameObject PopupPanel;

        [Tooltip("Knife")]
        [SerializeField]
        private GameObject knife;

        [Tooltip("KnifeSpeed")]
        [SerializeField]
        private float knife_speed;
        #endregion

        #region Public Fields
        [HideInInspector]
        public GameObject lastSelectedGameObject;
        #endregion

        #region Private Fields
        private bool _isRendered_MyCard = false;
        private Card my_card = null;

        /* Allplayers[Player.UserId] = UserTemplateGameObject */
        private Dictionary<string,GameObject> AllPlayers = new Dictionary<string,GameObject>();

        /* Knife 相關參數 */
        private bool _isKnifeMoving = false;    // 開關，在Update()中監測，如果是true則更新knife_game_object的位置
        private string knife_p1, knife_p2;
        private Vector3 knife_pos1, knife_pos2;
        private GameObject currentSelectedGameObject_Recent;
        private GameObject knife_game_object;
        private bool knife_isGive;

        #endregion

        #region Private Methods
        void instantiatePlayersName() {
            Player nextPlayer = PhotonNetwork.LocalPlayer;
            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            float radius = Mathf.Min(this.GetComponent<RectTransform>().rect.height, this.GetComponent<RectTransform>().rect.width)/2-50.0f;
            
            for(int i=0; i<playerCount ; i++) {
                float angle = i * Mathf.PI * 2f / playerCount;
                angle -= (float)0.5*Mathf.PI;
                Vector2 newPos = new Vector3(Mathf.Cos(angle)*radius,Mathf.Sin(angle)*radius);
                GameObject g = Instantiate(bloodBoundPlayer, newPos, Quaternion.identity, this.transform);
                g.GetComponent<RectTransform>().anchoredPosition = newPos;
                g.GetComponentInChildren<Text>().text = nextPlayer.NickName;
                g.GetComponent<BloodBoundPlayer>().player = nextPlayer;
                AllPlayers.Add(nextPlayer.UserId, g);
                nextPlayer = nextPlayer.GetNext();
            }
        }

        void test_instantiatePlayersName(int playerCount) {
            
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

        public void KnifeStabButtonClick() {
            
            if(lastSelectedGameObject!=null && lastSelectedGameObject.name == "isSelected" && lastSelectedGameObject.transform.parent.gameObject.GetComponent<BloodBoundPlayer>() != null) {
                choosePlayerEnd();
                GameObject g = lastSelectedGameObject.transform.parent.gameObject;
                KnifeStabPlayer(PhotonNetwork.LocalPlayer, g.GetComponent<BloodBoundPlayer>().player);
            }
        }

        public void KnifeGiveButtonClick() {
            
            if(lastSelectedGameObject!=null && lastSelectedGameObject.name == "isSelected" && lastSelectedGameObject.transform.parent.gameObject.GetComponent<BloodBoundPlayer>() != null) {
                choosePlayerEnd();
                GameObject g = lastSelectedGameObject.transform.parent.gameObject;
                KnifeGivePlayer(PhotonNetwork.LocalPlayer, g.GetComponent<BloodBoundPlayer>().player);
            }
        }


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
                if(LocalRender())              
                    _isRendered_MyCard = true;
            }
        }

        public void DisplayNextPlayerCard() {
            NextColor.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/QuestionBlood");
            Player nextPlayer = PhotonNetwork.LocalPlayer.GetNext();
            Color nextPlayerColor = (Color) nextPlayer.CustomProperties[Constants.key_shown_color];
            if (nextPlayerColor == Color.Blue) {
                NextColor.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/BlueBlood");
            } 
            else {
                NextColor.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/RedBlood");
            }
            
        }

        public void HideNextPlayerCard() {
            NextColor.GetComponent<Image>().sprite = Resources.Load<Sprite>("UI_dong/eye");
        }

        #endregion

        #region MonoBehaviour Callbacks
        
        void Start() {
            instantiatePlayersName();
            DisplayMyCard();
            Debug.Log("GameUiManager: Start() end");
        }

        void Update() {
            GetLastGameObjectSelected();
            if(!_isRendered_MyCard) {
                DisplayMyCard();
            }
            if(_isKnifeMoving ==true) {   
                knife_game_object.transform.position = Vector2.MoveTowards(knife_game_object.transform.position, knife_pos2, knife_speed * Time.deltaTime);
                if(knife_game_object.transform.position == knife_pos2) {
                    // Destroy(knife_game_object);
                    _isKnifeMoving = false;

                    SetKnifeOwner(AllPlayers[knife_p2].GetComponent<BloodBoundPlayer>().player);

                }
            }
        }
        #endregion
        
        #region MonoBehaviourPunCallbacks
        public override void OnPlayerPropertiesUpdate(Player p, ExitGames.Client.Photon.Hashtable changedProps) {
            int BloodIdx = (int) changedProps[Constants.key_blood_idx];
            int[] intBloodArr = (int[]) changedProps[Constants.key_blood_arr];
            Blood[] BloodArr = intBloodArr.Select(x => (Blood)x).ToArray();
            AllPlayers[p.UserId].GetComponent<BloodBoundPlayer>().updateSprite(BloodIdx, BloodArr);
            Debug.Log(p.NickName+": idx="+BloodIdx.ToString());

            if(p==PhotonNetwork.LocalPlayer && my_card == null) {
                Color myTeam = (Color) changedProps[Constants.key_team];
                int myNum = (int) changedProps[Constants.key_num];
                my_card = new Card(myTeam, myNum);
            }
        }

        public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable changedProps) {
            if(changedProps.ContainsKey(Constants.key_knife_owner)) {
                int knifeOwnerId = (int)changedProps[Constants.key_knife_owner];
                if(knifeOwnerId == PhotonNetwork.LocalPlayer.ActorNumber) {
                    choosePlayer();
                } else {
                    Debug.Log(PhotonNetwork.LocalPlayer.Get(knifeOwnerId).NickName + " is the knife owner");
                }
            }
        }

        #endregion
        
        
        /**
        *
        * 所有玩家接收到event1就會同步update畫面上的刀子
        *
        */
        #region IOnEventCallback
        public void OnEvent(EventData photonEvent) {
            if(knife_game_object!= null)
                Destroy(knife_game_object);
            byte eventCode = photonEvent.Code;
            if (eventCode == Constants.eventcode_move_knife) {
                object[] data = (object[]) photonEvent.CustomData;
                knife_p1 = (string) data[0];
                knife_p2 = (string) data[1];
                knife_isGive = (bool)data[2];
                knife_pos1 = AllPlayers[knife_p1].transform.position;
                knife_pos2 = AllPlayers[knife_p2].transform.position;
                knife_game_object = Instantiate(knife, knife_pos1 , Quaternion.identity, this.transform);

                if(knife_isGive) {
                    knife_game_object.transform.up = -(knife_pos2 - knife_pos1).normalized;
                } else {
                    knife_game_object.transform.up = (knife_pos2 - knife_pos1).normalized;
                }
                _isKnifeMoving = true;
            }
        }
        #endregion

        #region Private Methods

        void choosePlayer() {
            PopupPanel.SetActive(true);
            
            foreach (KeyValuePair<string,GameObject> player in AllPlayers) {
                if(player.Key == PhotonNetwork.LocalPlayer.UserId) continue;
                GameObject g = player.Value.transform.Find("isSelected").gameObject;
                g.SetActive(true);
            }
        }

        
        void KnifeUpdateEvent() { 
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
            object[] content = new object[] {knife_p1, knife_p2, knife_isGive}; // true = give, false = stab
            PhotonNetwork.RaiseEvent(Constants.eventcode_move_knife, content, raiseEventOptions, SendOptions.SendReliable);
        }

        void KnifeStabPlayer(Player p1, Player p2) {
            knife_p1 = p1.UserId;
            knife_p2 = p2.UserId;
            knife_isGive = false;
            KnifeUpdateEvent();   
        }


        void KnifeGivePlayer(Player p1, Player p2) {
            knife_p1 = p1.UserId;
            knife_p2 = p2.UserId;
            knife_isGive = true;
            KnifeUpdateEvent();
        }

    

        void choosePlayerEnd() {
            PopupPanel.SetActive(false);
            foreach (KeyValuePair<string,GameObject> player in AllPlayers) {
                GameObject g = player.Value.transform.Find("isSelected").gameObject;
                g.SetActive(false);
             }
        }

        bool LocalRender() {
            if(my_card!=null) {
                string cardname;
                if(my_card.Team == Color.Blue)  cardname = "UI_dong/blue" + my_card.Num.ToString();
                else cardname = "UI_dong/red"+my_card.Num.ToString();
                Character.GetComponent<Image>().sprite = Resources.Load<Sprite>(cardname);
                if(my_card.Team == Color.Blue) {
                    ColorBloodButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/BlueBlood");
                } else {
                    ColorBloodButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/RedBlood");
                }
                NumberBloodButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/NumBlood" + my_card.Num.ToString());
                return true;
            } else {
                return false;
            }
        }

        void GetLastGameObjectSelected() {
            if(EventSystem.current.currentSelectedGameObject != currentSelectedGameObject_Recent) {
                lastSelectedGameObject = currentSelectedGameObject_Recent;
                currentSelectedGameObject_Recent = EventSystem.current.currentSelectedGameObject;
            }
        }

        void SetKnifeOwner(Player p) {
            ExitGames.Client.Photon.Hashtable t = new ExitGames.Client.Photon.Hashtable();
            t.Add(Constants.key_knife_owner, p.ActorNumber);
            PhotonNetwork.CurrentRoom.SetCustomProperties(t);
        }


        #endregion
    }
}