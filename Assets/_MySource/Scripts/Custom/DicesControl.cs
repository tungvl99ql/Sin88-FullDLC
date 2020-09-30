using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Server.Api
{
    public class DicesControl : MonoBehaviour
    {

        public void SpinDices(int id)
        {
            App.trace(id, "red");
        }
    }
}
