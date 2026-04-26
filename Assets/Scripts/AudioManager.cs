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

        // When added via AddComponent at runtime the SerializeField slots are empty.
        // Load from the known project path so no manual Inspector wiring is needed.
#if UNITY_EDITOR
        if (_combatClip == null)
            _combatClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/music/sfx_combat.mp3");
        if (_conqueClip == null)
            _conqueClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/music/sfx_conque.mp3");
        if (_diceClip == null)
            _diceClip   = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/music/sfx_dice.mp3");
        if (_buttonClip == null)
            _buttonClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/music/sfx_button.mp3");
#endif
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
