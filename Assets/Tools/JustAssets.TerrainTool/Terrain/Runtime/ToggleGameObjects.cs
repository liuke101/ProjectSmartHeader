using UnityEngine;

namespace JustAssets.TerrainUtility
{
    public class ToggleGameObjects : MonoBehaviour
    {
        public GameObject[] ListA;

        public GameObject[] ListB;

        public bool ListAIsActive = false;

        public void Toggle()
        {
            ListAIsActive = !ListAIsActive;

            foreach (var item in ListA)
            {
                item.SetActive(ListAIsActive);
            }

            foreach (var item in ListB)
            {
                item.SetActive(!ListAIsActive);
            }
        }
    }
}