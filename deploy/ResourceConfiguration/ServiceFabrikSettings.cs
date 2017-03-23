namespace ResourceConfiguration
{
    public class ServiceFabrikSettings
    {
        public string ClusterName { get; set; }
        public string ClusterLocation { get; set; }
        public string ComputeLocation { get; set; }
        public string AdminUsername { get; set; }
        public string AdminPassword { get; set; }
        public string NicName { get; set; }
        public string PublicIPAddressName { get; set; }
        public string VmStorageAccountName { get; set; }
        public string DnsName { get; set; }
        public string VirtualNetworkName { get; set; }
        public string LbName { get; set; }
        public string LbIPName { get; set; }
        public string ApplicationDiagnosticsStorageAccountName { get; set; }
        public string SupportLogStorageAccountName { get; set; }
    }
}