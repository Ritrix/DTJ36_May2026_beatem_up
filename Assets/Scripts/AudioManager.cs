using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private AudioClip gruntAttack;
    [SerializeField] private AudioClip playerHurt;
    [SerializeField] private AudioClip jumperJump;
    [SerializeField] private AudioClip jumperFall;
    [SerializeField] private AudioClip enemyHurt;
    [SerializeField] private AudioClip coinPickup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayGruntAttack() => PlaySFX(gruntAttack);
    public void PlayPlayerHurt() => PlaySFX(playerHurt);
    public void PlayJumperJump() => PlaySFX(jumperJump);
    public void PlayJumperFall() => PlaySFX(jumperFall);
    public void PlayEnemyHurt() => PlaySFX(enemyHurt);
    public void PlayCoinPickup() => PlaySFX(coinPickup);

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }
}