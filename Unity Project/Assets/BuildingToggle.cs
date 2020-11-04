﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingToggle : Toggle
{
    public BuildingObject building;
    public Image buildingImage;
    public Text buildingName;
    public Text buildingDescription;

    public void InitializeMiniature(){
        buildingImage.sprite = building.sprite;
        buildingName.text = building.buildingName;
        buildingDescription.text = building.description;
    }
}