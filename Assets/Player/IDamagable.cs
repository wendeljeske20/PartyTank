public interface IDamagable
{
	Team Team { get; set; }

	void TakeDamage(int damage);

	void SendTakeDamage(int damage);
}
