using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Assets.Code;

public class SpawnerUI : MonoBehaviour
{
    public Button spawnButton, toggleGravityButton, clearButton, resetButton, skinPhysicsButton, skinLerpButton;
    public Text counterText;
    public GameObject character;
    Spawner _spawner;
    
    public void SpawnMore ()
    {
        _spawner.count = 100;
        _spawner.Spawn();
    }

    // Start is called before the first frame update
    void Start()
    {
        _spawner = GameObject.Find("Spawner").GetComponent<Spawner>();

        spawnButton.onClick.AddListener(SpawnMore);
        toggleGravityButton.onClick.AddListener(ToggleGravityTowards);
        clearButton.onClick.AddListener(Clear);
        resetButton.onClick.AddListener(ResetPositions);
        skinPhysicsButton.onClick.AddListener(SpawnSkinPhysics);
        skinLerpButton.onClick.AddListener(SpawnSkinLerp);

        _spawner.SetLightDirection();

        character.SetActive(false);
    }

    void EnableButtons(bool spawnButtonEnabled = true, bool toggleGravityButtonEnabled = true, bool resetbuttonEnabled = true, bool clearButtonEnabled = true, bool spawnInSkinButtonEnabled = true, bool skinLerpButtonEnabled = true)
    {
        spawnButton.enabled = spawnButtonEnabled;
        toggleGravityButton.enabled = toggleGravityButtonEnabled;
        resetButton.enabled = resetbuttonEnabled;
        clearButton.enabled = clearButtonEnabled;
        skinPhysicsButton.enabled = spawnInSkinButtonEnabled;
        skinLerpButton.enabled = skinLerpButtonEnabled;
    }

    void SpawnSkinLerp()
    {
        EnableButtons(spawnButtonEnabled: false, toggleGravityButtonEnabled: false, resetbuttonEnabled: false);
        character.SetActive(true);
        _spawner.SpawnInSkin(true);
    }

    void SpawnSkinPhysics()
    {
        EnableButtons(spawnButtonEnabled: false, resetbuttonEnabled: false);
        character.SetActive(true);

        _spawner.SpawnInSkin(false);
    }

    void Clear()
    {
        ECSHelper.EnableSystem<LerpPositionSystem>(false);
        ECSHelper.EnableSystem<GravitateToTargetSystem>(false);
        character.SetActive(false);

        EnableButtons();

        _spawner.Clear();
    }

    void ResetPositions()
    {
        _spawner.ResetPositions();
    }

    void ToggleGravityTowards()
    {
        var system = World.Active.GetExistingSystem<GravitateToTargetSystem>();

        if (system == null)
        {
            Debug.Log("Creating new GravitateToTargetSystem");
            var world = World.Active;
            system = world.GetOrCreateSystem<GravitateToTargetSystem>();
            system.Enabled = true;

            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(world);

            //Had some weird trouble trying to create a system with [DisableAutoCreation], 
            //systems need to be added to the right SimulationSystemGroup so that they are called at the right time of the player loop.
            //So in the end went with the pattern of creating all systems, and disabling them upon creation
            //AddSystem<GravitateToTargetSystem>(system);

            //SimulationSystemGroup.AddSystemToUpdateList(system);
            //SimulationSystemGroup.SortSystemUpdateList();

            //ScriptBehaviourUpdateOrder.UpdatePlayerLoop(world);
        }
        else
        {
            system.Enabled = !system.Enabled;
            Debug.Log("Flipping Enabled on GravitateToTargetSystem to:" + system.Enabled);
        }
    }

    // Update is called once per frame
    void Update()
    {
        counterText.text = _spawner.counter.ToString();
    }


}
