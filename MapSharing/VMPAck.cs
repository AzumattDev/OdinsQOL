namespace VMP_Mod.MapSharing
{
    public class VMPAck
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