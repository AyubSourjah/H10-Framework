using Microsoft.Extensions.Configuration;

namespace H10.Shared
{
    public class AppSetting
    {
        private readonly IConfiguration _configuration;

        public AppSetting(IConfiguration configuration)
        {
            _configuration = configuration;
            
            this.BuildConfiguration();
        }

        public string GetValue(string key)
        {
            return string.Empty;
        }

        public void Refresh()
        {
        }

        private void BuildConfiguration()
        {
        }
    }
}