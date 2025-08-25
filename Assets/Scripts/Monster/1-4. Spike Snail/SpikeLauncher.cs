using System;
using System.Linq;
using UnityEngine;

internal class SpikeLauncher : MonoBehaviour
{
    // Property
    [SerializeField] private GameObject[] spikes;

    // Internal
    private Vector3[] spikePositions;

    private SpikeSnail owner;
    private PlatformManager platformManager;
    private string[] collisionTags;


    // Content
    private void Start()
    {
        if (spikes.Length != 5)
            throw new InvalidOperationException("가시달팽이는 5개의 spikes를 가지고 있어야 합니다.");

        spikePositions = new Vector3[spikes.Length];

        for (int i = 0; i < spikes.Length; i++)
        {
            spikePositions[i] = spikes[i].transform.localPosition;
            spikes[i].SetActive(false);
        }
    }

    public virtual void Initialize(SpikeSnail owner, PlatformManager platformManager, params string[] collisionTags)
    {
        this.owner = owner;
        this.platformManager = platformManager;
        this.collisionTags = collisionTags;
    }


    public void Launch()
    {
        var spikes = this.spikes.Select((spike, i) =>
        {
            var _spike = Instantiate(spike);

            _spike.transform.position = owner.transform.position + spikePositions[i];
            _spike.SetActive(true);

            return _spike.GetComponent<Spike>().Initialize<Spike>(platformManager, collisionTags);
        }).ToArray();

        spikes[0].Launch(new Vector2(1, 0), owner.spikeSpeed);
        spikes[1].Launch(new Vector2(1, 1), owner.spikeSpeed);
        spikes[2].Launch(new Vector2(0, 1), owner.spikeSpeed);
        spikes[3].Launch(new Vector2(-1, 1), owner.spikeSpeed);
        spikes[4].Launch(new Vector2(-1, 0), owner.spikeSpeed);
    }
}
