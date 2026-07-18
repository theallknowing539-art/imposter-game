using UnityEngine;
using UnityEngine.UI;
[CreateAssetMenu(fileName = "Player Card",menuName = "ScriptableObjects/Cards")]
public class Card : ScriptableObject
{
    public string Name;
    public Sprite CardSprite;
}
