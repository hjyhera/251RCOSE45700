using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public enum ItemType
    {
        ExtraBomb,
        BlastRadius,
        SpeedIncrease,
    }

    public ItemType type;    private void OnItemPickup(GameObject player)
    {
        switch (type)
        {
            case ItemType.ExtraBomb:
                BombController bombController = player.GetComponent<BombController>();
                if (bombController != null)
                {
                    bombController.AddBomb();
                }
                break;

            case ItemType.BlastRadius:
                BombController bombController2 = player.GetComponent<BombController>();
                if (bombController2 != null)
                {
                    bombController2.explosionRadius++;
                }
                break;

            case ItemType.SpeedIncrease:
                // Try both movement controllers
                MovementController movementController = player.GetComponent<MovementController>();
                if (movementController != null)
                {
                    movementController.speed++;
                }
                else
                {
                    TilemapMovementController tilemapMovementController = player.GetComponent<TilemapMovementController>();
                    if (tilemapMovementController != null)
                    {
                        tilemapMovementController.speed++;
                    }
                }
                break;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")){
            OnItemPickup(other.gameObject);
        }
    }
}
