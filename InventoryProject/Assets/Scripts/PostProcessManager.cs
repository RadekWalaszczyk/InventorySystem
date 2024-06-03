using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostProcessManager : MonoBehaviour
{
    /*
    I know I should create a custom rendering feature so that I can change these material parameters via script as post process, 
    but for project with this scope it will be massive overkill
    */
    [SerializeField] Material DrunkMaterial;
    float currDrunkness;

    public static PostProcessManager Instance;
    void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void Start()
    {
        DrunkMaterial.SetFloat("_DrunkValue", 0f);
    }

    public void SetDrunkness(float newDrunkLevel, float drunkTime)
    {
        StopAllCoroutines();
        StartCoroutine(IncreaseDrunkness(Mathf.Clamp01(currDrunkness + newDrunkLevel), drunkTime));
    }

    IEnumerator IncreaseDrunkness(float newDrunkLevel, float drunkTime)
    {
        float delay = 1f / ((newDrunkLevel - currDrunkness) / 0.005f);

        while (currDrunkness < newDrunkLevel)
        {
            currDrunkness += 0.005f;
            DrunkMaterial.SetFloat("_DrunkValue", currDrunkness);
            yield return new WaitForSeconds(delay);
        }

        StartCoroutine(DicreaseDrunkness(drunkTime));
    }

    IEnumerator DicreaseDrunkness(float drunkTime)
    {
        DrunkMaterial.SetFloat("_DrunkValue", currDrunkness);

        yield return new WaitForSeconds(drunkTime);

        float delay = (1f * drunkTime) / (currDrunkness / 0.005f);

        while (currDrunkness > 0)
        {
            currDrunkness -= 0.005f;
            DrunkMaterial.SetFloat("_DrunkValue", currDrunkness);
            yield return new WaitForSeconds(delay);
        }

        DrunkMaterial.SetFloat("_DrunkValue", 0f);
    }

    private void OnDisable()
    {
        DrunkMaterial.SetFloat("_DrunkValue", 0f);
    }
}
