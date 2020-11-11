﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingInformation : MonoBehaviour
{
    public Pointer pointer;
    public BuildingSpot selectedSpot;
    [Header("Header")]
    public Image outline;
    public Image buildingImage;
    public Text buildingName;
    public Text populationText;
    [Header("Tabs")]
    public Toggle overviewToggle;
    public Toggle productionToggle;
    public Toggle projectToggle;
    public Toggle maintenanceToggle;
    public GameObject overviewMenu, productionMenu, projectMenu, projectDetail, maintenanceMenu;
    [Header("Overview")]
    public List<Text> statusTexts;
    public GameObject destroyButton;
    public GameObject destroyWarning;
    [Header("Maintenance")]
    public GameObject integrity;
    public Text buildDate;
    public Text costInfo;
    public Text shortageEffect;
    public Image integrityMeter;
    public Image integrityMeterOutline;
    public Text integrityMeterText;
    public Text integrityRiskText;
    public GameObject startMaintenanceButton;
    public GameObject stopMaintenanceButton;
    [Header("Production")]
    public Text efficiency;
    public RessourceBox monthlyProduction;
    public RessourceBox monthlyCost;
    public GameObject stopProduction;
    public GameObject resumeProduction;
    public Text storage;
    public Image storageBar;
    public Image storageOutline;
    public GameObject increaseStorageButton;
    public GameObject stopStorageButton;
    public RessourceBox increaseStorageCost;
    public Text increaseStorageTime;
    [Header("Project")]
    public List<ProjectChoice> projectChoices;
    public Text projectNameText;
    public Text projectDescriptionText;
    public Text projectEffectText;
    public Text projectLengthText;
    public Image projectImage;
    public RessourceBox projectCost;
    public GameObject startProjectButton, stopProjectButton;



    public void ShowBuildingInfo(BuildingSpot spot){
        if(spot != null){
            gameObject.SetActive(true);
            pointer.gameObject.SetActive(true);
            pointer.target = spot.transform;
            buildingName.color = spot.currentBuilding.color;
            buildingImage.color = spot.currentBuilding.color;
            
            productionToggle.gameObject.SetActive(spot.currentBuilding.productor);
            projectToggle.gameObject.SetActive(spot.currentBuilding.research);
            maintenanceToggle.gameObject.SetActive(true);

            if(productionToggle.isOn && spot.currentBuilding.research){
                productionToggle.SetIsOnWithoutNotify(false);
                projectToggle.SetIsOnWithoutNotify(true);
            }
            if(projectToggle.isOn && spot.currentBuilding.productor){
                productionToggle.SetIsOnWithoutNotify(true);
                projectToggle.SetIsOnWithoutNotify(false);
            }
            if((productionToggle.isOn || projectToggle.isOn)&&!spot.currentBuilding.productor&&!spot.currentBuilding.research){
                overviewToggle.SetIsOnWithoutNotify(true);
                productionToggle.SetIsOnWithoutNotify(false);
                projectToggle.SetIsOnWithoutNotify(false);
                maintenanceToggle.SetIsOnWithoutNotify(false);
            }

            if(!spot.Built){
                overviewToggle.SetIsOnWithoutNotify(true);
                productionToggle.SetIsOnWithoutNotify(false);
                projectToggle.SetIsOnWithoutNotify(false);
                maintenanceToggle.SetIsOnWithoutNotify(false);
                productionToggle.gameObject.SetActive(false);
                projectToggle.gameObject.SetActive(false);
                maintenanceToggle.gameObject.SetActive(false);
            }

            DestroyMenu(false);
            selectedSpot = spot;
            UpdateMenuInfo(spot);
        }else{
            gameObject.SetActive(false);
            pointer.gameObject.SetActive(false);
            selectedSpot = null;
        }
    }

    public void UpdateMenuInfo(BuildingSpot spot){
        
        buildingImage.sprite = spot.currentBuilding.sprite;
        buildingName.text = spot.currentBuilding.buildingName + " " + spot.district;
        Color populationColor = spot.OverPopulated ? GM.I.art.red : GM.I.art.white;
        populationText.text = "<color=#"+ColorUtility.ToHtmlStringRGBA(populationColor) + ">" + UIManager.HumanNotation(spot.population) + "</color>" + " / " + UIManager.HumanNotation(spot.currentBuilding.populationRequirement);
        
        overviewMenu.SetActive(overviewToggle.isOn);
        productionMenu.SetActive(productionToggle.isOn);
        projectMenu.SetActive(projectToggle.isOn);
        maintenanceMenu.SetActive(maintenanceToggle.isOn);

        costInfo.text = spot.currentBuilding.costInfo;
        shortageEffect.text = spot.Cost.Limited(GM.I.resource.resources)? spot.currentBuilding.shortageEffect:"";
        integrity.SetActive(spot.Built && !spot.currentBuilding.control);
        buildDate.text = "Built in "+UIManager.TimeToDate(spot.constructionDate);
        integrityMeterText.text = UIManager.HumanNotation(spot.integrity);
        integrityMeter.fillAmount = spot.integrity;
        if(spot.BadIntegrity){
            integrityMeter.color = GM.I.art.orange;
            integrityRiskText.color = GM.I.art.orange;
            integrityRiskText.text = "FAILURE RISK - MEDIUM";
        }else if(spot.DangerousIntegrity){
            integrityMeter.color = GM.I.art.red;
            integrityRiskText.color = GM.I.art.red;
            integrityRiskText.text = "FAILURE RISK - HIGH";
        }else{
            integrityMeter.color = GM.I.art.light;
            integrityRiskText.color = GM.I.art.light;
            integrityRiskText.text = "FAILURE RISK - LOW";
        }
        
        monthlyProduction.UpdateRessourceBox(spot.currentBuilding.production.GetProduction().Multiply(spot.efficiency));
        monthlyCost.UpdateRessourceBox(spot.currentBuilding.production.GetCost().Multiply(spot.costEfficiency));
        storage.text = (int)spot.ResourcePortion() + " / " + spot.storage;
        storageBar.fillAmount = spot.ResourcePortion() / spot.storage;
        storageBar.color = spot.currentBuilding.color;
        storageOutline.color = spot.currentBuilding.color;
        increaseStorageButton.SetActive(!(spot.increaseStorage||spot.storage == spot.storageMax));
        stopStorageButton.SetActive(!(!spot.increaseStorage||spot.storage == spot.storageMax));
        increaseStorageCost.gameObject.SetActive(spot.storage != spot.storageMax);
        increaseStorageTime.gameObject.SetActive(spot.storage != spot.storageMax);
        increaseStorageCost.UpdateRessourceBox(spot.currentBuilding.storageIncreaseMonthlyCost);
        increaseStorageTime.text = spot.storageCounter + " months";
        StartMaintenance(spot.maintenance);
        StopProduction(!spot.producing);

        ProcessStatus(spot, true);

        for (int i = 0; i < spot.currentBuilding.projects.Count; i++)
        {
            Project project = spot.currentBuilding.projects[i];
            projectChoices[i].Init(project);
        }
        
        if(spot.currentBuilding.control){
            destroyButton.SetActive(false);
        }
        if(!overviewToggle.isOn){
            DestroyMenu(false);
        }
    }

    public int ProcessStatus(BuildingSpot spot, bool updateText){
        List<string> statuses = new List<string>();
        List<Color> colors = new List<Color>();
        if(updateText){
            foreach (Text text in statusTexts)
            {
                text.text = "";
            }
        }

        if(!spot.Built){
            if(spot.constructionHalted){
                statuses.Add("Construction stopped");
                colors.Add(GM.I.art.red);
                statuses.Add("Not enough resources!");
                colors.Add(GM.I.art.red);
            }else{
                statuses.Add("Under construction");
                colors.Add(GM.I.art.light);
            }
        }else{
            
            if(spot.Cost.Limited(GM.I.resource.resources)){
                statuses.Add("Not enough resources!");
                colors.Add(GM.I.art.red);
            }
            if(spot.maintenance){
                    statuses.Add("Under maintenance");
                    colors.Add(GM.I.art.light);
            }
            else if(spot.DangerousIntegrity){
                statuses.Add("Needs maintenance!");
                colors.Add(GM.I.art.red);
            }
            else if(spot.BadIntegrity){
                statuses.Add("Needs maintenance");
                colors.Add(GM.I.art.orange);
            }
            if(spot.currentBuilding.housing){
                if(spot.OverPopulated){
                    statuses.Add("Overpopulated!");
                    colors.Add(GM.I.art.red);
                }else if(spot.HighPopulated){
                    statuses.Add("Crowded");
                    colors.Add(GM.I.art.orange);
                }
            }
            if(spot.currentBuilding.productor){
                if(!spot.producing){
                    statuses.Add("Production stopped");
                    colors.Add(GM.I.art.orange);
                }else if (!spot.maintenance){
                    if(spot.LowPopulated){
                        statuses.Add("Not enough workers");
                        colors.Add(GM.I.art.orange);
                    }
                    if(spot.storage == spot.ResourcePortion()){
                        statuses.Add("Storage full");
                        colors.Add(GM.I.art.light);
                    }
                }
                
            }
        }

        if(statuses.Count == 0){
            statuses.Add("Working correctly");
            colors.Add(GM.I.art.green);
        }
        int statusNumber = 0;
        for (int i = 0; i < statusTexts.Count; i++)
        {
            if(i < statuses.Count){
                if(updateText){
                    statusTexts[i].text = statuses[i];
                    statusTexts[i].color = colors[i];
                }
                if(colors[i] == GM.I.art.red){
                    statusNumber = 3;
                }
                if(colors[i] == GM.I.art.orange && statusNumber <= 1){
                    statusNumber = 2;
                }
                if(colors[i] == GM.I.art.light && statusNumber == 0){
                    statusNumber = 1;
                }
            }
        }
        return statusNumber;
    }

    public void UpdateMenuInfo(){
        if(selectedSpot != null){
            UpdateMenuInfo(selectedSpot);
        }
    }

    public void StartMaintenance(bool onOff){
        StartMaintenance(onOff, selectedSpot);
    }

    public void StartMaintenance(bool onOff, BuildingSpot spot){
        spot.maintenance = onOff;
        startMaintenanceButton.SetActive(!onOff);
        stopMaintenanceButton.SetActive(onOff);
        integrityMeterOutline.color = onOff? GM.I.art.white : GM.I.art.light;
    }

    public void StopProduction(bool onOff){
        StopProduction(onOff, selectedSpot);
    }

    public void StopProduction(bool onOff, BuildingSpot spot){
        spot.producing = !onOff;
        stopProduction.SetActive(!onOff);
        resumeProduction.SetActive(onOff);
        spot.UpdateVisual();
    }

    public void DestroyMenu(bool onOff){
        destroyWarning.SetActive(onOff);
        destroyButton.SetActive(!onOff);
    }

    public void DestroyBuilding(){
        selectedSpot.Destroy();
        ShowBuildingInfo(null);
    }

    public void IncreaseStorage(bool onOff){
        selectedSpot.increaseStorage = onOff;
        increaseStorageButton.SetActive(!onOff);
        stopStorageButton.SetActive(onOff);
    }

    public void SelectProject(Project project){
        projectMenu.SetActive(false);
        projectDetail.SetActive(true);
        projectNameText.text = project.projectName;
        projectDescriptionText.text = project.projectDescription;
        projectEffectText.text = project.effectDescription;
        projectCost.UpdateRessourceBox(project.monthlyCost);
    }
}
