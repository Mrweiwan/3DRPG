using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public event Action<int, int> UpdateHealthBarOnAttack;

    public CharacterData_SO templateDate;
    public CharacterData_SO characterData;
    public AttackData_SO attackData;
    [HideInInspector]
    public bool isCritical;
    private void Awake()
    {
        if (templateDate!=null)
        {
            characterData = Instantiate(templateDate);
        }
    }
    #region Read from Data_SO
    public int MaxHealth {
        get { if (characterData != null){return characterData.maxHealth;}else {return 0;}}
        set {characterData.maxHealth = value;}
    }
    public int CurrentHealth
    {
        get { if (characterData != null) { return characterData.currentHealth; } else { return 0; } }
        set { characterData.currentHealth = value; }
    }
    public int BaseDefence
    {
        get { if (characterData != null) { return characterData.baseDefence; } else { return 0; } }
        set { characterData.baseDefence = value; }
    }
    public int CurrentDefence
    {
        get { if (characterData != null) { return characterData.currentDefence; } else { return 0; } }
        set { characterData.currentDefence = value; }
    }
    #endregion
    #region Character Combat
    public void TakeDamage(CharacterStats attacker,CharacterStats defener) {
        int damage = Mathf.Max(attacker.CurrentDamage() - defener.CurrentDefence,0);
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);
        if (attacker.isCritical)
        {
            defener.GetComponent<Animator>().SetTrigger("Hit");
        }
        //update UI
        UpdateHealthBarOnAttack?.Invoke(CurrentHealth, MaxHealth);
        //TODO:¾­Ñéupdate
        if (CurrentHealth<=0)
        {
            attacker.characterData.UpdateExp(characterData.killPoint);
        }
    }
    public void TakeDamage(int damage,CharacterStats defener) {
        int currentDamage = Mathf.Max(damage - defener.CurrentDefence, 0);
        CurrentHealth = Mathf.Max(CurrentHealth - currentDamage, 0);
        UpdateHealthBarOnAttack?.Invoke(CurrentHealth, MaxHealth);
        if (CurrentHealth<=0)
        {
            GameManager.Instance.playerStats.characterData.UpdateExp(characterData.killPoint);
        }
    }

    private int CurrentDamage()
    {
        float coreDamage = UnityEngine.Random.Range(attackData.minDamage, attackData.maxDamage);
        if (isCritical)
        {
            coreDamage *= attackData.criticalMultiplier;
            Debug.Log("±©»÷£¡" + coreDamage);
        }
        return (int)coreDamage;
    }

    #endregion

}
