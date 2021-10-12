using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

namespace PotDong.BloodBound {
    public class BloodBoundPlayer : MonoBehaviour {
        #region Private Serializable Fields
        [Tooltip("Blood")]
        [SerializeField]
        private GameObject[] mBlood;
        
        
        #endregion

        #region Private Fields
        
        Card card;
        #endregion
        
        #region Public Fields
        public Player player;
        #endregion


        #region MonoBehavior Callbacks
        
        void Start() {
            
        }
        
        #endregion
        
        #region Public Method

        public void updateSprite(int BloodId, Blood[] BloodArr) {
            // Initialize the card
            if(card == null) {
                Color team = (Color) player.CustomProperties[Constants.key_team];
                int num = (int) player.CustomProperties[Constants.key_num];
                card = new Card(team, num);
            }
            if (BloodId == 0) {
                for (int i=0; i < 3 ; i++) {
                    Image curBlood = mBlood[i].GetComponent<Image>();
                    SetSprite(Blood.None, curBlood);
                }
            } else {
                for (int i=0; i < BloodId ; i++) {
                    Image curBlood = mBlood[i].GetComponent<Image>();
                    SetSprite(BloodArr[i], curBlood);
                }
            }            
        }
        
        #endregion

        #region Private Method

        void SetSprite(Blood blood, Image curBlood) {
            if(blood == Blood.Color) {
                if(card.Team == Color.Blue) {
                    curBlood.sprite = Resources.Load<Sprite>("Sprites/BlueBlood");
                } else {
                    curBlood.sprite = Resources.Load<Sprite>("Sprites/RedBlood");
                }
            } else if (blood == Blood.Number) {
                string loader = "Sprites/NumBlood" + card.Num.ToString();
                curBlood.sprite = Resources.Load<Sprite>(loader);
            } else if (blood == Blood.None) {
                curBlood.sprite = Resources.Load<Sprite>("Sprites/NoneBlood");
            } else if (blood == Blood.Question) {
                curBlood.sprite = Resources.Load<Sprite>("Sprites/QuestionBlood");
            }
        }
        
        #endregion

    }
}