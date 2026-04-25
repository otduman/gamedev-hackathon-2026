using UnityEngine;

// Procedural sound effects — no audio assets required.
// Spawned automatically by HexGridManager after grid generation.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource _source;

    private AudioClip _attackClip;
    private AudioClip _conquerClip;
    private AudioClip _turnStartClip;
    private AudioClip _migrationClip;

    private const float MasterVolume = 0.22f;

    private void Awake()
    {
        
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        
        DontDestroyOnLoad(gameObject);

        _source = gameObject.AddComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.volume      = MasterVolume;

        int rate = AudioSettings.outputSampleRate;
        _attackClip    = BuildClip("Atk",    GenerateAttack(rate),    rate);
        _conquerClip   = BuildClip("Conq",   GenerateConquer(rate),   rate);
        _turnStartClip = BuildClip("Turn",   GenerateTurnStart(rate), rate);
        _migrationClip = BuildClip("Migr",   GenerateMigration(rate), rate);
    }

    public void PlayAttack()    => Play(_attackClip);
    public void PlayConquer()   => Play(_conquerClip);
    public void PlayTurnStart() => Play(_turnStartClip);
    public void PlayMigration() => Play(_migrationClip);

    private void Play(AudioClip clip)
    {
        if (clip != null && _source != null)
            _source.PlayOneShot(clip, MasterVolume);
    }

    // -------------------------------------------------------------------------
    // Clip builders
    // -------------------------------------------------------------------------

    private static AudioClip BuildClip(string name, float[] samples, int rate)
    {
        AudioClip clip = AudioClip.Create(name, samples.Length, 1, rate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // Short percussive hit: decaying 200 Hz sine, 0.15 s
    private static float[] GenerateAttack(int rate)
    {
        int len = (int)(rate * 0.15f);
        float[] s = new float[len];
        for (int i = 0; i < len; i++)
        {
            float t   = (float)i / rate;
            float env = Mathf.Exp(-t * 22f);
            s[i] = env * Mathf.Sin(2f * Mathf.PI * 200f * t);
        }
        return s;
    }

    // Three ascending triumphant tones (330 → 440 → 550 Hz), 0.6 s total
    private static float[] GenerateConquer(int rate)
    {
        float[] freqs  = { 330f, 440f, 550f };
        int     segLen = (int)(rate * 0.2f);
        float[] s      = new float[freqs.Length * segLen];
        for (int f = 0; f < freqs.Length; f++)
        {
            for (int i = 0; i < segLen; i++)
            {
                float t   = (float)i / rate;
                float env = Mathf.Sin(Mathf.PI * (float)i / segLen); // fade in + out per segment
                s[f * segLen + i] = env * Mathf.Sin(2f * Mathf.PI * freqs[f] * t) * 0.75f;
            }
        }
        return s;
    }

    // Soft bell: C5 (523 Hz) with exponential decay, 0.4 s
    private static float[] GenerateTurnStart(int rate)
    {
        int len = (int)(rate * 0.4f);
        float[] s = new float[len];
        for (int i = 0; i < len; i++)
        {
            float t = (float)i / rate;
            s[i] = Mathf.Exp(-t * 5f) * Mathf.Sin(2f * Mathf.PI * 523f * t) * 0.5f;
        }
        return s;
    }

    // Frequency-glide whoosh: 300 → 150 Hz, 0.25 s
    private static float[] GenerateMigration(int rate)
    {
        int len = (int)(rate * 0.25f);
        float[] s     = new float[len];
        float   phase = 0f;
        for (int i = 0; i < len; i++)
        {
            float freq = Mathf.Lerp(300f, 150f, (float)i / len);
            float env  = Mathf.Sin(Mathf.PI * (float)i / len);
            phase += 2f * Mathf.PI * freq / rate;
            s[i] = env * Mathf.Sin(phase) * 0.55f;
        }
        return s;
    }
}
