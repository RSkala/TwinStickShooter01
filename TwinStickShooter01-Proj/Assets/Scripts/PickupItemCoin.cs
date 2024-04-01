using UnityEngine;

public class PickupItemCoin : PickupItemBase
{
    protected override void OnPlayerPickedUp(PlayerController enteredPlayer)
    {
        AudioManager.Instance.PlaySound(AudioManager.SFX.CoinPickup);
        base.OnPlayerPickedUp(enteredPlayer);
    }
}
