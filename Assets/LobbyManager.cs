using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject unitSelectionTile;

    [SerializeField] private List<Unit> qinUnits;
    [SerializeField] private List<Unit> zhaoUnits;
    private List<Unit> selectableUnits;
    [SerializeField] private List<Person> qinGenerals;
    [SerializeField] private List<Person> zhaoGenerals;
    private List<Person> selectableGenerals;

    private Person currentGeneral;
    private Unit currentUnit;
    private int generalIdx;
    private int unitIdx;

    private int unitsPerPlayer;
    private int recruitmentPoints;
    private int map;
    private int skillPoints;

    private static int[] timeOptions = { 60, 120, 300, 600, -1 };

    private Person myGeneral;
    private List<Unit> myUnits;

    private static int MIN_RECRUITMENT_POINTS = 100;
    private static int MAX_RECRUITMENT_POINTS = 10000;

    private float timePerTurn;

    private NetworkVariable<bool> hostIsDone = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private bool switchedFromHostOptions;

    private NetworkVariable<int> unitsPerPlayerNetwork = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> recruitmentPointsNetwork = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> mapNetwork = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> skillPointsNetwork = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<float> timePerTurnNetwork = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> team1Side = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> team2Side = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    /*
    private NetworkVariable<int[]> teams = new NetworkVariable<int[]>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool[]> readyPlayers = new NetworkVariable<bool[]>(null,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    */
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        Debug.Log("NetworkSpawn");
//        readyPlayers.Value = new bool[NetworkManager.Singleton.ConnectedClients.Count + 1];
        StaticData.findDeepChild(menu.transform, "Ready").GetComponent<Button>().interactable = false;

        StaticData.findDeepChild(menu.transform, "General Choices").gameObject.SetActive(false);
        StaticData.findDeepChild(menu.transform, "Unit Choices").gameObject.SetActive(false);
        StaticData.findDeepChild(menu.transform, "Selected Units").gameObject.SetActive(false);

        myUnits = new List<Unit>();

        if (!IsServer)
        {
            StaticData.findDeepChild(menu.transform, "HostOptions").gameObject.SetActive(false);
            StaticData.findDeepChild(menu.transform, "Waiting").gameObject.SetActive(true);
            StaticData.findDeepChild(menu.transform, "State1").GetComponent<TMP_Dropdown>().interactable = false;
            StaticData.findDeepChild(menu.transform, "State2").GetComponent<TMP_Dropdown>().interactable = false;
        }
        else
        {
            bool done = true;
            /*
            for (int q = 0; q < readyPlayers.Value.Length; q++)
            {
                if (!readyPlayers.Value[q])
                {
                    done = false;
                    break;
                }
            }
            */
            if (done)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("Gameboard",
                    UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!switchedFromHostOptions && hostIsDone.Value)
        {
            StaticData.findDeepChild(menu.transform, "Waiting").gameObject.SetActive(false);
            StaticData.findDeepChild(menu.transform, "Ready").GetComponent<Button>().interactable = true;

            unitsPerPlayer = unitsPerPlayerNetwork.Value;
            recruitmentPoints = recruitmentPointsNetwork.Value;
            map = mapNetwork.Value;
            timePerTurn = timePerTurnNetwork.Value;
            skillPoints = skillPointsNetwork.Value;
            StaticData.findDeepChild(menu.transform, "State1").GetComponent<TMP_Dropdown>().value = team1Side.Value;
            StaticData.findDeepChild(menu.transform, "State2").GetComponent<TMP_Dropdown>().value = team2Side.Value;

            int[] teamSides = new int[] {
                StaticData.findDeepChild(menu.transform, "State1").GetComponent<TMP_Dropdown>().value,
                StaticData.findDeepChild(menu.transform, "State2").GetComponent<TMP_Dropdown>().value
            };
            int myTeam = StaticData.playerId % 2;
            setSelectables(teamSides[myTeam]);

            StaticData.findDeepChild(menu.transform, "General Choices").gameObject.SetActive(true);
            StaticData.findDeepChild(menu.transform, "Unit Choices").gameObject.SetActive(true);
            StaticData.findDeepChild(menu.transform, "Selected Units").gameObject.SetActive(true);

            switchedFromHostOptions = true;
        }
    }

    public void confirmHostChoices()
    {
        Transform numUnitsField = StaticData.findDeepChild(menu.transform, "UnitAmount");
        Transform unitPointsField = StaticData.findDeepChild(menu.transform, "RecruitAmount");
        Transform skillPointsField = StaticData.findDeepChild(menu.transform, "SkillAmount");

        if (numUnitsField.GetComponent<NumberRestricter>().isValid()
            && unitPointsField.GetComponent<NumberRestricter>().isValid()
            && skillPointsField.GetComponent<NumberRestricter>().isValid())
        {
            unitsPerPlayer = int.Parse(numUnitsField.GetComponent<TMP_InputField>().text);
            recruitmentPoints = Mathf.Min(int.Parse(unitPointsField.GetComponent<TMP_InputField>().text), MAX_RECRUITMENT_POINTS);
            map = StaticData.findDeepChild(menu.transform, "MapOptions").GetComponent<TMP_Dropdown>().value;
            timePerTurn = timeOptions[StaticData.findDeepChild(menu.transform, "TimeOptions").GetComponent<TMP_Dropdown>().value];
            skillPoints = int.Parse(skillPointsField.GetComponent<TMP_InputField>().text);

            unitsPerPlayerNetwork.Value = unitsPerPlayer;
            recruitmentPointsNetwork.Value = recruitmentPoints;
            mapNetwork.Value = map;
            timePerTurnNetwork.Value = timePerTurn;
            skillPointsNetwork.Value = skillPoints;

            StaticData.findDeepChild(menu.transform, "HostOptions").gameObject.SetActive(false);
            StaticData.findDeepChild(menu.transform, "General Choices").gameObject.SetActive(true);
            StaticData.findDeepChild(menu.transform, "Unit Choices").gameObject.SetActive(true);
            StaticData.findDeepChild(menu.transform, "Selected Units").gameObject.SetActive(true);

            StaticData.findDeepChild(menu.transform, "Ready").GetComponent<Button>().interactable = true;

            int[] teamSides = new int[] {
                StaticData.findDeepChild(menu.transform, "State1").GetComponent<TMP_Dropdown>().value,
                StaticData.findDeepChild(menu.transform, "State2").GetComponent<TMP_Dropdown>().value
            };
            setSelectables(teamSides[0]);

            StaticData.findDeepChild(menu.transform, "State1").GetComponent<TMP_Dropdown>().interactable = false;
            StaticData.findDeepChild(menu.transform, "State2").GetComponent<TMP_Dropdown>().interactable = false;


            changeGeneral(0);
            changeUnit(0);

            hostIsDone.Value = true;
        }
    }
    private void setSelectables(int teamSide)
    {
        if (teamSide == 0)
        {
            selectableGenerals = qinGenerals;
            selectableUnits = qinUnits;
        }
        else if (teamSide == 1)
        {
            selectableGenerals = zhaoGenerals;
            selectableUnits = zhaoUnits;
        }
    }
    public void changeGeneral(int direction)
    {
        if (currentGeneral != null)
        {
            Destroy(currentGeneral.gameObject);
        }
        generalIdx += direction;
        generalIdx = generalIdx < 0 ? selectableGenerals.Count - 1 : generalIdx % selectableGenerals.Count;
        currentGeneral = Instantiate(selectableGenerals[generalIdx]);
    }
    public void chooseGeneral()
    {
        if (myGeneral != null)
        {
            Destroy(myGeneral.gameObject);
        }
        myGeneral = Instantiate(selectableGenerals[generalIdx]);
        myGeneral.gameObject.SetActive(false);
    }
    public void changeUnit(int direction)
    {
        if (currentUnit != null)
        {
            Destroy(currentUnit.gameObject);
        }
        unitIdx += direction;
        unitIdx = unitIdx < 0 ? selectableUnits.Count - 1 : unitIdx % selectableUnits.Count;
        currentUnit = Instantiate(selectableUnits[unitIdx]);
    }
    public void addUnit()
    {
        if (recruitmentPoints >= currentUnit.getRecruitmentCost()
            && unitsPerPlayer > myUnits.Count)
        {
            recruitmentPoints -= currentUnit.getRecruitmentCost();
            Unit unit = Instantiate(selectableUnits[unitIdx]);
            unit.gameObject.SetActive(false);
            myUnits.Add(unit);
            GameObject tile = Instantiate(unitSelectionTile);
            StaticData.findDeepChild(tile.transform, "UnitName").GetComponent<TextMeshProUGUI>()
                .text = currentUnit.getUnitName();
            tile.transform.SetParent(StaticData.findDeepChild(menu.transform, "ContentSelectedUnits"));
            StaticData.findDeepChild(menu.transform, "SelectedUnits").GetComponent<TextMeshProUGUI>()
                .text = $"ARMÉE ({myUnits.Count}/{unitsPerPlayer})";
        }
    }
    public void ready()
    {
//        readyPlayers.Value[StaticData.playerId] = true;
        StaticData.findDeepChild(menu.transform, "ChooseGen").GetComponent<Button>().interactable = false;
        StaticData.findDeepChild(menu.transform, "ChooseUnit").GetComponent<Button>().interactable = false;
        StaticData.findDeepChild(menu.transform, "Ready").GetComponent<Button>().interactable = false;
    }
}
