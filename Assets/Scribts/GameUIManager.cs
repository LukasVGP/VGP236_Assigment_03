using UnityEngine;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    [SerializeField] private TextMeshProUGUI playerMessageText;
    [SerializeField] private TextMeshProUGUI teamText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetPlayerMessage(string message)
    {
        playerMessageText.text = message;
    }

    public void ClearPlayerMessage()
    {
        playerMessageText.text = "";
    }

    public void SetTeamText(string teamName)
    {
        teamText.text = teamName;
    }
}
