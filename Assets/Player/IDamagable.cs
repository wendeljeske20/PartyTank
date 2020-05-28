public interface IDamagable
{
	Team Team { get; set; }

	void TakeDamage(int damage, PlayerData ownerPlayer);

	void SendTakeDamage(int damage, PlayerData ownerPlayer);
}
