namespace OdinQOL.MapSharing
{
    public class VmpMapPinSync
    {
        /// <summary>
        ///     Sync Pin with clients via the server
        /// </summary>
        public static void RPC_OdinQOLMapPinSync(long sender, ZPackage mapPinPkg)
        {
            if (ZNet.m_isServer) //Server
            {
                if (sender == ZRoutedRpc.instance.GetServerPeerID()) return;

                if (mapPinPkg == null) return;

                foreach (ZNetPeer peer in ZRoutedRpc.instance.m_peers)
                    if (peer.m_uid != sender)
                        ZRoutedRpc.instance.InvokeRoutedRPC(peer.m_uid, "OdinQOLMapPinSync", mapPinPkg);
                OdinQOLplugin.Dbgl("Sending pin to all clints");
                ZLog.Log("Sent map pin to all clients");
                //OdinQOLAck.SendAck(sender);
            }
            else //Client
            {
                if (sender != ZRoutedRpc.instance.GetServerPeerID()) return; //Only bother if it's from the server.

                if (mapPinPkg == null)
                {
                    ZLog.LogWarning("Warning: Got empty map pin package from server.");
                    return;
                }

                var pinSender = mapPinPkg.ReadLong();
                string senderName = mapPinPkg.ReadString();
                if (senderName != Player.m_localPlayer.GetPlayerName() && pinSender != ZRoutedRpc.instance.m_id)
                {
                    ZLog.Log("Checking sent pin");
                    var pinPos = mapPinPkg.ReadVector3();
                    var pinType = mapPinPkg.ReadInt();
                    string pinName = mapPinPkg.ReadString();
                    var keepQuiet = mapPinPkg.ReadBool();
                    if (!Minimap.instance.HaveSimilarPin(pinPos, (Minimap.PinType)pinType, pinName, true))
                    {
                        Minimap.PinData addedPin =
                            Minimap.instance.AddPin(pinPos, (Minimap.PinType)pinType, pinName, true, false);
                        if (!keepQuiet)
                            MessageHud.instance.ShowMessage(MessageHud.MessageType.Center,
                                $"Received map pin {pinName} from {senderName}!",
                                0, Minimap.instance.GetSprite((Minimap.PinType)pinType));
                        ZLog.Log($"I got pin named {pinName} from {senderName}!");
                    }
                }
                //Send Ack
                //OdinQOLAck.SendAck(sender);
            }
        }

        /// <summary>
        ///     Send the pin, attach client ID
        /// </summary>
        public static void SendMapPinToServer(Minimap.PinData pinData, bool keepQuiet = false)
        {
            ZLog.Log("-------------------- SENDING MAP PIN DATA");
            ZPackage pkg = new();

            pkg.Write(ZRoutedRpc.instance.m_id); // Sender ID
            if (keepQuiet)
                pkg.Write(""); // Loaded in
            else
                pkg.Write(Player.m_localPlayer.GetPlayerName()); // Sender Name
            pkg.Write(pinData.m_pos); // Pin position
            pkg.Write((int)pinData.m_type); // Pin type
            pkg.Write(pinData.m_name); // Pin name
            pkg.Write(keepQuiet); // Don't shout

            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "OdinQOLMapPinSync", pkg);

            ZLog.Log($"Sent map pin {pinData.m_name} to the server");
        }
    }
}