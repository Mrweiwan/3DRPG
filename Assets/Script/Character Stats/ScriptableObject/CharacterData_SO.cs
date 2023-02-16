using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Data",menuName ="Character Stats/Data")]
public class CharacterData_SO : ScriptableObject
{
    [Header("Stats Info")]
    public int maxHealth;//最大血量
    
    public int currentHealth;//当前血量
    
    public int baseDefence;//基础防御

    public int currentDefence;//当前防御

    [Header("Kill")]
    
    public int killPoint;

    [Header("Level")]

    public int currentLevel;//当前等级

    public int maxLevel;//最大等级

    public int baseExp;//基础经验值

    public int currentExp;//当前经验值

    public float levelBuff;//升级加成

    public float LevelMultiplier {
        get {
            return 1 + (currentLevel - 1) * levelBuff;
        }
    }
    //更新经验值
    public void UpdateExp(int point) {

        currentExp += point;
        if (currentExp>= baseExp)
        {
            LeveUp();
        }
    }
    //升级
    private void LeveUp()
    {
        currentLevel = Mathf.Clamp(currentLevel + 1, 0, maxLevel);
        baseExp += (int)(baseExp * LevelMultiplier);
        maxHealth = (int)(maxHealth * LevelMultiplier);
        currentHealth = maxHealth;
        Debug.Log("Level UP!" + currentHealth + "Max Health" + maxHealth);
    }
}
