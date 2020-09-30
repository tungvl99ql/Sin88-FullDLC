using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using UnityEngine.UI;

namespace Casino.Games.BxB {
	public class BXBCharContainer : MonoBehaviour {
		private Image character;
        public int index;
        private void Awake()
        {
            character = GetComponent<Image>();
        }
        public virtual void Replace(Sprite prefab) {
			if (character != null) {
                //	Destroy (character.gameObject);
                character.overrideSprite = prefab;
			}
       //   character = Instantiate (prefab, this.transform) as Image;
           // character.overrideSprite = prefab;



        }
 
	}
}

