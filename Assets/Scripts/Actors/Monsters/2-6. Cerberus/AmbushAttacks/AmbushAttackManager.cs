using Infrastructure;
using UnityEngine;

namespace Actors.Monsters.Bosses
{
    public class AmbushAttackManager : MonoBehaviour
    {
        [SerializeField] private GameObject _indicator;
        [SerializeField] private GameObject _smokeEffect;

        public void ShowIndicator()
        {
            var indicator = Instantiate(_indicator);
            indicator.transform.position = _indicator.transform.position;
            indicator.SetActive(true);

            new Timer(5, _ =>
            {
                if (indicator)
                    Destroy(indicator);
            });
        }

        public void ShowSmokeEffect()
        {
            var smokeEffct = Instantiate(_smokeEffect);
            smokeEffct.transform.position = _smokeEffect.transform.position;
            smokeEffct.SetActive(true);

            new Timer(10, _ =>
            {
                if (smokeEffct)
                    Destroy(smokeEffct);
            });
        }
    }
}
