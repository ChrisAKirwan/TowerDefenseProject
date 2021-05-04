using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [HideInInspector] public List<Menu> openMenuList;
    public Menu contextMenu;
    public Menu placeCastleNotif;
    public Menu HUD;
    public Menu pauseMenu;
    public Slider detailsMenuHealthBar;
    public Slider castleHealthBar;

    public Menu buildMenu { get; private set; }
    public Menu detailsMenu { get; private set; }
    public Menu nextWaveButton { get; private set; }

    private BaseStructureClass currentStructure;
    private MainLevel mainLevelRef;
    private SpawnManager spawnManagerRef;


    // Start is called before the first frame update
    void Start()
    {
        AssignMenuChildren();
        mainLevelRef = this.GetComponent<MainLevel>();
        spawnManagerRef = this.GetComponent<SpawnManager>();


        HUD.transform.GetChild(1).GetChild(2).GetComponent<Text>().text = Mathf.FloorToInt(mainLevelRef.currency).ToString();
    }

    void Update()
    {
        if (detailsMenu.isOpen)
        {
            if (currentStructure != null)
                FillDetailsMenu(currentStructure);
            else
                detailsMenu.transform.GetChild(4).GetComponent<Button>().interactable = false;
        }
        else if(buildMenu.isOpen)
        {
            FillBuildMenu();
        }

        if(mainLevelRef.playerCastleRef != null)
            castleHealthBar.value = mainLevelRef.playerCastleRef.currentHealth / mainLevelRef.playerCastleRef.maxHealth;
    }

    public void OpenMenu(Menu menu)
    {
        if (menu.isOpen)
            return;

        menu.isOpen = true;
        openMenuList.Add(menu);
    }

    public void CloseMenuRecursive(Menu menu) 
    {
        if (!menu.isOpen)
            return;

        menu.isOpen = false;
        openMenuList.Remove(menu);

        if (menu.hasChildren)
            for (int i = 0; i < menu.children.Length; i++)
                CloseMenuRecursive(menu.children[i]);
    }

    public void CloseMenu(Menu menu)
    {
        if (!menu.isOpen)
            return;

        menu.isOpen = false;
        openMenuList.Remove(menu);
    }

    private void AssignMenuChildren()
    {
        if (contextMenu.children.Length > 1)
        {
            detailsMenu = contextMenu.children[0];
            buildMenu = contextMenu.children[1];
        }
        else
        {
            Debug.LogError("Missing ContextMenu UI Menu Children. (Should contain BuildMenu and DetailsMenu)");
            Application.Quit();
        }

        if (HUD.children.Length > 0)
        {
            nextWaveButton = HUD.children[0];
        }
        else
        {
            Debug.LogError("Missing HUD UI Menu Children. (Should contain NextWaveButton)");
            Application.Quit();
        }
    }

    public void FillDetailsMenu(BaseStructureClass structure)
    {
        currentStructure = structure;

        detailsMenu.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = structure.buildingName;
        detailsMenuHealthBar.value = structure.currentHealth / structure.maxHealth;

        string structureDetails = "Health: " + structure.currentHealth.ToString() + "/" + structure.maxHealth.ToString() + "\n";
        structureDetails += "Armor: " + structure.armor.ToString() + "\n";
        structureDetails += "Upgrades: " + structure.currentUpgrades.ToString() + "/" + structure.maxNumUpgrades.ToString() + "\n";
        detailsMenu.transform.GetChild(3).GetComponent<UnityEngine.UI.Text>().text = structureDetails;

        Button detailsButton = detailsMenu.transform.GetChild(4).GetComponent<Button>();
        detailsButton.interactable = CanStructureUpgrade();
        // if player has enough currency:
        if (detailsButton.interactable)
        {
            // Set button text to GREEN UPGRADE
            detailsButton.transform.GetChild(0).GetComponent<Text>().color = Color.green;
            detailsButton.transform.GetChild(0).GetComponent<Text>().text = "Upgrade!";
        }
        else
        {
            if (structure.currentUpgrades == structure.maxNumUpgrades)
            {
                detailsButton.transform.GetChild(0).GetComponent<Text>().color = Color.gray;
                detailsButton.transform.GetChild(0).GetComponent<Text>().text = "MAX!";
            }
            else
            {
                // Set button to RED CURRENTAMOUNT/COST
                detailsButton.transform.GetChild(0).GetComponent<Text>().color = Color.red;

                int currency = Mathf.FloorToInt(mainLevelRef.currency);
                int cost = Mathf.FloorToInt(currentStructure.upgradePrice);

                detailsButton.transform.GetChild(0).GetComponent<Text>().text = currency.ToString() + "/" + cost.ToString();
            }
        }

        detailsMenu.transform.GetChild(4).GetComponent<Button>().interactable = CanStructureUpgrade();
    }

    public void FillBuildMenu()
    {
        int sr_turretCost = Mathf.FloorToInt(spawnManagerRef.srTurret.structurePrice);
        int lr_turretCost = Mathf.FloorToInt(spawnManagerRef.lrTurret.structurePrice);
        int farmCost = Mathf.FloorToInt(spawnManagerRef.farmRef.structurePrice);
        int wallCost = Mathf.FloorToInt(spawnManagerRef.wallTowerRef.structurePrice);

        Button sr_turretButton = buildMenu.transform.GetChild(1).GetComponent<Button>();
        Button lr_turretButton = buildMenu.transform.GetChild(2).GetComponent<Button>();
        Button farmButton = buildMenu.transform.GetChild(3).GetComponent<Button>();
        Button wallButton = buildMenu.transform.GetChild(4).GetComponent<Button>();

        SetBuildMenuButtonText(sr_turretButton, sr_turretCost);
        SetBuildMenuButtonText(lr_turretButton, lr_turretCost);
        SetBuildMenuButtonText(farmButton, farmCost);
        SetBuildMenuButtonText(wallButton, wallCost);
    }

    private void SetBuildMenuButtonText(Button structureButton, int price)
    {
        if (mainLevelRef.currency >= price)
        {
            structureButton.transform.GetChild(1).GetComponent<Text>().color = Color.green;
            structureButton.transform.GetChild(1).GetComponent<Text>().text = "Purchase!";
            structureButton.interactable = true;
        }
        else
        {
            int currency = Mathf.FloorToInt(mainLevelRef.currency);
            structureButton.transform.GetChild(1).GetComponent<Text>().color = Color.red;
            structureButton.transform.GetChild(1).GetComponent<Text>().text = currency.ToString() + "/" + price.ToString();
            structureButton.interactable = false;
        }
    }

    public void UpdateRoundTimer()
    {
        HUD.transform.GetChild(2).transform.GetChild(5).GetComponent<UnityEngine.UI.Text>().text = this.GetComponent<MainLevel>().roundTimer.ToString();
    }

    public void UpdateWaveNumber()
    {
        HUD.transform.GetChild(2).transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = this.GetComponent<MainLevel>().currentWave.ToString();
    }

    public void UpdateRoundNumber()
    {
        HUD.transform.GetChild(2).transform.GetChild(3).GetComponent<UnityEngine.UI.Text>().text = this.GetComponent<MainLevel>().currentRound.ToString();
    }

    public void ToggleVisibilityOfUI()
    {
        for(int i = 0; i < openMenuList.Count; i++)
        {
            openMenuList[i].isOpen = !openMenuList[i].isOpen;
        }
    }

    public void UpgradeStructure()
    {
        if(currentStructure != null)
        {
            mainLevelRef.RemoveCurrency(currentStructure.upgradePrice);
            currentStructure.Upgrade();

            detailsMenu.transform.GetChild(4).GetComponent<Button>().interactable = CanStructureUpgrade();
        }
    }

    private bool CanStructureUpgrade()
    {
        if (currentStructure == null)
            return false;

        if (currentStructure.currentUpgrades == currentStructure.maxNumUpgrades)
            return false;

        if (mainLevelRef.currency < currentStructure.structurePrice * (currentStructure.currentUpgrades * 0.75f + 1.0f))
            return false;

        return true;
    }

    public void UpdateHUD()
    {
        Text currencyText = HUD.transform.GetChild(1).GetChild(2).GetComponent<Text>();
        currencyText.text = Mathf.FloorToInt(mainLevelRef.currency).ToString();
    }
}
