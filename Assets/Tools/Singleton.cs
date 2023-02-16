using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;

    public static T Instance {
        get {
            return instance;
        }
    }
    protected virtual void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
            instance = (T)this;
    }
    public static bool IsInitialized {//判断单例模式是否已经生成
        get {
            return instance != null;
        }
    }
    protected virtual void OnDestroy() {//销毁单例
        if (instance == this)
        {
            instance = null;    
        }
    }
}
