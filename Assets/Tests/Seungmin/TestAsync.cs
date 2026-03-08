using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Tests.Seungmin
{
    public class TestAsync : MonoBehaviour
    {
        CancellationTokenSource _cts;

        private void Start()
        {
            _cts = new();
            DoTest(succeeded => print(succeeded));
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                _cts.Cancel();
        }

        public void DoTest(Action<bool> callback)
        {
            _ = DoTestWrapper(callback);

            async Task DoTestWrapper(Action<bool> callback)
            {
                try
                {
                    await DoTestAsync(CancellationToken.None);
                    callback?.Invoke(true);
                }
                catch
                {
                    callback?.Invoke(false);
                }
            }
        }

        public async Task DoTestAsync(CancellationToken cancellationToken)
        {
            int i = 0;

            while (i < 5)
            {
                await Task.Delay(1000, cancellationToken);

                print(i + " " + name);
                i++;
            }
        }
    }
}
