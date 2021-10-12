using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PotDong.BloodBound {
    public enum Color {
        Blue,
        Red
    }

    public enum Blood {
        Color = 1,
        Question = 2,
        Number = 3,
        None = 0
    }

    public class Card {
        #region Private Fields
        public Color Team {
            get; private set;
        }
        public int Num {
            get; private set;
        }
        private Blood ColorBlood1, ColorBlood2;
        public Color ShownColor {
            get; private set;
        }
        #endregion

        #region Public Methods

        public Card(Color team, int num) {
            Team = team;
            Num = num;
            if(num > 9 || num < 1) {
                Debug.LogError("Wrong Number In Card Init.");
                return;
            }
            if(num == 3) {
                ShownColor = OppositeColor(team);
            } else {
                ShownColor = team;
            }
        }

        #endregion

        #region Private Methods

        Color OppositeColor(Color c) {
            if (c==Color.Blue) {
                return Color.Red;
            }else {
                return Color.Blue;
            }
        }

        #endregion

        
    }
}
