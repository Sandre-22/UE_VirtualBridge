namespace Loupedeck.UE_VirtualBrige
{
    using Loupedeck;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    // This action sets an actor's location

    public class SetActorLocation : PluginDynamicCommand
    {
        private static readonly HttpClient client = new HttpClient();
        private string baseUrl = "http://192.168.10.215:30010";  // Replace with IP address of host computer

        public SetActorLocation() : base(displayName: "Set Location", description: "Set location to new XYZ coordinate.", groupName: "Unreal") { }

        protected override void RunCommand(String actionParameter)
        {

        }

    }
}