using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataPersistence
{
    void LoadData(GameData data); // implementing script only needs to read.

    void SaveData(GameData data); // allow the implementing script to modify data, so use a passby ref
}
