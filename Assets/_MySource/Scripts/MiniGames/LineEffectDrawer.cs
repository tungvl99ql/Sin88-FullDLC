using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using DG.Tweening;

namespace Casino.Games.BxB {
	public class LineEffectDrawer : MonoBehaviour {
        public UILineRenderer uiLineRendererPrefab;
        public Color[] lineColors;

        public Vector2 testVector;

		public void Start()
		{
			//DrawLine(new Vector2(0, 0), new Vector2(1000, 1000));
		}

        public void TweenPos(Vector2 beTween, Vector2 targetPos, float duration) {
            DOTween.To(() => beTween, (Vector2 pNewValue) => beTween = pNewValue , targetPos, duration);
        }



        private List<UILineRenderer> drawnLineList = new List<UILineRenderer>();
        public void DrawLine(int charID, params Vector2[] pos) {

            var l = Instantiate(uiLineRendererPrefab, transform) as UILineRenderer;
			l.rectTransform.SetAsFirstSibling ();
//			l.color = Random.ColorHSV();
			l.color = lineColors[charID-1];
            drawnLineList.Add(l);

            l.Points = new Vector2[pos.Length];
			for (int i = 0; i < l.Points.Length; i++) {
				l.Points[i] = pos[i];
			}

        }
			
		public void EarseAllLineEffect() {

            foreach (var item in drawnLineList)
            {
                DestroyObject(item.gameObject);
            }
            drawnLineList.Clear();
        }

	}
}

