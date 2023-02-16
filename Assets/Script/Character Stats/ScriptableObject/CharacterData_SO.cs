using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Data",menuName ="Character Stats/Data")]
public class CharacterData_SO : ScriptableObject
{
    [Header("Stats Info")]
    public int maxHealth;//���Ѫ��
    
    public int currentHealth;//��ǰѪ��
    
    public int baseDefence;//��������

    public int currentDefence;//��ǰ����

    [Header("Kill")]
    
    public int killPoint;

    [Header("Level")]

    public int currentLevel;//��ǰ�ȼ�

    public int maxLevel;//���ȼ�

    public int baseExp;//��������ֵ

    public int currentExp;//��ǰ����ֵ

    public float levelBuff;//�����ӳ�

    public float LevelMultiplier {
        get {
            return 1 + (currentLevel - 1) * levelBuff;
        }
    }
    //���¾���ֵ
    public void UpdateExp(int point) {

        currentExp += point;
        if (currentExp>= baseExp)
        {
            LeveUp();
        }
    }
    //����
    private void LeveUp()
    {
        currentLevel = Mathf.Clamp(currentLevel + 1, 0, maxLevel);
        baseExp += (int)(baseExp * LevelMultiplier);
        maxHealth = (int)(maxHealth * LevelMultiplier);
        currentHealth = maxHealth;
        Debug.Log("Level UP!" + currentHealth + "Max Health" + maxHealth);
    }
}
