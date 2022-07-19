using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;

namespace OdinQOL.Patches
{
    internal class QuickAccessBar : MonoBehaviour
    {
        public static ConfigEntry<bool> AddEquipmentRow = null!;
        public static ConfigEntry<bool> DisplayEquipmentRowSeparate = null!;
        public static ConfigEntry<int> ExtraRows = null!;
        public static ConfigEntry<string> HelmetText = null!;
        public static ConfigEntry<string> ChestText = null!;
        public static ConfigEntry<string> LegsText = null!;
        public static ConfigEntry<string> BackText = null!;
        public static ConfigEntry<string> UtilityText = null!;
        public static ConfigEntry<float> QuickAccessScale = null!;

        public static ConfigEntry<KeyCode> HotKey1 = null!;
        public static ConfigEntry<KeyCode> HotKey2 = null!;
        public static ConfigEntry<KeyCode> HotKey3 = null!;
        public static ConfigEntry<KeyCode> ModKeyOne = null!;
        public static ConfigEntry<KeyCode> ModKeyTwo = null!;

        public static ConfigEntry<KeyCode>[] Hotkeys = null!;

        public static ConfigEntry<float> QuickAccessX = null!;
        public static ConfigEntry<float> QuickAccessY = null!;

        public GameObject m_elementPrefab;

        public float m_elementSpace = 70f;

        private readonly List<ElementData> m_elements = new();

        private readonly List<ItemDrop.ItemData> m_items = new();

        private int m_selected;

        private void Update()
        {
            Player localPlayer = Player.m_localPlayer;
            if (localPlayer && !InventoryGui.IsVisible() && !Menu.IsVisible() && !GameCamera.InFreeFly())
            {
                if (ZInput.GetButtonDown("JoyDPadLeft"))
                    m_selected = Mathf.Max(0, m_selected - 1);
                if (ZInput.GetButtonDown("JoyDPadRight"))
                    m_selected = Mathf.Min(m_elements.Count - 1, m_selected + 1);
                if (ZInput.GetButtonDown("JoyDPadUp"))
                    localPlayer.UseHotbarItem(m_selected + 1);
            }

            if (m_selected > m_elements.Count - 1)
                m_selected = Mathf.Max(0, m_elements.Count - 1);
            UpdateIcons(localPlayer);
        }

        private void UpdateIcons(Player player)
        {
            if (!player || player.IsDead())
            {
                foreach (ElementData element in m_elements)
                    Destroy(element.m_go);
                m_elements.Clear();
            }
            else
            {
                try
                {
                    m_items.Clear();
                    Inventory inventory = player.GetInventory();
                    if (inventory.GetItemAt(5, inventory.GetHeight() - 1) != null)
                        m_items.Add(inventory.GetItemAt(5, inventory.GetHeight() - 1));
                    if (inventory.GetItemAt(6, inventory.GetHeight() - 1) != null)
                        m_items.Add(inventory.GetItemAt(6, inventory.GetHeight() - 1));
                    if (inventory.GetItemAt(7, inventory.GetHeight() - 1) != null)
                        m_items.Add(inventory.GetItemAt(7, inventory.GetHeight() - 1));
                    m_items.Sort((Comparison<ItemDrop.ItemData>)((x, y) => x.m_gridPos.x.CompareTo(y.m_gridPos.x)));
                    int num = m_items.Select(itemData => itemData.m_gridPos.x - 4).Prepend(0).Max();
                    if (m_elements.Count != num)
                    {
                        foreach (ElementData element in m_elements)
                            Destroy(element.m_go);
                        m_elements.Clear();
                        for (int index = 0; index < num; ++index)
                        {
                            ElementData elementData = new()
                            {
                                m_go = Instantiate(m_elementPrefab, transform)
                            };
                            elementData.m_go.transform.localPosition = new Vector3(index * m_elementSpace, 0.0f, 0.0f);
                            elementData.m_go.transform.Find("binding").GetComponent<Text>().text =
                                Hotkeys[index].Value.ToString();
                            elementData.m_icon =
                                elementData.m_go.transform.transform.Find("icon").GetComponent<Image>();
                            elementData.m_durability =
                                elementData.m_go.transform.Find("durability").GetComponent<GuiBar>();
                            elementData.m_amount = elementData.m_go.transform.Find("amount").GetComponent<Text>();
                            elementData.m_equiped = elementData.m_go.transform.Find("equiped").gameObject;
                            elementData.m_queued = elementData.m_go.transform.Find("queued").gameObject;
                            elementData.m_selection = elementData.m_go.transform.Find("selected").gameObject;
                            m_elements.Add(elementData);
                        }
                    }

                    foreach (ElementData element in m_elements)
                        element.m_used = false;
                    bool flag = ZInput.IsGamepadActive();
                    foreach (ItemDrop.ItemData itemData in m_items)
                    {
                        ElementData element = m_elements[itemData.m_gridPos.x - 5];
                        element.m_used = true;
                        element.m_icon.gameObject.SetActive(true);
                        element.m_icon.sprite = itemData.GetIcon();
                        element.m_durability.gameObject.SetActive(itemData.m_shared.m_useDurability);
                        if (itemData.m_shared.m_useDurability)
                        {
                            if (itemData.m_durability <= 0.0)
                            {
                                element.m_durability.SetValue(1f);
                                element.m_durability.SetColor(Mathf.Sin(Time.time * 10f) > 0.0
                                    ? Color.red
                                    : new Color(0.0f, 0.0f, 0.0f, 0.0f));
                            }
                            else
                            {
                                element.m_durability.SetValue(itemData.GetDurabilityPercentage());
                                element.m_durability.ResetColor();
                            }
                        }

                        element.m_equiped.SetActive(itemData.m_equiped);
                        element.m_queued.SetActive(player.IsItemQueued(itemData));
                        if (itemData.m_shared.m_maxStackSize > 1)
                        {
                            element.m_amount.gameObject.SetActive(true);
                            element.m_amount.text = itemData.m_stack + "/" + itemData.m_shared.m_maxStackSize;
                        }
                        else
                        {
                            element.m_amount.gameObject.SetActive(false);
                        }
                    }

                    for (int index = 0; index < m_elements.Count; ++index)
                    {
                        ElementData element = m_elements[index];
                        element.m_selection.SetActive(flag && index == m_selected);
                        if (element.m_used) continue;
                        element.m_icon.gameObject.SetActive(false);
                        element.m_durability.gameObject.SetActive(false);
                        element.m_equiped.SetActive(false);
                        element.m_queued.SetActive(false);
                        element.m_amount.gameObject.SetActive(false);
                    }
                }
                catch (Exception ex)
                {
                    OdinQOLplugin.QOLLogger.LogDebug($"There was an error in your shit: {ex}");
                }
            }
        }

        private class ElementData
        {
            public Text m_amount;

            public GuiBar m_durability;

            public GameObject m_equiped;

            public GameObject m_go;

            public Image m_icon;

            public GameObject m_queued;

            public GameObject m_selection;
            public bool m_used;
        }
    }
}