﻿using VMP_Mod.Utility;

namespace VMP_Mod.RPC
{
    public class VPlusAck
    {
        public static void RPC_VPlusAck(long sender)
        {
            RpcQueue.GotAck();
        }

        public static void SendAck(long target)
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(target, "VPlusAck");
        }
    }
}