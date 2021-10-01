using UnityEngine;

namespace OdinQOL
{
    internal class Utilities
    {
        public static bool IgnoreKeyPresses(bool extra = false)
        {
            if (!extra)
                return ZNetScene.instance == null || Player.m_localPlayer == null || Minimap.IsOpen() ||
                       Console.IsVisible() || TextInput.IsVisible() || ZNet.instance.InPasswordDialog() ||
                       Chat.instance?.HasFocus() == true;
            return ZNetScene.instance == null || Player.m_localPlayer == null || Minimap.IsOpen() ||
                   Console.IsVisible() || TextInput.IsVisible() || ZNet.instance.InPasswordDialog() ||
                   Chat.instance?.HasFocus() == true || StoreGui.IsVisible() || InventoryGui.IsVisible() ||
                   Menu.IsVisible() || TextViewer.instance?.IsVisible() == true;
        }

        public static bool CheckKeyDown(string value)
        {
            try
            {
                return Input.GetKeyDown(value.ToLower());
            }
            catch
            {
                return false;
            }
        }

        public static bool CheckKeyHeld(string value, bool req = true)
        {
            try
            {
                return Input.GetKey(value);
            }
            catch
            {
                return !req;
            }
        }
        
        public static bool CheckKeyDownKeycode(KeyCode value)
        {
            try
            {
                return Input.GetKeyDown(value);
            }
            catch
            {
                return false;
            }
        }

        public static bool CheckKeyHeldKeycode(KeyCode value, bool req = true)
        {
            try
            {
                return Input.GetKey(value);
            }
            catch
            {
                return !req;
            }
        }
    }
}