using Casino.Games.BxB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Casino.Games.OneLineSlot {
	public class CharacterSwapper : BXBCharContainer
    {
		private Image character;

		public void Awake() {
			character = gameObject.GetComponent<Image> ();
		}

//		void OnEnable() {
//			if (character != null) {
//				character = gameObject.GetComponent<Image> ();
//			}
//		}

		public override void Replace(Sprite replacingSprite) {
			character.sprite = replacingSprite;
           
		}

        public void Normal()
        {
            character.color = new Color32(255, 255, 255, 255);
        }
        public void UnNormal()
        {
            character.color = new Color32(255, 255, 255, 100);
        }
    }
}

