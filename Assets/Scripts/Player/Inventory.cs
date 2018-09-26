using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class Inventory : MonoBehaviour
{

    #region Variables
    /// <summary>
    /// The number of rows
    /// </summary>
    public int rows;

    /// <summary>
    /// The number of slots
    /// </summary>
    public int slots;

    /// <summary>
    /// The number of empty slots in the inventory
    /// </summary>
    private static int emptySlots;

    /// <summary>
    /// Offset used to move the hovering object away from the mouse 
    /// </summary>
    private float hoverYOffset;

    /// <summary>
    /// The width and height of the inventory
    /// </summary>
    private float inventoryWidth, inventoryHight;

    /// <summary>
    /// The left and top slots padding
    /// </summary>
    public float slotPaddingLeft, slotPaddingTop;

    /// <summary>
    /// The size of each slot
    /// </summary>
    public float slotSize;

    /// <summary>
    /// The slots prefab
    /// </summary>
    public GameObject slotPrefab;

    /// <summary>
    /// A prefab used for instantiating the hoverObject
    /// </summary>
    public GameObject iconPrefab;

    /// <summary>
    /// A reference to the object that hovers next to the mouse
    /// </summary>
    private static GameObject hoverObject;

    /// <summary>
    /// The slots that we are moving an item from
    /// </summary>
    private static Slot from;

    /// <summary>
    /// The slots that we are moving and item to
    /// </summary>
    private static Slot to;

    /// <summary>
    /// A reference to the inventorys RectTransform
    /// </summary>
    private RectTransform inventoryRect;

    /// <summary>
    /// A reference to the inventorys canvas
    /// </summary>
    public Canvas canvas;

    /// <summary>
    /// A reference to the EventSystem 
    /// </summary>
    public EventSystem eventSystem;

    /// <summary>
    /// The inventory's canvas group, this is used for hiding the inventory
    /// </summary>
    private static CanvasGroup canvasGroup;

    /// <summary>
    /// The inventory's singleton instance
    /// </summary>
    private static Inventory instance;

    /// <summary>
    /// Indicates if the inventory is in the process of fading in
    /// </summary>
    private bool fadingIn;

    /// <summary>
    /// Indicates if the inventory is in the process of fading out
    /// </summary>
    private bool fadingOut;

    /// <summary>
    /// The time it takes for the inventory to fade in seconds
    /// </summary>
    public float fadeTime;

    /// <summary>
    /// The clicked object
    /// </summary>
    private static GameObject clicked;

    /// <summary>
    /// The UI element that we are using when we need to split a stack
    /// </summary>
    public GameObject selectStackSize;

    /// <summary>
    /// The amount of items to pickup (this is the text on the UI element we use for splitting)
    /// </summary>
    public Text stackText;

    /// <summary>
    /// The amount of items we have in our "hand"
    /// </summary>
    private int splitAmount;

    /// <summary>
    /// The maximum amount of items we are allowed to remove from the stack
    /// </summary>
    private int maxStackCount;

    /// <summary>
    /// This is sed to store our items when moving them from one slot to another
    /// </summary>
    private static Slot movingSlot;

    /// <summary>
    /// A prototy of our mana item
    /// This is used when loading a saved inventory
    /// </summary>
    public GameObject mana;

    /// <summary>
    /// A prototype of our healt potion
    /// This is used when loading a saved inventory
    /// </summary>
    public GameObject health;

    /// <summary>
    /// A prototype of our weapon
    /// This is used when loading a saved inventory
    /// </summary>
    public GameObject weapon;

    #endregion

    #region Collections
    /// <summary>
    /// A list of all the slots in the inventory
    /// </summary>
    private List<GameObject> allSlots;
    #endregion

    #region Properties

    /// <summary>
    /// Proprty for accessing our singleton
    /// </summary>
    public static Inventory Instance
    {
        get
        {
            if (instance == null) //Creates a reference to our inventory, if it's null
            {
                instance = GameObject.FindObjectOfType<Inventory>();
            }
            return Inventory.instance;
        }
    }

    /// <summary>
    /// A property for accssing the canvasgroup
    /// </summary>
    public static CanvasGroup CanvasGroup
    {
        get { return Inventory.canvasGroup; }
    }

    /// <summary>
    /// Property for accessing the amount of empty slots
    /// </summary>
    public static int EmptySlots
    {
        get { return emptySlots; }
        set { emptySlots = value; }
    }

    #endregion

    private static GameObject selectStackSizeStatic;
    public GameObject tooltipObject;
    private static GameObject tooltip;

    public Text sizeTextObject;
    private static Text sizeText;

    public Text visualTextObject;
    private static Text visualText;

    // Use this for initialization
    void Start()
    {
        tooltip = tooltipObject;
        sizeText = sizeTextObject;
        visualText = visualTextObject;

        selectStackSizeStatic = selectStackSize;

        canvasGroup = transform.parent.GetComponent<CanvasGroup>();
 
        //Creates the inventory layout
        CreateLayout();

        movingSlot = GameObject.Find("MovingSlot").GetComponent<Slot>();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0)) //Checks if the user lifted the first mousebutton
        {
            //Removes the selected item from the inventory
            if (!eventSystem.IsPointerOverGameObject(-1) && from != null) //If we click outside the inventory and the have picked up an item
            {
                from.GetComponent<Image>().color = Color.white; //Rests the slots color 
                from.ClearSlot(); //Removes the item from the slot
                Destroy(GameObject.Find("Hover")); //Removes the hover icon

                //Resets the objects
                to = null;
                from = null;
                emptySlots++;
            }
            else if (!eventSystem.IsPointerOverGameObject(-1) && !movingSlot.IsEmpty)
            {
                movingSlot.ClearSlot();
                Destroy(GameObject.Find("Hover"));
            }
        }
        if (hoverObject != null) //Checks if the hoverobject exists
        {
            //The hoverObject's position
            Vector2 position;

            //Translates the mouse screen position into a local position and stores it in the position
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, canvas.worldCamera, out position);

            //Adds the offset to the position
            position.Set(position.x, position.y - hoverYOffset);

            //Sets the hoverObject's position
            hoverObject.transform.position = canvas.transform.TransformPoint(position);
        }
        if (Input.GetKeyDown(KeyCode.B))//Checks if we press the B button
        {
            if (canvasGroup.alpha > 0) //If our inventory is visible, then we know that it is open
            {
                StartCoroutine("FadeOut"); //Close the inventory
                PutItemBack(); //Put all items we have in our hand back in the inventory

            }
            else//If it isn't open then it's closed and we neeed to fade in
            {
                StartCoroutine("FadeIn");
            }
        }
        if (Input.GetMouseButton(2)) //If we press the middle mouse button
        {
            if (eventSystem.IsPointerOverGameObject(-1)) //Makes suare that we cliced inside the inventory
            {
                MoveInventory();//Moves the inventory around
            }
        }
    }

    /// <summary>
    /// Shows the tooltip
    /// </summary>
    /// <param name="slot">The slot we just hovered</param>
    public void ShowToolTip(GameObject slot)
    {   
        //Saves a reference to the slot we just moused over
        Slot tmpSlot = slot.GetComponent<Slot>();

        //If the slot contains an item and we arent splitting or moving any items then we can show the tooltip
        if (!tmpSlot.IsEmpty && hoverObject == null && !selectStackSizeStatic.activeSelf)
        {   
            //Gets the information from the item on the slot we just moved our mouse over
            visualText.text = tmpSlot.CurrentItem.GetTooltip();

            //Makes sure that the tooltip has the correct size.
            sizeText.text = visualText.text;

            //Shows the tool tip
            tooltip.SetActive(true);

            //Calculates the position while taking the padding into account
            float xPos = slot.transform.position.x + slotPaddingLeft;
            float yPos = slot.transform.position.y - slot.GetComponent<RectTransform>().sizeDelta.y - slotPaddingTop;

            //Sets the position
            tooltip.transform.position = new Vector2(xPos, yPos);
        }

       
    }

    /// <summary>
    /// Hide the tooltip
    /// </summary>
    public void HideToolTip()
    {
        tooltip.SetActive(false);
    }

    /// <summary>
    /// Saves the inventory and its content
    /// </summary>
    public void SaveInventory()
    {
        string content = string.Empty; //Creates a string for containing infor about the items inside the inventory

        for (int i = 0; i < allSlots.Count; i++) //Runs through all slots in the inventory
        {
            Slot tmp = allSlots[i].GetComponent<Slot>(); //Careates a reference to the slot at the current index

            if (!tmp.IsEmpty) //We only want to save the info if the slot contains an item
            {
                //Creates a string with this format: SlotIndex-ItemType-AmountOfItems; this string can be read so that we can rebuild the inventory
                content += i + "-" + tmp.CurrentItem.type.ToString() + "-" + tmp.Items.Count.ToString() + ";";
            }
        }

        //Stores all the info in the PlayerPrefs
        PlayerPrefs.SetString("content", content);
        PlayerPrefs.SetInt("slots", slots);
        PlayerPrefs.SetInt("rows", rows);
        PlayerPrefs.SetFloat("slotPaddingLeft", slotPaddingLeft);
        PlayerPrefs.SetFloat("slotPaddingTop", slotPaddingTop);
        PlayerPrefs.SetFloat("slotSize", slotSize);
        PlayerPrefs.SetFloat("xPos", inventoryRect.position.x);
        PlayerPrefs.SetFloat("yPos", inventoryRect.position.y);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads the inventory
    /// </summary>
    public void LoadInventory()
    {
        //Loads all the inventory's data from the playerprefs
        string content = PlayerPrefs.GetString("content");
        slots = PlayerPrefs.GetInt("slots");
        rows = PlayerPrefs.GetInt("rows");
        slotPaddingLeft = PlayerPrefs.GetFloat("slotPaddingLeft");
        slotPaddingTop = PlayerPrefs.GetFloat("slotPaddingTop");
        slotSize = PlayerPrefs.GetFloat("slotSize");

        //Sets the inventorys position
        inventoryRect.position = new Vector3(PlayerPrefs.GetFloat("xPos"), PlayerPrefs.GetFloat("yPos"), inventoryRect.position.z);

        //Recreates the inventory's layout
        CreateLayout();

        //Splits the loaded content string into segments, so that each index inthe splitContent array contains information about a single slot
        //e.g[0]0-MANA-3
        string[] splitContent = content.Split(';');

        //Runs through every single slot we have infor about -1 is to avoid an empty string error
        for (int x = 0; x < splitContent.Length - 1; x++)
        {
            //Splits the slot's information into single values, so that each index in the splitValues array contains info about a value
            //E.g[0]InventorIndex [1]ITEMTYPE [2]Amount of items
            string[] splitValues = splitContent[x].Split('-');

            int index = Int32.Parse(splitValues[0]); //InventorIndex 

            ItemType type = (ItemType)Enum.Parse(typeof(ItemType), splitValues[1]); //ITEMTYPE

            int amount = Int32.Parse(splitValues[2]); //Amount of items

            for (int i = 0; i < amount; i++) //Adds the correct amount of items to the inventory
            {
                switch (type)
                {
                    case ItemType.MANA: //Adds a manapotion
                        allSlots[index].GetComponent<Slot>().AddItem(mana.GetComponent<Item>());
                        break;
                    case ItemType.HEALTH://Adds a healthpotion
                        allSlots[index].GetComponent<Slot>().AddItem(health.GetComponent<Item>());
                        break;
                    case ItemType.WEAPON://Adds a weapon
                        allSlots[index].GetComponent<Slot>().AddItem(weapon.GetComponent<Item>());
                        break;
                }
            }


        }


    }

    /// <summary>
    /// Creates the inventory's layout
    /// </summary>
    private void CreateLayout()
    {
        if (allSlots != null)
        {
            foreach (GameObject go in allSlots)
            {
                Destroy(go);
            }
        }

        //Instantiates the allSlot's list
        allSlots = new List<GameObject>();

        //Calculates the hoverYOffset by taking 1% of the slot size
        hoverYOffset = slotSize * 0.01f;

        //Stores the number of empty slots
        emptySlots = slots;

        //Calculates the width of the inventory
        inventoryWidth = (slots / rows) * (slotSize + slotPaddingLeft);

        //Calculates the highs of the inventory
        inventoryHight = rows * (slotSize + slotPaddingTop);

        //Creates a reference to the inventory's RectTransform
        inventoryRect = GetComponent<RectTransform>();

        //Sets the with and height of the inventory.
        inventoryRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, inventoryWidth+slotPaddingLeft);
        inventoryRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, inventoryHight+slotPaddingTop);

        //Calculates the amount of columns
        int columns = slots / rows;

        for (int y = 0; y < rows; y++) //Runs through the rows
        {
            for (int x = 0; x < columns; x++) //Runs through the columns
            {   
                //Instantiates the slot and creates a reference to it
                GameObject newSlot = (GameObject)Instantiate(slotPrefab);

                //Makes a reference to the rect transform
                RectTransform slotRect = newSlot.GetComponent<RectTransform>();

                //Sets the slots name
                newSlot.name = "Slot";

                //Sets the canvas as the parent of the slots, so that it will be visible on the screen
                newSlot.transform.SetParent(this.transform.parent);

                //Sets the slots position
                slotRect.localPosition = inventoryRect.localPosition + new Vector3((slotPaddingLeft * (x + 1) + (slotSize * x)) , (-slotPaddingTop * (y + 1) - (slotSize * y)));

                //Sets the size of the slot
                slotRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, slotSize* canvas.scaleFactor);
                slotRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, slotSize * canvas.scaleFactor);
                newSlot.transform.SetParent(this.transform);

                //Adds the new slots to the slot list
                allSlots.Add(newSlot);

            }
        }
    }

    /// <summary>
    /// Adds an item to the inventory
    /// </summary>
    /// <param name="item">The item to add</param>
    /// <returns></returns>
    public bool AddItem(Item item)
    {
        if (item.maxSize == 1) //If the item isn't stackable
        {   
            //Places the item at an empty slot
            PlaceEmpty(item);
            return true;
        }
        else //If the item is stackable 
        {
            foreach (GameObject slot in allSlots) //Runs through all slots in the inventory
            {
                Slot tmp = slot.GetComponent<Slot>(); //Creates a reference to the slot

                if (!tmp.IsEmpty) //If the item isn't empty
                {
                    //Checks if the om the slot is the same type as the item we want to pick up
                    if (tmp.CurrentItem.type == item.type && tmp.IsAvailable) 
                    {
                        if (!movingSlot.IsEmpty && clicked.GetComponent<Slot>() == tmp.GetComponent<Slot>())
                        {
                            continue;
                        }
                        else
                        {
                            tmp.AddItem(item); //Adds the item to the inventory
                            return true;
                        }
                    }
                }
            }
            if (emptySlots > 0) //Places the item on an empty slots
            {
                PlaceEmpty(item);
            }
        }

        return false;
    }
    /// <summary>
    /// Moves the whole inventory
    /// </summary>
    private void MoveInventory()
    {
        Vector2 mousePos; //The inventory's new position

        //Translates the middle of the inventory into the mouse position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, new Vector3(Input.mousePosition.x - (inventoryRect.sizeDelta.x / 2 * canvas.scaleFactor), Input.mousePosition.y + (inventoryRect.sizeDelta.y / 2 * canvas.scaleFactor)), canvas.worldCamera, out mousePos);

        //Sets the inventorys position
        transform.position = canvas.transform.TransformPoint(mousePos);
    }

    /// <summary>
    /// Places an item on an empty slot
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private bool PlaceEmpty(Item item)
    {
        if (emptySlots > 0) //If we have atleast 1 empty slot
        {
            foreach (GameObject slot in allSlots) //Runs through all slots
            {
                Slot tmp = slot.GetComponent<Slot>(); //Creates a reference to the slot 

                if (tmp.IsEmpty) //If the slot is empty
                {
                    tmp.AddItem(item); //Adds the item
                    emptySlots--; //Reduces the number of empty slots
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Moves an item to another slot in the inventory
    /// </summary>
    /// <param name="clicked"></param>
    public void MoveItem(GameObject clicked)
    {
        //Careates a reference to the object that we just clicked
        Inventory.clicked = clicked;

        if (!movingSlot.IsEmpty)//Checks if we are splitting an item
        {
            Slot tmp = clicked.GetComponent<Slot>(); //Get's a reference to the slot we just clicked

            if (tmp.IsEmpty)//If the clicked slot is empty, then we can simply put all items down
            {
                tmp.AddItems(movingSlot.Items); //Puts all the items down in the slot that we clicked
                movingSlot.Items.Clear(); //Clears the moving slot
                Destroy(GameObject.Find("Hover")); //Removes the hover object
            }
            else if (!tmp.IsEmpty && movingSlot.CurrentItem.type == tmp.CurrentItem.type && tmp.IsAvailable) //If the slot we clicked isn't empty, then we need to merge the stacks
            {
                //Merges two stacks of the same type
                MergeStacks(movingSlot, tmp);
            }
        }
        else if (from == null && canvasGroup.alpha == 1 && !Input.GetKey(KeyCode.LeftShift)) //If we haven't picked up an item
        {
            if (!clicked.GetComponent<Slot>().IsEmpty && !GameObject.Find("Hover")) //If the slot we clicked sin't empty
            {
                from = clicked.GetComponent<Slot>(); //The slot we ar emoving from

                from.GetComponent<Image>().color = Color.gray; //Sets the from slots color to gray, to visually indicate that its the slot we are moving from

                CreateHoverIcon();

            }
        }
        else if (to == null && !Input.GetKey(KeyCode.LeftShift)) //Selects the slot we are moving to
        {
            to = clicked.GetComponent<Slot>(); //Sets the to object
            Destroy(GameObject.Find("Hover")); //Destroys the hover object
        }
        if (to != null && from != null) //If both to and from are null then we are done moving. 
        {
            if (!to.IsEmpty && from.CurrentItem.type == to.CurrentItem.type && to.IsAvailable)
            {
                MergeStacks(from, to);
            }
            else
            {
                Stack<Item> tmpTo = new Stack<Item>(to.Items); //Stores the items from the to slot, so that we can do a swap

                to.AddItems(from.Items); //Stores the items in the "from" slot in the "to" slot

                if (tmpTo.Count == 0) //If "to" slot if 0 then we dont need to move anything to the "from " slot.
                {
                    from.ClearSlot(); //clears the from slot
                }
                else
                {
                    from.AddItems(tmpTo); //If the "to" slot contains items thne we need to move the to the "from" slot
                }
            }

            //Resets all values
            from.GetComponent<Image>().color = Color.white;
            to = null;
            from = null;
            Destroy(GameObject.Find("Hover"));
        }
    }

    /// <summary>
    /// Creates a hover icon next to the mouse
    /// </summary>
    private void CreateHoverIcon()
    {
        hoverObject = (GameObject)Instantiate(iconPrefab); //Instantiates the hover object 

        hoverObject.GetComponent<Image>().sprite = clicked.GetComponent<Image>().sprite; //Sets the sprite on the hover object so that it reflects the object we are moing

        hoverObject.name = "Hover"; //Sets the name of the hover object

        //Creates references to the transforms
        RectTransform hoverTransform = hoverObject.GetComponent<RectTransform>();
        RectTransform clickedTransform = clicked.GetComponent<RectTransform>();

        ///Sets the size of the hoverobject so that it has the same size as the clicked object
        hoverTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, clickedTransform.sizeDelta.x);
        hoverTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, clickedTransform.sizeDelta.y);

        //Sets the hoverobject's parent as the canvas, so that it is visible in the game
        hoverObject.transform.SetParent(GameObject.Find("Canvas").transform, true);

        //Sets the local scale to make usre that it has the correct size
        hoverObject.transform.localScale = clicked.gameObject.transform.localScale;

        hoverObject.transform.GetChild(0).GetComponent<Text>().text = movingSlot.Items.Count > 1 ? movingSlot.Items.Count.ToString() : string.Empty;
    }

    /// <summary>
    /// Puts the items back in the inventory
    /// </summary>
    private void PutItemBack()
    {
        if (from != null)//If we are carrying a whole stack of items
        {
            //put the items back and remove the hover icon
            Destroy(GameObject.Find("Hover"));
            from.GetComponent<Image>().color = Color.white;
            from = null;
        }
        else if (!movingSlot.IsEmpty) //If we are carrying  split stack
        {
            //Removes the hover icon
            Destroy(GameObject.Find("Hover"));

            //Puts the items back one by one
            foreach (Item item in movingSlot.Items)
            {
                clicked.GetComponent<Slot>().AddItem(item);
            }

            movingSlot.ClearSlot(); //Makes sure that the moving slot is empty
        }

        //Hides the UI for splitting a stack
        selectStackSize.SetActive(false);
    }

    /// <summary>
    /// Sets the stacks info, so that we know how many items we can remove
    /// </summary>
    /// <param name="maxStackCount"></param>
    public void SetStackInfo(int maxStackCount)
    {
        //Shows the UI for splitting a stack
        selectStackSize.SetActive(true);

        //Hides the tooltip so that it doesn't overlap the splitstack ui
        tooltip.SetActive(false);

        //Resets the amount of split items
        splitAmount = 0;

        //Stores the maxcount
        this.maxStackCount = maxStackCount;

        //Writes writes the selected amount of itesm in the UI
        stackText.text = splitAmount.ToString();
    }

    /// <summary>
    /// Splits a stack of items
    /// </summary>
    public void SplitStack()
    {
        //Hids the UI for splitting a stack
        selectStackSize.SetActive(false);

        if (splitAmount == maxStackCount) //If we picked up all the items then we dont need to handle it as as split stack
        {
            MoveItem(clicked);
        }
        else if (splitAmount > 0) //If the split amount is larger than 0 then we need to pick up x amount of items
        {
            movingSlot.Items = clicked.GetComponent<Slot>().RemoveItems(splitAmount); //Picks up the items 

            CreateHoverIcon(); //Careates the hover icon
        }
    }

    /// <summary>
    /// Updates the text on the split UI elemt so that it reflects the users selection
    /// </summary>
    /// <param name="i"></param>
    public void ChangeStackText(int i)
    {
        splitAmount += i;

        if (splitAmount < 0) //Makes sure we dont go below 
        {
            splitAmount = 0;
        }
        if (splitAmount > maxStackCount) //Makes sure that we dont go above max
        {
            splitAmount = maxStackCount;
        }

        //Writes the text on the UI element
        stackText.text = splitAmount.ToString();
    }

    /// <summary>
    /// Merges the items on two slots
    /// </summary>
    /// <param name="source">The slot to merge the items from</param>
    /// <param name="destination">The slot to merge the items into</param>
    public void MergeStacks(Slot source, Slot destination)
    {
        //Calculates the max amount of items we are allowed to merge onto the stack
        int max = destination.CurrentItem.maxSize - destination.Items.Count;

        //Sets the correct amount so that we don't put too many items down
        int count = source.Items.Count < max ? source.Items.Count : max;

        for (int i = 0; i < count; i++) //Merges the items into the other stack
        {
            destination.AddItem(source.RemoveItem()); //Removes the items from the source and adds them to the destination
            hoverObject.transform.GetChild(0).GetComponent<Text>().text = movingSlot.Items.Count.ToString(); //Updates the text on the stack that
        }
        if (source.Items.Count == 0) //We ont have more items to merge with
        {
            source.ClearSlot();
            Destroy(GameObject.Find("Hover"));
        }
    }

    /// <summary>
    /// Makes the inventory fade out
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeOut()
    {
        if (!fadingOut) //Checks if we are already fading out
        {
            //Sets the current state
            fadingOut = true;
            fadingIn = false;

            //Makes sure that we are not fading out the at same time
            StopCoroutine("FadeIn");

            //Sets the values for fading
            float startAlpha = canvasGroup.alpha;

            float rate = 1.0f / fadeTime; //Calculates the rate, so that we can fade over x amount of seconds

            float progress = 0.0f; //Progresses over the set time


            while (progress < 1.0) //Progresses over the set time
            {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, progress);  //Lerps from the start alpha to 0 to make the inventory invisible

                progress += rate * Time.deltaTime; //Adds to the progress so that we will get close to out goal

                yield return null;
            }

            //Sets the end condition to make sure we are 100% invisible
            canvasGroup.alpha = 0;

            //Sets the status
            fadingOut = false;
        }
    }

    /// <summary>
    /// Makes the inventory fade in
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeIn()
    {
        if (!fadingIn) //Checks if we are already fading out
        {
            //Sets the current state
            fadingOut = false;
            fadingIn = true;

            //Makes sure that we are not fading out the at same time
            StopCoroutine("FadeOut");

            float startAlpha = canvasGroup.alpha; //Sets the start alpha value

            float rate = 1.0f / fadeTime; //Calculates the rate, so that we can fade over x amount of seconds

            float progress = 0.0f; //Resets the progress

            while (progress < 1.0) //Progresses over the set time
            {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 1, progress); //Lerps from the start alpha to 1 to make the inventory visible

                progress += rate * Time.deltaTime; //Adds to the progress so that we will get close to out goal

                yield return null;
            }

            //Sets the end condition to make sure we are 100% visible
            canvasGroup.alpha = 1;

            //Sets the status
            fadingIn = false;
        }
    }
}
