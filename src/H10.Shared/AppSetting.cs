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

        private void BuildConfiguration()
        {
            string shellBaseUrl = this._configuration["Services/Shell/BaseUrl"];
        }
    }
}