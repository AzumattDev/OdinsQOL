namespace OdinQOL.MapSharing
{
    public class OdinQOLAck
    {
        public static void RPC_OdinQOLAck(long sender)
        {
            RpcQueue.GotAck();
        }

        public static void SendAck(long target)
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(target, "OdinQOLAck");
        }
    }
}