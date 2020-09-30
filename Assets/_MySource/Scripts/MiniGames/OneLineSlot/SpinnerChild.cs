using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Casino.Games.OneLineSlot {	
	public class SpinnerChild : ItemMoveContainer {
		public CharacterSwapper[] columnImgs;

		public void UpdateImageSprite(int index, Sprite newSprite) {
			columnImgs [index].Replace(newSprite);
		}

		public void UpdateImageSprite(Sprite sprite) {
			//columnImgs [index].Replace(sprite);
		}

		public void UpdateImageSprite(int index, int newSpriteIndex) {

		}

   
    }
}

