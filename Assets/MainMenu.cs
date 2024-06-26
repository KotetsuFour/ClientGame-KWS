using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Transform menusHolder;
    [SerializeField] private LobbyScript lobby;
    [SerializeField] private AudioSource menuMusic;

    private int numPlayersPerTeam = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void changeMenu(string menu)
    {
        for (int q = 0; q < menusHolder.childCount; q++)
        {
            menusHolder.GetChild(q).gameObject.SetActive(false);
        }
        StaticData.findDeepChild(menusHolder, menu).gameObject.SetActive(true);
    }

    public void confirmName()
    {
        TMP_InputField field = StaticData.findDeepChild(menusHolder, "NameField").GetComponent<TMP_InputField>();
        if (field.text != null && field.text.Trim() != "")
        {
            StaticData.playerName = field.text;
            StaticData.findDeepChild(menusHolder, "NameDisplay").GetComponent<TextMeshProUGUI>().text = StaticData.playerName;
            changeMenu("Main Menu");
        }
    }

    public void quit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }

    public void pickNumPlayers()
    {
        if (StaticData.playerName == null || StaticData.playerName == "")
        {
            StaticData.findDeepChild(menusHolder, "ErrorMessage").GetComponent<TextMeshProUGUI>()
                .text = "Vous devez créer un profil.";
            changeMenu("Error");
        }
        else
        {
            changeMenu("ChangeNumPlayers");
        }
    }
    public void pickSavedBattle()
    {
        if (StaticData.playerName == null || StaticData.playerName == "")
        {
            StaticData.findDeepChild(menusHolder, "ErrorMessage").GetComponent<TextMeshProUGUI>()
                .text = "Vous devez créer un profil.";
            changeMenu("Error");
        }
        else
        {
            changeMenu("SavedBattles");
        }
    }
    public void createBattle()
    {
        lobby.createLobby(numPlayersPerTeam * 2, $"La Bataille d'{StaticData.playerName}");
        StaticData.findDeepChild(menusHolder, "LobbyName").GetComponent<TextMeshProUGUI>()
            .text = $"La Bataille d'{StaticData.playerName} (1/{numPlayersPerTeam * 2})";
        changeMenu("NewBattle");
    }
    public void joinMenu()
    {
        if (StaticData.playerName == null || StaticData.playerName == "")
        {
            StaticData.findDeepChild(menusHolder, "ErrorMessage").GetComponent<TextMeshProUGUI>()
                .text = "Vous devez créer un profil.";
            changeMenu("Error");
        }
        else
        {
            changeMenu("JoinMenu");
            updateLobbiesList();
        }
    }
    public void backFromJoinMenu()
    {
        lobby.discardJoinButtons();
        StaticData.findDeepChild(menusHolder, "JoinList").DetachChildren();
        changeMenu("Main Menu");
    }
    public void changeNumPlayers(int change)
    {
        numPlayersPerTeam = Mathf.Clamp(numPlayersPerTeam + change, 1, 4);
        StaticData.findDeepChild(menusHolder, "NumPlayers").GetComponent<TextMeshProUGUI>()
            .text = "" + numPlayersPerTeam;
    }

    public void changeVolume(float val)
    {
        menuMusic.volume = val;
    }

    public void updateLobbiesList()
    {
        if (lobby.joinButtons != null && lobby.joinButtons.Count > 0)
        {
            lobby.discardJoinButtons();
        }
        lobby.searchJoins(menusHolder);
    }

    // Update is called once per frame
    void Update()
    {
        if (lobby.lobby != null || lobby.joinedLobby != null)
        {
            lobby.updateLobbyNameWhileWaiting(StaticData.findDeepChild(menusHolder, "LobbyName").GetComponent<TextMeshProUGUI>(),
                StaticData.findDeepChild(menusHolder, "JoinedLobbyName").GetComponent<TextMeshProUGUI>());
        }
    }


}
