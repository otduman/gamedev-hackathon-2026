using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientEnvironment : MonoBehaviour
{
    private Light _sunLight;
    private float _baseLightIntensity;
    private float _lightNoiseOffset;

    private struct TileSwayData
    {
        public Transform visual;
        public float phaseOffset;
        public float speed;
        public float baseRotY;
    }

    private List<TileSwayData> _swayTargets = new List<TileSwayData>();

    private void Start()
    {
        SetupDustParticles();
        SetupLight();
        StartCoroutine(FindTilesNextFrame());
    }

    private void SetupDustParticles()
    {
        GameObject psGo = new GameObject("AmbientDust");
        psGo.transform.SetParent(transform);
        psGo.transform.localPosition = new Vector3(0f, 1.5f, 0f);

        ParticleSystem ps = psGo.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(8f, 14f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.25f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.09f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.85f, 0.72f, 0.45f, 0.18f),
            new Color(0.92f, 0.83f, 0.60f, 0.35f)
        );
        main.maxParticles = 80;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 6f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(20f, 0.5f, 20f);

        var velocityOverLifetime = ps.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(0.01f, 0.06f);
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-0.05f, 0.10f);

        var renderer = psGo.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                             ?? Shader.Find("Particles/Standard Unlit");
        Material mat = particleShader != null ? new Material(particleShader) : null;
        if (mat != null)
        {
            mat.SetFloat("_Mode", 2);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;
            renderer.material = mat;
        }

        ps.Play();
    }

    private void SetupLight()
    {
        _sunLight = FindAnyObjectByType<Light>();
        if (_sunLight != null)
        {
            _baseLightIntensity = _sunLight.intensity;
            _lightNoiseOffset = Random.Range(0f, 100f);
        }
    }

    private IEnumerator FindTilesNextFrame()
    {
        yield return null;

        GameObject[] allObjects = FindObjectsByType<GameObject>();
        foreach (GameObject go in allObjects)
        {
            if (!go.name.StartsWith("Tile_")) continue;
            if (go.transform.childCount == 0) continue;

            Transform visual = go.transform.GetChild(0);
            _swayTargets.Add(new TileSwayData
            {
                visual = visual,
                phaseOffset = Random.Range(0f, Mathf.PI * 2f),
                speed = 0.8f + Random.Range(0f, 0.4f),
                baseRotY = visual.localEulerAngles.y
            });
        }
    }

    private void Update()
    {
        BreathLight();
        SwayTiles();
    }

    private void BreathLight()
    {
        if (_sunLight == null) return;
        float noise = Mathf.PerlinNoise(Time.time * 0.15f + _lightNoiseOffset, 0f);
        float oscillation = (noise - 0.5f) * 2f * 0.08f;
        _sunLight.intensity = _baseLightIntensity * (1f + oscillation);
    }

    private void SwayTiles()
    {
        float t = Time.time;
        for (int i = 0; i < _swayTargets.Count; i++)
        {
            TileSwayData data = _swayTargets[i];
            if (data.visual == null) continue;

            float angle = Mathf.Sin(t * data.speed + data.phaseOffset) * 3f;
            Vector3 euler = data.visual.localEulerAngles;
            euler.y = data.baseRotY + angle;
            data.visual.localEulerAngles = euler;
        }
    }
}
