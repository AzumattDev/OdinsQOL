using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VMP_Mod.EAQS
{
    public class QuickAccessBar : MonoBehaviour
    {
        public GameObject m_elementPrefab;

        public float m_elementSpace = 70f;

        private readonly List<ElementData> m_elements = new List<ElementData>();

        private readonly List<ItemDrop.ItemData> m_items = new List<ItemDrop.ItemData>();

        private int m_selected;

        private void Update()
        {
            var localPlayer = Player.m_localPlayer;
            if (localPlayer && !InventoryGui.IsVisible() && !Menu.IsVisible() && !GameCamera.InFreeFly())
            {
                if (ZInput.GetButtonDown("JoyDPadLeft")) m_selected = Mathf.Max(0, m_selected - 1);
                if (ZInput.GetButtonDown("JoyDPadRight")) m_selected = Mathf.Min(m_elements.Count - 1, m_selected + 1);
                if (ZInput.GetButtonDown("JoyDPadUp")) localPlayer.UseHotbarItem(m_selected + 1);
            }

            if (m_selected > m_elements.Count - 1) m_selected = Mathf.Max(0, m_elements.Count - 1);
            UpdateIcons(localPlayer);
        }

        private void UpdateIcons(Player player)
        {
            if (!player || player.IsDead())
            {
                foreach (var elementData in m_elements) Destroy(elementData.m_go);
                m_elements.Clear();
                return;
            }

            m_items.Clear();

            var inv = player.GetInventory();
            if (inv.GetItemAt(5, inv.GetHeight() - 1) != null)
                m_items.Add(inv.GetItemAt(5, inv.GetHeight() - 1));
            if (inv.GetItemAt(6, inv.GetHeight() - 1) != null)
                m_items.Add(inv.GetItemAt(6, inv.GetHeight() - 1));
            if (inv.GetItemAt(7, inv.GetHeight() - 1) != null)
                m_items.Add(inv.GetItemAt(7, inv.GetHeight() - 1));

            m_items.Sort((x, y) => x.m_gridPos.x.CompareTo(y.m_gridPos.x));
            var num = 0;
            foreach (var itemData in m_items)
                if (itemData.m_gridPos.x - 4 > num)
                    num = itemData.m_gridPos.x - 4;

            if (m_elements.Count != num)
            {
                foreach (var elementData in m_elements) Destroy(elementData.m_go);
                m_elements.Clear();
                for (var i = 0; i < num; i++)
                {
                    var elementData = new ElementData();
                    elementData.m_go = Instantiate(m_elementPrefab, transform);
                    elementData.m_go.transform.localPosition = new Vector3(i * m_elementSpace, 0f, 0f);
                    elementData.m_go.transform.Find("binding").GetComponent<Text>().text = EAQS.hotkeys[i].Value;
                    elementData.m_icon = elementData.m_go.transform.transform.Find("icon").GetComponent<Image>();
                    elementData.m_durability = elementData.m_go.transform.Find("durability").GetComponent<GuiBar>();
                    elementData.m_amount = elementData.m_go.transform.Find("amount").GetComponent<Text>();
                    elementData.m_equiped = elementData.m_go.transform.Find("equiped").gameObject;
                    elementData.m_queued = elementData.m_go.transform.Find("queued").gameObject;
                    elementData.m_selection = elementData.m_go.transform.Find("selected").gameObject;
                    m_elements.Add(elementData);
                }
            }

            foreach (var elementData in m_elements) elementData.m_used = false;
            var flag = ZInput.IsGamepadActive();
            for (var j = 0; j < m_items.Count; j++)
            {
                var itemData2 = m_items[j];
                var elementData = m_elements[itemData2.m_gridPos.x - 5];
                elementData.m_used = true;
                elementData.m_icon.gameObject.SetActive(true);
                elementData.m_icon.sprite = itemData2.GetIcon();
                elementData.m_durability.gameObject.SetActive(itemData2.m_shared.m_useDurability);
                if (itemData2.m_shared.m_useDurability)
                {
                    if (itemData2.m_durability <= 0f)
                    {
                        elementData.m_durability.SetValue(1f);
                        elementData.m_durability.SetColor(Mathf.Sin(Time.time * 10f) > 0f
                            ? Color.red
                            : new Color(0f, 0f, 0f, 0f));
                    }
                    else
                    {
                        elementData.m_durability.SetValue(itemData2.GetDurabilityPercentage());
                        elementData.m_durability.ResetColor();
                    }
                }

                elementData.m_equiped.SetActive(itemData2.m_equiped);
                elementData.m_queued.SetActive(player.IsItemQueued(itemData2));
                if (itemData2.m_shared.m_maxStackSize > 1)
                {
                    elementData.m_amount.gameObject.SetActive(true);
                    elementData.m_amount.text = itemData2.m_stack + "/" + itemData2.m_shared.m_maxStackSize;
                }
                else
                {
                    elementData.m_amount.gameObject.SetActive(false);
                }
            }

            for (var k = 0; k < m_elements.Count; k++)
            {
                var elementData = m_elements[k];
                elementData.m_selection.SetActive(flag && k == m_selected);
                if (!elementData.m_used)
                {
                    elementData.m_icon.gameObject.SetActive(false);
                    elementData.m_durability.gameObject.SetActive(false);
                    elementData.m_equiped.SetActive(false);
                    elementData.m_queued.SetActive(false);
                    elementData.m_amount.gameObject.SetActive(false);
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