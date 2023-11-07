using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI MainDisplayText;
    public GameObject RestartButton;
    public GameObject HowToScreen;
    public GameObject BackupCamImage;

    public ChallengeItem ChallengeItemPrefab;
    public Transform ChallengeItemsParent;

    Dictionary<ChallengeType, Toggle> challengeProgressToggles = new Dictionary<ChallengeType, Toggle>();
    float textDisplayWaitTime = 6f;
    Coroutine hideTextCoroutine;

    private void Start()
    {
        StartCoroutine(waitToHideStartScreen());
    }

    IEnumerator waitToHideStartScreen()
    {
        yield return new WaitForSecondsRealtime(2f);
        HowToScreen.SetActive(false);
    }

    public void CreateChallengeItem(ChallengeType challengeType, string challengeText)
    {
        ChallengeItem challengeItem = Instantiate(ChallengeItemPrefab, ChallengeItemsParent);
        challengeItem.Text.text = challengeText;
        challengeProgressToggles[challengeType] = challengeItem.Toggle;
    }

    public void DisplayMainText(string text)
    {
        MainDisplayText.text = text;
        if(hideTextCoroutine != null)
        {
            StopCoroutine(hideTextCoroutine);
        }
        hideTextCoroutine = StartCoroutine(displayTextWait());
    }

    IEnumerator displayTextWait()
    {
        yield return new WaitForSecondsRealtime(textDisplayWaitTime);
        MainDisplayText.text = "";
    }

    public void OnChallengeComplete(ChallengeType challengeType)
    {
        challengeProgressToggles[challengeType].isOn = true;
    }

    public void OnPlayerCrashed()
    {
        if (hideTextCoroutine != null)
        {
            StopCoroutine(hideTextCoroutine);
        }
        MainDisplayText.text = "Crashed!";
        RestartButton.SetActive(true);
    }

    public void DisableBackupCam()
    {
        BackupCamImage.SetActive(false);
    }

    public void OnRestartButtonPressed()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
