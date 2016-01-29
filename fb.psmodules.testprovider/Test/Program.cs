using FB.PSModules.UnitTestProvider;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var configs = GetProductConfigurationsInformationCmdlet.GetProductconfigurationsInformation("beta", "WURL", "");
        }
    }
}
