using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;


public class ScenceController : Singleton<ScenceController>,IEndGameObserver
{
    public GameObject playerPrefab;
    GameObject player;
    NavMeshAgent playAgent;
    public SceneFader sceneFaderPrefab;
    bool fadeFinish;//判断淡入淡出是否播放过
  

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }
    private void Start()
    {
        GameManager.Instance.AddObserver(this);
        fadeFinish = true;
    }
    public void TransitionToDestination(TransitionPoint transitionPoint) {
        switch (transitionPoint.transitionType)
        {
            case TransitionPoint.TransitionType.SameScence:
                StartCoroutine(Transition(SceneManager.GetActiveScene().name, transitionPoint.destinationTag));
                break;
            case TransitionPoint.TransitionType.DifferentScence:
                StartCoroutine(Transition(transitionPoint.sceneName, transitionPoint.destinationTag));
                break;
            default:
                break;
        }
    }
    IEnumerator Transition(string scenceName, TransitionDestination.DestinationTag destinationTag)
    {
        //保存数据
        SaveManager.Instance.SavePlayerData();
        if (SceneManager.GetActiveScene().name != scenceName)
        {
            yield return SceneManager.LoadSceneAsync(scenceName);
            yield return Instantiate(playerPrefab, GetDestination(destinationTag).transform.position, GetDestination(destinationTag).transform.rotation);
            //读取数据
            SaveManager.Instance.LoadPlayerData();
            yield break;
        }
        else
        {
            player = GameManager.Instance.playerStats.gameObject;
            playAgent = player.GetComponent<NavMeshAgent>();
            playAgent.enabled = false;
            player.transform.SetPositionAndRotation(GetDestination(destinationTag).transform.position, GetDestination(destinationTag).transform.rotation);
            playAgent.enabled = true;
            yield return null;
        }
    }
    private TransitionDestination GetDestination(TransitionDestination.DestinationTag destinationTag) 
    {
        var entrances = FindObjectsOfType<TransitionDestination>();
        for (int i = 0; i <entrances.Length; i++)
        {
            if (entrances[i].destinationTag == destinationTag)
            {
                return entrances[i];
            }
        }
        return null;
    }
    public void TransitionToMain() 
    {
        StartCoroutine(LoadMain());
    }
    public void TransitionToFirstLevel() 
    {
        StartCoroutine(LoadLevel("SampleScene"));
    }
    public void TransitionToLoadGame() 
    {
        StartCoroutine(LoadLevel(SaveManager.Instance.SceneName));
    }

    IEnumerator LoadLevel(string scene) 
    {
        SceneFader fade = Instantiate(sceneFaderPrefab);
        if (scene != "")
        {
            yield return StartCoroutine(fade.FadeOut(2f));
            yield return SceneManager.LoadSceneAsync(scene);
            yield return player = Instantiate(playerPrefab, GameManager.Instance.GetEntrance().position,GameManager.Instance.GetEntrance().rotation);

            //保存数据
            SaveManager.Instance.SavePlayerData();
            yield return StartCoroutine(fade.FadeIn(2f));
            yield break;
        }
        
    }
    IEnumerator LoadMain() 
    {
        SceneFader fade = Instantiate(sceneFaderPrefab);
        yield return StartCoroutine(fade.FadeOut(2f));
        yield return SceneManager.LoadSceneAsync("Main");
        yield return StartCoroutine(fade.FadeIn(2f));
        yield break;
    }

    public void EndNodify()
    {
        if (fadeFinish)
        {
            fadeFinish = false;
            StartCoroutine(LoadMain());
        }
        
    }
}
