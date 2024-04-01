using UnityEngine;

public class EnemySlug : EnemyBase
{
    protected override void PlayDeathSound()
    {
        AudioManager.Instance.PlaySound(AudioManager.SFX.TinySlugDeath);
    }
}
