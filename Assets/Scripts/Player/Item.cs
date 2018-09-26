using UnityEngine;
using System.Collections;

public enum ItemType {MANA, HEALTH, BlackStone, BlueStone, WEAPON};
public enum Quality {COMMON,UNCOMMON,RARE,EPIC,LEGENDARY,ARTIFACT}

public class Item : MonoBehaviour 
{
    /// <summary>
    /// The current item type
    /// </summary>
    public ItemType type;

    /// <summary>
    /// The items quality
    /// </summary>
    public Quality quality;

    /// <summary>
    /// The item's neutral sprite
    /// </summary>
    public Sprite spriteNeutral;

    /// <summary>
    /// The item's highlighted sprite
    /// </summary>
    public Sprite spriteHighlighted;

    /// <summary>
    /// The max amount of times the item can stack
    /// </summary>
    public int maxSize;

    /// <summary>
    /// These variable contains the stats of the item
    /// </summary>
    public float strength, intellect, agility, stamina;

    /// <summary>
    /// The item's name
    /// </summary>
    public string itemName;

    /// <summary>
    /// The item's description
    /// </summary>
    public string description;

    /// <summary>
    /// Uses the item
    /// </summary>
    public void Use()
    {
        switch (type) //Checks which kind of item this is
        {
            case ItemType.MANA:
                //GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().CurrHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().CurrHealth + 50;
                break;
            case ItemType.HEALTH:
                GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().CurrHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().CurrHealth + 50;
                GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().UpdateHealthBar();
                break;

            case ItemType.BlackStone:
                GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().Deffence++;
                GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().UpdateStatsPanel();
                break;

            case ItemType.BlueStone:
                GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().AttackDmg++;
                GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().UpdateStatsPanel();
                break;
        }

    }

    public string GetTooltip()
    {
        string stats = string.Empty;  //Resets the stats info
        string color = string.Empty;  //Resets the color info
        string newLine = string.Empty; //Resets the new line

        if (description != string.Empty) //Creates a newline if the item has a description, this is done to makes sure that the headline and the describion isn't on the same line
        {
            newLine = "\n";
        }

        switch (quality) //Sets the color accodring to the quality of the item
        {
            case Quality.COMMON:
                color = "white";
                break;
            case Quality.UNCOMMON:
                color = "lime";
                break;
            case Quality.RARE:
                color = "navy";
                break;
            case Quality.EPIC:
                color = "magenta";
                break;
            case Quality.LEGENDARY:
                color = "orange";
                break;
            case Quality.ARTIFACT:
                color = "red";
                break;
        }

        //Adds the stats to the string if the value is larger than 0. If the value is 0 we dont need to show it on the tooltip
        if (strength > 0)
        {
            stats += "\n+" + strength.ToString() + " Strength";
        }
        if (intellect > 0)
        {
            stats += "\n+" + intellect.ToString() + " Intellect";
        }
        if (agility > 0)
        {
            stats += "\n+" + agility.ToString() + " Agility";
        }
        if (stamina > 0)
        {
            stats += "\n+" + stamina.ToString() + " Stamina";
        }

        //Returns the formattet string
        return string.Format("<color=" + color + "><size=16>{0}</size></color><size=14><i><color=lime>" + newLine + "{1}</color></i>{2}</size>", itemName, description, stats);
    }

}
