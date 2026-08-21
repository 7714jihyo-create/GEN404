using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonDown : MonoBehaviour
{
    int cnt = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartButtonDown()
    {
        SceneManager.LoadScene("GameScene");
    }
    public void NextButtonDown()
    {
        SceneManager.LoadScene("WaitingScene");
    }
    public void NextButtonDown2()
    {
        SceneManager.LoadScene("GameScene");
        cnt++;
        if (cnt == 3)
        {
            SceneManager.LoadScene("ResultScene");
        }
    }
}
