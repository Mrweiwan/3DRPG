using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    Button newGameBtn;
    Button continueBtn;
    Button quitBtn;
    PlayableDirector director;
    private void Awake()
    {
        newGameBtn = transform.GetChild(1).GetComponent<Button>();
        continueBtn = transform.GetChild(2).GetComponent<Button>();
        quitBtn = transform.GetChild(3).GetComponent<Button>();

        newGameBtn.onClick.AddListener(PlayTimeline);
        continueBtn.onClick.AddListener(ContinueGame); 
        quitBtn.onClick.AddListener(QuitGame);
        director = FindObjectOfType<PlayableDirector>();
        director.stopped += NewGame;
    }
    void PlayTimeline() 
    {
        director.Play();
    }
    void NewGame(PlayableDirector obj) 
    {
        PlayerPrefs.DeleteAll();
        //转换场景
        ScenceController.Instance.TransitionToFirstLevel();
    }
    void ContinueGame() 
    {
        //转换场景，读取进度
        ScenceController.Instance.TransitionToLoadGame();
    }
    void QuitGame() 
    {
        Application.Quit();
        Debug.Log("退出游戏");
    }
}
