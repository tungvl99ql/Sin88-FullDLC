using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;

namespace Casino.Games.BxB {	
	public class SlotColumn : ItemMoveContainer {
		public BXBCharContainer[] columnImgs;

		public void UpdateImageSprite(int index, Sprite newSprite) {
            //columnImgs [index].Replace(newSprite);
            columnImgs[index].GetComponent<Image>().overrideSprite = newSprite;
        }

		public void UpdateImageSprite(int index, int newSpriteIndex) {
			
		}

	}
}

