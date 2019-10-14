public class Player : LivingEntity
{
    private PlayerController playerController;

    protected override void Start()
    {
        base.Start();

        playerController = GetComponent<PlayerController>();
    }
}
