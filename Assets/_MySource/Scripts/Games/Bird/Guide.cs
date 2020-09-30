using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Bird
{
    public class Guide : MonoBehaviour
    {
        public void Close()
        {
            gameObject.SetActive(false);
        }

    }
}