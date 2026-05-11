using UnityEngine;

// Definiert die Seltenheitsstufen
public enum Rarity { Common, Uncommon, Rare, Epic, Legendary, Heavenly }
public enum Subclass { Vampire, Human, Zombie, Beast, Settlement, Nature, Holy, Artifact, Fire, Ice, Dragon }
public enum Cardtype { Creature, Spell, Aura, Weapon }

[CreateAssetMenu(fileName = "New Card", menuName = "Cardgame/Card")]
public class CardData : ScriptableObject
{
    public int cardId;
    public int cardCost;
    public string cardName;
    public Cardtype cardType;
    public Rarity rarity;
    public Sprite artwork;
    public Sprite fullArtwork;
    public int attack;
    public int health;
    public Subclass subclass;
    [TextArea(5, 10)]
    public string description;
    [TextArea(5, 10)] // Erzeugt ein größeres Textfeld im Inspector
    public string loreText;
}
