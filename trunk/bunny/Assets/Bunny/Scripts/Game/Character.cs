using UnityEngine;
using System.Collections;

// Base class for any character in the game.
// Every interactive character derives from this.

/// <summary>
/// [PASSIVE wont attack unless attacked]
/// [FRIENDLY helps player and follows around]
/// [PACIFIST will try to run away and refuses to fight back]
/// [HOSTILE will attack player if player gets too close]
/// </summary>
public enum NPCBEHAVIOUR { PASSIVE, PACIFIST, FRIENDLY, HOSTILE, PLAYER }

public class Character : MonoBehaviour
{

    public NPCBEHAVIOUR myBehaviour = NPCBEHAVIOUR.PASSIVE;

    public int CharacterHealth = 5;

    public Vector3 DamagePointOffset;
    public Vector3 DamageDirection;
    public bool DebugDamageDirection;
    public float HitDistance = 0.5f;

    public ParticleEmitter GetHitEffect;
    public Vector3 SetHitPoint { set { gotHitPoint = value; }}

    private Vector3 gotHitPoint;
    public Vector3 damageDebugGizmo = Vector3.zero;

    public virtual void Awake()
    {
        NPCLister.Instance.CharacterList.Add(this);
    }

    public virtual void Start()
    {
    }

    public virtual void Update()
    {
        if (CharacterHealth <= 0)
        {
            Death();
        }
    }

    public virtual void FixedUpdate()
    {
    }

    // Debugging.
    public virtual void OnDrawGizmos()
    {
    }

    // Recieve damage methods
    #region Recieve Damage methods and overloads
    public virtual void RecieveDamage(GameObject source, int Amount, float KnockbackForce, ForceMode m)
    {
        if (CharacterHealth > 0)
        {
            this.CharacterHealth -= Amount;
            Vector3 relativeDirection = this.transform.position - source.transform.position;
            relativeDirection = relativeDirection.normalized;
            relativeDirection *= KnockbackForce;
            this.transform.rigidbody.AddForce(relativeDirection, m);
            // Visual effect
            if (GetHitEffect)
            {
                Instantiate(GetHitEffect, gotHitPoint, transform.rotation);
            }
            // Sound effect
            DamageTaken(source);
        }
    }
    #endregion

    /// <summary>
    /// Tells the inherited scripts the source of the damage.
    /// </summary>
    /// <param name="source"></param>
    public virtual void DamageTaken(GameObject source)
    {
        Debug.Log("Not overriden, should be.. Aniamte damage taken.");
    }

    // Damage targets
    /// <summary>
    /// Damage targets, returns the Character it hit
    /// </summary>
    /// <returns>GameObject</returns>
    public GameObject MeleeDamage(GameObject source, int Amount, float KnockbackForce, ForceMode m)
    {
        RaycastHit mRay;
        LayerMask layer = (1 << LayerMask.NameToLayer("NPC")) | (1 << LayerMask.NameToLayer("Player")); // Player and NPC layer.
        if (Physics.Raycast(transform.position + transform.TransformDirection(DamagePointOffset), transform.TransformDirection(Vector3.forward), out mRay, HitDistance, layer))
        {
            damageDebugGizmo = mRay.point;
            Character _char = (Character)mRay.collider.gameObject.GetComponent(typeof(Character));
            if (_char)
            {
                Debug.Log(_char.gameObject.name);
                _char.SetHitPoint = mRay.point;
                _char.RecieveDamage(source, Amount, KnockbackForce, m);
                return mRay.collider.gameObject;
            }
            else
                return null;
        }
        return null;
    }

    /// <summary>
    /// Death.
    /// </summary>
    public virtual void Death()
    {
        // Should be overriden, does not work otherwise.
        Debug.LogWarning("Death not overriden, check your code! " + this.name);
    }
}