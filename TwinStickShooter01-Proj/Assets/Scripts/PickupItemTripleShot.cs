using UnityEngine;

public class PickupItemTripleShot : PickupItemBase
{
    protected override void OnPlayerPickedUp(PlayerController enteredPlayer)
    {
        enteredPlayer.ActivateTripleShot();
        base.OnPlayerPickedUp(enteredPlayer);
    }
}
