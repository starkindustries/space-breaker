﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cinemachine;

public class EndlessModeManager : MonoBehaviour
{
    // Camera
    // public float sceneWidth = 12f;
    // public CinemachineVirtualCamera vcamera;

    // Pause button and menu
    public GameObject pauseButton;
    public GameObject pauseMenu;
    private bool gameIsPaused;

    // Game over menu
    public GameObject gameOverMenu;
    public TextMeshProUGUI gameOverScoreText;

    // Joystick
    public GameObject joystick;    

    // Singleton pattern
    // https://gamedev.stackexchange.com/a/116010/123894
    private static EndlessModeManager _instance;
    public static EndlessModeManager Instance { get { return _instance; } }

    
    public Player player;
    public EndlessMode endlessMode;
    public GameObject coinPrefab;

    private void Awake()
    {
        // Singleton Enforcement Code
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            _instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        /*
        // Adjust camera to fit field        
        // Reference:
        // https://forum.unity.com/threads/how-to-programmatically-change-virtual-cams-orthographic-size.499491/
        float unitsPerPixel = sceneWidth / Screen.width;
        float desiredHalfHeight = 0.5f * unitsPerPixel * Screen.height;
        vcamera.m_Lens.OrthographicSize = desiredHalfHeight;
        */

        // Hide pause and game over menus on first load
        pauseMenu.SetActive(false);
        gameOverMenu.SetActive(false);
    }

    // Game Actions
    public void DidPressPause()
    {
        Debug.Log("Pause button pressed");

        // Pause player and endless mode
        player.shouldIgnoreInput = true;
        endlessMode.gameIsPaused = true;

        gameIsPaused = true;
        pauseButton.SetActive(false);
        pauseMenu.SetActive(true);
        joystick.SetActive(false);
        Time.timeScale = 0f;
    }

    public void DidPressResume()
    {
        // Resume player and endless mode
        player.shouldIgnoreInput = false;
        endlessMode.gameIsPaused = false;

        gameIsPaused = false;
        pauseButton.SetActive(true);
        pauseMenu.SetActive(false);
        joystick.SetActive(true);
        Time.timeScale = 1f;
    }

    public void DidPressSelectLevel()
    {
        Debug.Log("Did press SelectLevel button.");
        SceneChanger.Instance.FadetoScene(sceneIndex: 1);
        Time.timeScale = 1f;
    }

    public void DidPressMenu()
    {
        Debug.Log("Did press menu button.");        
        SceneChanger.Instance.FadetoScene(sceneIndex: 0);
        Time.timeScale = 1f;
    }

    public void DidTriggerGameOver()
    {
        // Pause game
        gameIsPaused = true;
        endlessMode.gameIsPaused = true;
        player.shouldIgnoreInput = true;
        Time.timeScale = 0f;

        // Show game over menu; disable others
        gameOverMenu.SetActive(true);
        pauseButton.SetActive(false);
        joystick.SetActive(false);        

        // Save player's current level
        GameManager.Instance.IncrementCurrentLevel();        
    }

    public bool GameIsPaused()
    {
        return gameIsPaused;
    }

    public void DestroyedBlock(Block block, Vector3 position)
    {
        Debug.Log("Destroyed block with base hit points: " + block.GetBaseHitPoints());

        float coinDropRate = 0.2f;
        int coinDropCount = 5;

        float shouldDropCoin = Random.Range(0f, 1f);
        if (shouldDropCoin < coinDropRate)
        {
            for(int i = 0; i < coinDropCount; i++)
            {
                Instantiate(coinPrefab, position + new Vector3(i*0.001f, 0), Quaternion.identity);
            }            
        }
    }
}
