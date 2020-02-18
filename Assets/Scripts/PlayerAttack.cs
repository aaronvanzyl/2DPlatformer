using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
	public PlayerController controller;
    List<Enemy> alreadyHit = new List<Enemy>();

	void OnTriggerEnter2D(Collider2D other) {
        Enemy e = other.GetComponent<Enemy>();
        if(e!=null && !alreadyHit.Contains(e))
		{
            alreadyHit.Add(e);
            e.ReceiveHit();
            controller.OnHitEnemy(e);
		}
	}

}
