using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AbilityManager : MonoBehaviour
{
    [Header("Prefab Zone")]
    [Tooltip("The prefabs of the abilities")]
    [SerializeField] private GameObject[] AbilityPrefabs; // The prefabs of the abilities

    [Header("Hotbar Zone")]
    [SerializeField] private GameObject _abilityHotbar;

    private Dictionary<string, string> abilityTranslator = new()
    {
        { "Ability_Dash", "dash" },
    };

    public static AbilityManager Instance { get; private set; }

    private void Start()
    {
        Instance = this;
    }

    public void PutAbilityToSlot(string abilityName, int slotIndex)
    {
        Debug.Log("Putting ability " + abilityName + " to slot " + slotIndex);

        if(_abilityHotbar.transform.childCount <= slotIndex)
        {
            Debug.LogError("The slot index " + slotIndex + " does not exist");
            return;
        }

        if(_abilityHotbar.transform.GetChild(slotIndex).childCount > 0)
        {
            Debug.LogError("The slot " + slotIndex + " already contains an ability");
            return;
        }

        // Check if the ability exists
        foreach (GameObject prefab in AbilityPrefabs)
        {   
            // If the ability exists, instantiate it
            if(abilityTranslator[prefab.name] == abilityName)
            {
                GameObject instance = Instantiate(prefab, _abilityHotbar.transform.GetChild(slotIndex), false);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localScale = Vector3.zero;
                instance.transform.DOScale(Vector3.one, 0.5f );
                instance.name = abilityTranslator[prefab.name];
                return;
            }
        }

        Debug.LogError("The ability " + abilityName + " does not exist");
    }

    public void ListAbilities()
    {
        foreach (Transform child in _abilityHotbar.transform)
        {
            if(child.childCount == 0)
            {
                Debug.Log("Empty slot for child " + child.name);
            }
            else
            if(child.childCount > 0)
            {
                Debug.Log(abilityTranslator[child.GetChild(0).name]);
            }
        }
    }

    public string ReadAbility(int slot)
    {
        if(_abilityHotbar.transform.childCount <= slot)
        {
            Debug.LogError("The slot index " + slot + " does not exist");
            return null;
        }

        if(_abilityHotbar.transform.GetChild(slot).childCount == 0)
        {
            return null;
        }

        return _abilityHotbar.transform.GetChild(slot).GetChild(0).name;
    }

    public bool HasAbility(string abilityName)
    {
        foreach (Transform child in _abilityHotbar.transform)
        {
            if(child.childCount == 0)
            {
                continue;
            }
            else
            if(child.childCount > 0)
            {
                if(child.GetChild(0).name == abilityName)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void RemoveAbilityFromSlot(int slotIndex)
    {
        if(_abilityHotbar.transform.childCount <= slotIndex)
        {
            Debug.LogError("The slot index " + slotIndex + " does not exist");
            return;
        }

        if(_abilityHotbar.transform.GetChild(slotIndex).childCount > 0)
        {
            Destroy(_abilityHotbar.transform.GetChild(slotIndex).GetChild(0).gameObject);
        }
    }
}
 