using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
	Team team { get; }
	void TakeDamage(float damage);
}
