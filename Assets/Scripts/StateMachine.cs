using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct SecretData
{
    public string secretWord;
    public string imposterHint;
}

public class StateMachine : MonoBehaviour
{
    [Header("Card Library")]
    public Card[] allAvailableCards; 
    
    private Card[] activeDeck; 
    private Role[] playerRoles; 
    private string[] playerSecretText; 
    private int playerCount;

    [Header("Secret Library")]
    public SecretData[] secretLibrary;

    [Header("Setup UI References")]
    public GameObject SetupUIContainer;      
    public TMP_InputField PlayerCountInput;  
    public TMP_Dropdown[] PlayerDropdowns;   
    public Button StartGameButton;           

    [Header("Game UI References")]
    public Image CardImage;
    public TextMeshProUGUI TopText;
    public TextMeshProUGUI RoleText;         
    public TextMeshProUGUI SecretText;       
    public Button BigCardButton;             
    public Button NextButton;                

    [Header("Talking & Reveal UI References")]
    public GameObject TalkingPanel;          // Container for the talking phase UI
    public Image TalkingPlayerImage;         // Shows the card of whoever speaks first
    
    public GameObject RevealPanel;           // Container for the reveal phase UI
    public TextMeshProUGUI RevealText;       // Announces who the Imposter is
    public Image ImposterImage;              // Shows the Imposter's card

    public enum Role { Simple, Imposter }
    public enum Phase { SettingUp, PassingPhone, ViewingCard, Talking, Reveal }

    private Phase currentPhase;
    private int currentPlayerIndex = 0;
    private int firstSpeakerIndex;

    public void Start() 
    {
        // We can call Start() to restart the game, so we ensure everything resets here
        currentPhase = Phase.SettingUp;
        
        SetupUIContainer.SetActive(true);
        TalkingPanel.SetActive(false);
        RevealPanel.SetActive(false);
        
        CardImage.gameObject.SetActive(false);
        BigCardButton.gameObject.SetActive(false);
        NextButton.gameObject.SetActive(false);
        
        TopText.text = "Game Setup";
        RoleText.text = "";
        SecretText.text = "";

        PopulateDropdowns();
        
        foreach (var dropdown in PlayerDropdowns)
        {
            dropdown.gameObject.SetActive(false);
        }
    }

    public void OnPlayerCountChanged()
    {
        if (int.TryParse(PlayerCountInput.text, out int count))
        {
            playerCount = Mathf.Clamp(count, 3, 5);
            
            for (int i = 0; i < PlayerDropdowns.Length; i++)
            {
                PlayerDropdowns[i].gameObject.SetActive(i < playerCount);
            }
        }
    }

