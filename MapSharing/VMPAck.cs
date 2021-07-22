namespace VMP_Mod.MapSharing
{
    public class VMPAck
    {
        public static void RPC_VMPAck(long sender)
        {
            RpcQueue.GotAck();
        }

        public static void SendAck(long target)
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(target, "VMPAck");
        }
    }
}