using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Infrastructure;

public partial class Dokkaebi
{
    private partial class Brain
    {
        private class Dead : Work<Brain>
        {
            public Dead()
            {

            }

            protected override void Start(params object[] _)
            {
                Debug.Log($"{Parent.Dokkaebi.name} »ç¸Á");
                Parent.Close();
            }
        }
    }
}
