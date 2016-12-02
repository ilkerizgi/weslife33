namespace AramBuddy.AutoShop
{
    internal class BuildServices
    {
        public BuildServices(string patch, params string[] services)
        {
            this.Patch = patch;
            this.AvailableServices = services;
        }

        public string Patch;
        public string[] AvailableServices;
    }
}
