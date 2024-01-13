using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class DataPersistenceManager : MonoBehaviour {

    public float betaLoaded;
    public float rmaxLoaded;
    public float forceFactorLoaded;
    public float mapWidthLoaded;
    public float mapHeightLoaded;
    public float[] interactionMatrixLoaded;
    public static bool isLoadingGame = false;

    [Header("File Storage Config")]
    [SerializeField] private string fileName;
    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler dataHandler;
    public static DataPersistenceManager instance { get; private set; }

    private void Awake() {
        if (instance != null) {
            Debug.LogError("Found more than one DPM in the scene.");
        }
        instance = this;

        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        dataPersistenceObjects = FindAllDataPersistenceObjects();
        if (isLoadingGame) {
            LoadGame();
            isLoadingGame = false;
        }
        else {
            NewGame();
        }
    }

    public void OnLoadClicked() {
        isLoadingGame = true;
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }
    public void NewGame()
    {
        gameData = new GameData();

        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        dataPersistenceObjects = FindAllDataPersistenceObjects();

        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects) {
            dataPersistenceObj.LoadData(gameData);
        }
    }
    public void LoadGame()
    {
        gameData = dataHandler.Load();

        if (gameData == null) {
            Debug.LogError("No load file found; A new game must be created before data can be loaded.");
            return;
        }

        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects) {
            dataPersistenceObj.LoadData(gameData);
        }
    }
    public void SaveGame()
    {
        if (this.gameData == null)
        {
            Debug.LogWarning("No saved data was found. A new game must be created before data can be saved.");
            return;
        }
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects) {
            dataPersistenceObj.SaveData(gameData);
        }
        dataHandler.Save(gameData);
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects() {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>(true)
            .OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjects);
    }
}
