using UnityEngine;

public class PickupItemFiveWave : PickupItemBase
{
    protected override void OnPlayerPickedUp(PlayerController enteredPlayer)
    {
        enteredPlayer.ActivateFiveWave();
        base.OnPlayerPickedUp(enteredPlayer);
    }
}
