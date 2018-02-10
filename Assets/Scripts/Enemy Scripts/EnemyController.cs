using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState {
	IDLE,
	WALK,
	RUN,
	PAUSE,
	GOBACK,
	ATTACK,
	DEATH
}

public class EnemyController : MonoBehaviour {
	
	[HideInInspector]
	public EnemyState enemyCurrentState = EnemyState.IDLE;

	private float attackDistance = 1.5f, alertAttackDistance = 8f, followDistance = 15f, enemyToPlayerDistance, 
	moveSpeed = 2f, walkSpeed = 1f, currentAttackTime, waitAttackTime = 1f;

	private EnemyState enemyLastState = EnemyState.IDLE;

	private Transform playerTarget;

	private Vector3 initialPosition, whereToMove = Vector3.zero, whereToNavigate;

	private CharacterController charController;

	private Animator anim;

	private bool finishedAnimation = true, finishedMovement = true;

	private NavMeshAgent navAgent;

	private EnemyHealth enemyHealth;

	void Awake () {
		playerTarget = GameObject.FindGameObjectWithTag ("Player").transform;

		navAgent = GetComponent<NavMeshAgent> ();

		charController = GetComponent<CharacterController> ();

		anim = GetComponent<Animator> ();

		initialPosition = transform.position;

		whereToNavigate = transform.position;

		enemyHealth = GetComponent<EnemyHealth> ();
	}

	void Update () {
		if (enemyHealth.health <= 0f) {
			enemyCurrentState = EnemyState.DEATH;
		}

		if (enemyCurrentState != EnemyState.DEATH) {
			enemyCurrentState = SetEnemyState (enemyCurrentState, enemyLastState, enemyToPlayerDistance);

			if (finishedMovement) {
				GetStateControl (enemyCurrentState);
			} else {
				if (!anim.IsInTransition (0) && anim.GetCurrentAnimatorStateInfo (0).IsName ("Idle")) {
					finishedMovement = true;
				} else if (!anim.IsInTransition (0) && anim.GetCurrentAnimatorStateInfo (0).IsTag ("Atk1")
					|| anim.GetCurrentAnimatorStateInfo (0).IsTag ("Atk2")) {
					anim.SetInteger ("Atk", 0);
				}
			}
		} else {
			anim.SetBool ("Death", true);

			charController.enabled = false;

			navAgent.enabled = false;

			if (!anim.IsInTransition (0) && anim.GetCurrentAnimatorStateInfo (0).IsName ("Death")
				&& anim.GetCurrentAnimatorStateInfo (0).normalizedTime >= 0.95f) {
				Destroy (gameObject, 2f);
			}
		}
	}

	EnemyState SetEnemyState (EnemyState curState, EnemyState lastState, float enemyToPlayerDis) {
		enemyToPlayerDis = Vector3.Distance (transform.position, playerTarget.position);

		float initialDistance = Vector3.Distance (initialPosition, transform.position);

		if (initialDistance > followDistance) {
			lastState = curState;

			curState = EnemyState.GOBACK;
		} else if (enemyToPlayerDis <= attackDistance) {
			lastState = curState;

			curState = EnemyState.ATTACK;
		} else if (enemyToPlayerDis >= alertAttackDistance &&
			lastState == EnemyState.PAUSE || lastState == EnemyState.ATTACK) {
			lastState = curState;

			curState = EnemyState.PAUSE;
		} else if (enemyToPlayerDis <= alertAttackDistance && enemyToPlayerDis > attackDistance) {
			if (curState != EnemyState.GOBACK || lastState == EnemyState.WALK) {
				lastState = curState;

				curState = EnemyState.PAUSE;
			}
		} else if (enemyToPlayerDis > alertAttackDistance &&
			lastState != EnemyState.GOBACK && lastState != EnemyState.PAUSE) {
			lastState = curState;

			curState = EnemyState.WALK;
		}

		return curState;
	}

	void GetStateControl (EnemyState curState) {
		if (curState == EnemyState.RUN || curState == EnemyState.PAUSE) {
			if (curState != EnemyState.ATTACK) {
				Vector3 targetPosition = new Vector3 (playerTarget.position.x, transform.position.y,
					playerTarget.position.z);
			
				if (Vector3.Distance (transform.position, targetPosition) >= 2.1f) {
					anim.SetBool ("Walk", false);
					anim.SetBool ("Run", true);

					navAgent.SetDestination (targetPosition);
				}
			}
		} else if (curState == EnemyState.ATTACK) {
			anim.SetBool ("Run", false);

			whereToMove.Set (0f, 0f, 0f);

			navAgent.SetDestination (transform.position);

			transform.rotation = Quaternion.Slerp (transform.rotation,
				Quaternion.LookRotation (playerTarget.position - transform.position), 5f * Time.deltaTime);

			if (currentAttackTime >= waitAttackTime) {
				int atkRange = Random.Range (1, 3);

				anim.SetInteger ("Atk", atkRange);

				finishedAnimation = false;

				currentAttackTime = 0f;
			} else {
				anim.SetInteger ("Atk", 0);

				currentAttackTime += Time.deltaTime;
			}
		} else if (curState == EnemyState.GOBACK) {
			anim.SetBool ("Run", true);

			Vector3 targetPosition = new Vector3 (initialPosition.x, transform.position.y,
				initialPosition.z);

			navAgent.SetDestination (targetPosition);

			if (Vector3.Distance (targetPosition, initialPosition) <= 3.5f) {
				enemyLastState = curState;
			
				curState = EnemyState.WALK;
			}
		} else if (curState == EnemyState.WALK) {
			anim.SetBool ("Run", false);
			anim.SetBool ("Walk", true);

			if (Vector3.Distance (transform.position, whereToNavigate) <= 2f) {
				whereToNavigate.x = Random.Range (initialPosition.x - 5f, initialPosition.x + 5f);
				whereToNavigate.z = Random.Range (initialPosition.z - 5f, initialPosition.z + 5f);
			} else {
				navAgent.SetDestination (whereToNavigate);	
			}
		} else {
			anim.SetBool ("Run", false);
			anim.SetBool ("Walk", false);
	
			whereToMove.Set (0f, 0f, 0f);

			navAgent.isStopped = true;
		}

		//		charController.Move (whereToMove);
	}
}
