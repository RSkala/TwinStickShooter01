using UnityEngine;

public class EnemyTinySlug : EnemyBase
{
    protected override void PlayDeathSound()
    {
        AudioManager.Instance.PlaySound(AudioManager.SFX.TinySlugDeath);
    }
}