    void PopulateDropdowns()
    {
        List<string> cardNames = new List<string>();
        foreach (Card card in allAvailableCards)
        {
            cardNames.Add(card.Name);
        }

        foreach (TMP_Dropdown dropdown in PlayerDropdowns)
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(cardNames);
        }
    }

    public void OnStartGameClicked()
    {
        if (secretLibrary == null || secretLibrary.Length == 0)
        {
            Debug.LogError("Add Secret Data to the Library in the Inspector!");
            return;
        }

        activeDeck = new Card[playerCount];
        for (int i = 0; i < playerCount; i++)
        {
            int selectedIndex = PlayerDropdowns[i].value;
            activeDeck[i] = allAvailableCards[selectedIndex];
        }

        SetupUIContainer.SetActive(false);

        AssignRoles();
        StartChoosingPhase();
    }

    void AssignRoles()
    {
        playerRoles = new Role[playerCount];
        playerSecretText = new string[playerCount];

        for(int i = 0; i < playerRoles.Length; i++)
        {
            playerRoles[i] = Role.Simple;
        }

        int imposterIndex = Random.Range(0, playerRoles.Length);
        playerRoles[imposterIndex] = Role.Imposter;

        int randomIndex = Random.Range(0, secretLibrary.Length);
        SecretData selectedSecret = secretLibrary[randomIndex];

        for (int i = 0; i < playerCount; i++)
        {
            if (playerRoles[i] == Role.Simple)
            {
                playerSecretText[i] = selectedSecret.secretWord;
            }
            else if (playerRoles[i] == Role.Imposter)
            {
                playerSecretText[i] = selectedSecret.imposterHint;
            }
        }
    }

    public void StartChoosingPhase()
    {
        currentPlayerIndex = 0;
        currentPhase = Phase.PassingPhone;
        UpdateUI();
    }

    public void OnCardTapped()
    {
        if (currentPhase == Phase.PassingPhone)
        {
            currentPhase = Phase.ViewingCard;
            UpdateUI();
        }
    }

    public void OnNextButtonClicked()
    {
        if (currentPhase == Phase.ViewingCard)
        {
            currentPlayerIndex++;

            if (currentPlayerIndex < activeDeck.Length)
            {
                currentPhase = Phase.PassingPhone;
            }
            else
            {
                currentPhase = Phase.Talking;
                firstSpeakerIndex = Random.Range(0, activeDeck.Length);
            }
            UpdateUI();
        }
    }

    // Connect this to your "Reveal Imposter" button inside the Talking Panel
    public void OnRevealButtonClicked()
    {
        if (currentPhase == Phase.Talking)
        {
            currentPhase = Phase.Reveal;
            UpdateUI();
        }
    }

    // Connect this to your "Restart Game" button inside the Reveal Panel
    public void OnRestartButtonClicked()
    {
        Start(); // Re-runs the setup logic at the top of the script
    }

    void UpdateUI()
    {
        switch (currentPhase)
        {
            case Phase.PassingPhone:
                TopText.text = $"Pass phone to {activeDeck[currentPlayerIndex].Name}.";
                RoleText.text = ""; 
                SecretText.text = "";
                CardImage.gameObject.SetActive(true);
                CardImage.sprite = null; 
                BigCardButton.gameObject.SetActive(true);
                BigCardButton.interactable = true;      
                NextButton.gameObject.SetActive(false); 
                break;

            case Phase.ViewingCard:
                TopText.text = $"{activeDeck[currentPlayerIndex].Name}'s Secret";
                RoleText.text = "Role: " + playerRoles[currentPlayerIndex].ToString();
                SecretText.text = playerSecretText[currentPlayerIndex];
                CardImage.sprite = activeDeck[currentPlayerIndex].CardSprite;
                BigCardButton.interactable = false;    
                NextButton.gameObject.SetActive(true); 
                break;

            case Phase.Talking:
                TopText.text = "Discussion Phase!";
                RoleText.text = "";
                SecretText.text = "";
                
                // Hide the main game UI
                CardImage.gameObject.SetActive(false);
                BigCardButton.gameObject.SetActive(false);
                NextButton.gameObject.SetActive(false);

                // Show the Talking Panel and the first speaker's card
                TalkingPanel.SetActive(true);
                TalkingPlayerImage.sprite = activeDeck[firstSpeakerIndex].CardSprite;
                RoleText.text = $"{activeDeck[firstSpeakerIndex].Name} starts the discussion!";
                break;

            case Phase.Reveal:
                TalkingPanel.SetActive(false);
                RevealPanel.SetActive(true);

                TopText.text = "Game Over!";
                RoleText.text = "";

                // Find who the Imposter was
                int imposterIndex = 0;
                for (int i = 0; i < playerCount; i++)
                {
                    if (playerRoles[i] == Role.Imposter)
                    {
                        imposterIndex = i;
                        break; // Stop looking once we find them
                    }
                }

                // Show the Imposter's data
                RevealText.text = $"The Imposter was {activeDeck[imposterIndex].Name}!";
                ImposterImage.sprite = activeDeck[imposterIndex].CardSprite;
                break;
        }
    }
}