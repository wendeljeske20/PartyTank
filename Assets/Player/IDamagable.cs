public interface IDamagable
{
	Team team { get; set; }

	void TakeDamage(int damage);

	void SendTakeDamage(int damage);
}
