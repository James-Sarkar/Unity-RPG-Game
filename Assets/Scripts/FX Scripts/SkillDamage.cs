using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillDamage : MonoBehaviour {

	public LayerMask enemyLayer;

	public float radius = 0.5f, damageCount = 10f;

	private EnemyHealth enemyHealth;

	private bool collided;

	void Update () {
		Collider[] hits = Physics.OverlapSphere (transform.position, radius, enemyLayer);

		foreach (Collider c in hits) {
			enemyHealth = c.gameObject.GetComponent<EnemyHealth> ();

			collided = true;
		}

		if (collided) {
			enemyHealth.TakeDamage (damageCount);

			enabled = false;
		}
	}
}