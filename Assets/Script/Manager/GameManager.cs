using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameManager : Singleton<GameManager>
{
    public CharacterStats playerStats;
    private CinemachineFreeLook followCamera;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }
    public List<IEndGameObserver> endGameObservers = new List<IEndGameObserver>();
    public void RigisterPlayer(CharacterStats player) {
        playerStats = player;
        followCamera = FindObjectOfType<CinemachineFreeLook>();
        if (followCamera != null)
        {
            followCamera.Follow = playerStats.transform.GetChild(2);
            followCamera.LookAt = playerStats.transform.GetChild(2);
        }
    }
    public void AddObserver(IEndGameObserver observer) {
        endGameObservers.Add(observer);
    }
    public void RemoveObserver(IEndGameObserver observer) {
        endGameObservers.Remove(observer);
    }
    public void NodifyObserver() 
    {
        foreach (var observer in endGameObservers)
        {
            observer.EndNodify();
        }
    }

    public Transform GetEntrance() 
    {
        foreach (var item in FindObjectsOfType<TransitionDestination>())
        {
            if (item.destinationTag==TransitionDestination.DestinationTag.Enter)
            {
                return item.transform;
            }
        }
        return null;
    }
}
