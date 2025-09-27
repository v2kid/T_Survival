using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Global;
public class UIResourceDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;
    private void Start()
    {
        Global.Utilities.WaitAfter(0.1f, () =>
        {
            PlayerStats.Instance.Coin.Subscribe(OnCoinChanged, true);
        });

    }

    private void OnCoinChanged(int oldvalue, int newCoinAmount)
    {
        coinText.AnimateInt(newCoinAmount);
    }



}