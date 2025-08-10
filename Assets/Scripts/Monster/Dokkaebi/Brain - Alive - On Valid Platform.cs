using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Infrastructure;

public partial class Dokkaebi
{
    private partial class Brain
    {
        private class OnValidPlatform : Work<Alive>
        {
            // Internal
            public Dokkaebi Dokkaebi => Parent.Dokkaebi;


            // Content
            public OnValidPlatform()
            {
                AddChild(new Idle(), true);
            }

            protected override void Update()
            {
                if (!Parent.CheckPlatform(Direction.Center, out var _))
                {
                    Dokkaebi.BelongingPlatform = -1;
                    Parent.SetNext(typeof(SetPlatform));

                    return;
                }
            }
        }
    }
}
