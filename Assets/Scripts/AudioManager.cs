using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // Inspector override — leave empty to use the auto-loaded paths below.
    [SerializeField] private AudioClip _combatClip;
    [SerializeField] private AudioClip _conqueClip;
    [SerializeField] private AudioClip _diceClip;
    [SerializeField] private AudioClip _buttonClip;

    private AudioSource _source;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);

        _source             = gameObject.AddComponent<AudioSource>();
        _source.playOnAwake = false;

        // SFX live in Assets/Resources/sfx/ so they're available in built players
        // (AssetDatabase only works in the Editor). Inspector-assigned clips
        // still take precedence.
        if (_combatClip == null) _combatClip = Resources.Load<AudioClip>("sfx/sfx_combat");
        if (_conqueClip == null) _conqueClip = Resources.Load<AudioClip>("sfx/sfx_conque");
        if (_diceClip   == null) _diceClip   = Resources.Load<AudioClip>("sfx/sfx_dice");
        if (_buttonClip == null) _buttonClip = Resources.Load<AudioClip>("sfx/sfx_button");
    }

    public void PlayCombatSound()  => Play(_combatClip);
    public void PlayConquerSound() => Play(_conqueClip);
    public void PlayDiceSound()    => Play(_diceClip);
    public void PlayButtonSound()  => Play(_buttonClip);

    private void Play(AudioClip clip)
    {
        if (clip != null && _source != null)
            _source.PlayOneShot(clip);
    }
}
