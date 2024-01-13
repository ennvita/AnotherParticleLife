using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChangeOptions : MonoBehaviour, IDataPersistence
{

    [Header ("Sliders")]
    public Slider rmaxSlider;
    public TMP_InputField rmaxInput;
    [HideInInspector] public float _rmax;
    private float archiveRmax;


    public Slider betaSlider;
    public TMP_InputField betaInput;
    [HideInInspector] public float _beta;
    private float archiveBeta;


    public Slider forceFactorSlider;
    public TMP_InputField forceFactorInput;
    [HideInInspector] public float _forceFactor;
    private float archiveForceFactor;


    public Slider particleCountSlider;
    public TMP_InputField particleCountInput;
    private int archiveParticleCount;
    [HideInInspector] public int _particleCount;


    public Slider particleRadiusSlider;
    public TMP_InputField particleRadiusInput;
    [HideInInspector] public float _particleRadius;
    private float archiveParticleRadius;


    public Slider particleVerticesSlider;
    public TMP_InputField particleVertextCountInput;
    [HideInInspector] public int _particleVertices;
    private int archiveParticleVertices;

    [Header ("Map Sliders")]
    public Slider mapWidthSlider;
    public TMP_InputField mapWidthInput;
    [HideInInspector] public int _mapWidth;
    private int archiveMapWidth;


    public Slider mapHeightSlider;
    public TMP_InputField mapHeightInput;
    [HideInInspector] public int _mapHeight;
    private int archiveMapHeight;



    [Header ("Raw Input Fields")]
    public TMP_InputField collisionStrength;
    [HideInInspector] public float _collisionStrength;
    public TMP_InputField numWhiteParticles;
    [HideInInspector] public int _numWhiteParticles;

    void Start() {
        SetSliderDefaults();
    }

    public void SetSliderDefaults() {
        rmaxSlider.minValue = 1.0f;
        rmaxSlider.maxValue = 40f;
        rmaxSlider.SetDirection(Slider.Direction.LeftToRight, true);

        betaSlider.minValue = 0;
        betaSlider.maxValue = 1.0f;
        betaSlider.SetDirection(Slider.Direction.LeftToRight, true);

        forceFactorSlider.minValue = 0;
        forceFactorSlider.maxValue = .1f;
        forceFactorSlider.SetDirection(Slider.Direction.LeftToRight, true);

        particleCountSlider.minValue = 0;
        particleCountSlider.maxValue = 300000;
        particleCountSlider.SetDirection(Slider.Direction.LeftToRight, true);
        particleCountSlider.wholeNumbers = true;

        particleRadiusSlider.minValue = 0.125f;
        particleRadiusSlider.maxValue = 10.0f;
        particleRadiusSlider.SetDirection(Slider.Direction.LeftToRight, true);

        particleVerticesSlider.minValue = 3;
        particleVerticesSlider.maxValue = 30;
        particleVerticesSlider.SetDirection(Slider.Direction.LeftToRight, true);
        particleVerticesSlider.wholeNumbers = true;

        mapWidthSlider.minValue = 500;
        mapWidthSlider.maxValue = 2000;
        mapWidthSlider.SetDirection(Slider.Direction.LeftToRight, true);
        mapWidthSlider.wholeNumbers = true;

        mapHeightSlider.minValue = 500;
        mapHeightSlider.maxValue = 2000;
        mapHeightSlider.SetDirection(Slider.Direction.LeftToRight, true);
        mapHeightSlider.wholeNumbers = true;

        rmaxSlider.value = archiveRmax;
        rmaxInput.text = _rmax.ToString();

        betaSlider.value = archiveBeta; // reset to savedata value because above line triggers UpdateEngineSettings
        betaInput.text = archiveBeta.ToString();

        forceFactorSlider.value = archiveForceFactor;
        forceFactorInput.text = archiveForceFactor.ToString();

        particleCountSlider.value = archiveParticleCount;
        particleCountInput.text = archiveParticleCount.ToString();

        particleRadiusSlider.value = archiveParticleRadius;
        particleRadiusInput.text = archiveParticleRadius.ToString();

        particleVerticesSlider.value = archiveParticleVertices;
        particleVertextCountInput.text = archiveParticleVertices.ToString();

        mapWidthSlider.value = archiveMapWidth;
        mapWidthInput.text = archiveMapWidth.ToString();

        mapHeightSlider.value = archiveMapHeight;
        mapHeightInput.text = archiveMapHeight.ToString();
    }

    public void UpdateSettings() {
        UpdateRMAX();
        UpdateBETA();
        UpdateForceFactor();
        UpdateParticleCount();
        UpdateParticleRadius();
        UpdateParticleVertices();
        UpdateMapWidth();
        UpdateMapHeight();
    }

    private void UpdateRMAX() {
        _rmax = rmaxSlider.value; // get rmax value from slider
        float roundRMAX = (float)Math.Round(_rmax, 4); // round so it's easier to read
        rmaxInput.text = roundRMAX.ToString(); // update text shown
    }

    private void UpdateBETA() {
        _beta = betaSlider.value;
        float roundBETA = (float)Math.Round(_beta, 4);
        betaInput.text = roundBETA.ToString();
    }

    public void UpdateForceFactor() {
        _forceFactor = forceFactorSlider.value;
        float roundForceFactor = (float)Math.Round(_forceFactor, 4);
        forceFactorInput.text = roundForceFactor.ToString();
    }

    private void UpdateParticleCount() {
        _particleCount = (int)particleCountSlider.value;
        particleCountInput.text = _particleCount.ToString();
    }

    private void UpdateParticleRadius() {
        _particleRadius = (float)particleRadiusSlider.value;
        float roundParticleRadius = (float)Math.Round(_particleRadius, 2);
        particleRadiusInput.text = roundParticleRadius.ToString();
    }

    private void UpdateParticleVertices() {
        _particleVertices = (int)particleVerticesSlider.value;
        particleVertextCountInput.text = _particleVertices.ToString();
    }

    private void UpdateMapWidth() {
        _mapWidth = (int)mapWidthSlider.value;
        mapWidthInput.text = _mapWidth.ToString();
    }

    private void UpdateMapHeight() {
        _mapHeight = (int)mapHeightSlider.value;
        mapHeightInput.text = _mapHeight.ToString();
    }

    public void SyncSliderValues() {
        float sRmax = float.Parse(rmaxInput.text); // parse the input string to float 
        rmaxSlider.value = sRmax; // set slider to new value
        
        float sBeta = float.Parse(betaInput.text);
        betaSlider.value =  sBeta;

        float sForceFactor = float.Parse(forceFactorInput.text);
        forceFactorSlider.value = sForceFactor;

        int sParticleCount = int.Parse(particleCountInput.text);
        particleCountSlider.value = sParticleCount;

        float sParticleRadius = float.Parse(particleRadiusInput.text);
        particleRadiusSlider.value = sParticleRadius;

        int sParticleVertices = int.Parse(particleVertextCountInput.text);
        particleVerticesSlider.value = sParticleVertices;

        int sMapWidth = int.Parse(mapWidthInput.text);
        mapWidthSlider.value = sMapWidth;

        int sMapHeight = int.Parse(mapHeightInput.text);
        mapHeightSlider.value = sMapHeight;
    }

    public void LoadData(GameData data) {
        _rmax = data.rmax; // phsycis variables
        _beta = data.beta;
        _forceFactor = data.forceFactor;

        _particleCount = data.particleCount; // particle structure variables
        _particleRadius = data.particleRadius;
        _particleVertices = data.particleVertices;

        archiveRmax = data.rmax;
        archiveBeta = data.beta; //store all of these twice to recall variable after it's reset to zero by updateRMAX
        archiveForceFactor = data.forceFactor;

        archiveParticleCount = data.particleCount;
        archiveParticleRadius = data.particleRadius;
        archiveParticleVertices = data.particleVertices;

        _mapWidth = data.mapWidth; // begin map variables
        _mapHeight = data.mapHeight;

        archiveMapWidth = data.mapWidth;
        archiveMapHeight = data.mapHeight;
    }

    public void SaveData(GameData data) {
        data.rmax = _rmax;
        data.beta = _beta;
        data.forceFactor = _forceFactor;

        data.particleCount = _particleCount;
        data.particleRadius = _particleRadius;
        data.particleVertices = _particleVertices;

        data.mapHeight = _mapHeight;
        data.mapWidth = _mapWidth;
    }
}