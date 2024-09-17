using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class THKEvents : MonoBehaviour
{
    void SlashDamagePlayer()
    {
        if (PlayerController.Instance.transform.position.x > transform.position.x ||
            PlayerController.Instance.transform.position.x < transform.position.x)
        {
            Hit(TheVoidKnight.Instance.SideAttackTransform, TheVoidKnight.Instance.SideAttackArea);
        }
        else if (PlayerController.Instance.transform.position.y > transform.position.y)
        {
            Hit(TheVoidKnight.Instance.UpAttackTransform, TheVoidKnight.Instance.UpAttackArea);
        }
        else if (PlayerController.Instance.transform.position.y < transform.position.y)
        {
            Hit(TheVoidKnight.Instance.DownAttackTransform, TheVoidKnight.Instance.DownAttackArea);
        }
    }

    void Hit(Transform _attackTransform, Vector2 _attackArea)
    {
        Collider2D[] _objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0);
        for (int i = 0; i < _objectsToHit.Length; i++)
        {
            if (_objectsToHit[i].GetComponent<PlayerController>() != null)
            {
                _objectsToHit[i].GetComponent<PlayerController>().TakeDamage(TheVoidKnight.Instance.damage);
            }
        }
    }
    void Parrying()
    {
        TheVoidKnight.Instance.parrying = true;
    }

    void BendDownCheck()
    {
        if(TheVoidKnight.Instance.barrageAttack)
        {
            StartCoroutine(BarrageAttackTransition());
        }
    }
    
    void BarrageOrOutbreak()
    {
        if(TheVoidKnight.Instance.barrageAttack)
        {
            TheVoidKnight.Instance.StartCoroutine(TheVoidKnight.Instance.Barrage());
        }
    }

    IEnumerator BarrageAttackTransition()
    {
        yield return new WaitForSeconds(1f);
        TheVoidKnight.Instance.anim.SetBool("Cast",true);
    }

    void DestroyAfterDeath()
    {
        TheVoidKnight.Instance.DestroyAfterDeath();
    }
}

