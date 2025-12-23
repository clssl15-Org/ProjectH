using System;
using System.Collections.Generic;
using Actors.Monsters.Actions;
using Infrastructure;
using Infrastructure.StateMachines.Scp;
using UnityEngine;
using World;
using Scp = Infrastructure.StateMachines.Scp;

namespace Actors.Monsters.Bosses
{
    public partial class Werbellion
    {
        internal class WerbellionPortalAttackAction : MonsterActionComponent
        {
            private Sequence _sequence;
            private float _elapsedTime;


            public WerbellionPortalAttackAction(
                PlatformManager platformManager,
                GameObject spawnersParent,
                IEnumerable<WerbellionPortalAttackSpawner> spawners,
                Func<Vector2> getTargetPosition,
                float spawnGap = 0.7f)
            {
                spawnersParent.SetActive(true);

                _sequence = new(stopped: succeeded =>
                    {
                        if (succeeded)
                            Interrupt(InterruptType.Completed);
                        else
                        {
                            foreach (var spawner in spawners)
                                spawner.RequestStop();

                            Interrupt(InterruptType.Interrupted);
                        }
                    });

                IClip before = null;

                int i = 0;
                foreach (var spawner in spawners)
                {
                    var currentSpawner = spawner;
                    currentSpawner.gameObject.SetActive(false);

                    _sequence.Stopped += succeeded =>
                    {
                        if (!succeeded)
                            currentSpawner.RequestStop();
                    };

                    var clip = new Clip("Launcher " + i)
                    .OnStarted((self, _) =>
                    {
                        currentSpawner.RequestStart();
                        self.Stop();
                    });

                    _sequence.AddAfter(
                        before,
                        clip
                    );
                    _sequence.AddAfter(
                        clip, new Scp.Delay(spawnGap).AssignTo(out before));

                    i++;
                }
            }

            protected override void OnEnter(object _)
            {
                _sequence.Start();
            }

            protected override void OnUpdate(float deltaTime)
            {
                _sequence.Update(deltaTime);
            }

            protected override void OnInterrupt(InterruptType _) =>
                _sequence.Stop();
        }
    }
}
