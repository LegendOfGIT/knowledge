using System;
using System.Configuration;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace FB.Services.Extensions.Behaviors
{
    public class BiProBehavior : BehaviorExtensionElement, IEndpointBehavior
    {
        private const string UseTokenKey = "UseToken";
        private const string TokenPathKey = "TokenPath";
        private const string TokenURLKey = "TokenURL";
        private const string TokenUsernameKey = "TokenUsername";
        private const string TokenPasswordKey = "TokenPassword";
        private const string TokenTemplatepathKey = "TokenTemplatepath";

        private const string NamespacesKey = "Namespaces";
        private const string repairAttributePrefixesKey = "RepairAttributePrefixes";

        private const string NuernbergerBiPROWorkaroundKey = "NuernbergerBiPROWorkaround";

        
        protected static ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty useTokenProperty = new ConfigurationProperty(UseTokenKey, typeof(bool?), (object)null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty tokenPathProperty = new ConfigurationProperty(TokenPathKey, typeof(string), (object)null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty tokenTemplatepathProperty = new ConfigurationProperty(TokenTemplatepathKey, typeof(string), (object)null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty namespacesProperty = new ConfigurationProperty(NamespacesKey, typeof(string), (object)null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty repairAttributePrefixesProperty = new ConfigurationProperty(repairAttributePrefixesKey, typeof(bool?), (object)null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty tokenURLProperty = new ConfigurationProperty(TokenURLKey, typeof(string), (object)null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty tokenUsernameProperty = new ConfigurationProperty(TokenUsernameKey, typeof(string), (object)null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty tokenPasswordProperty = new ConfigurationProperty(TokenPasswordKey, typeof(string), (object)null, ConfigurationPropertyOptions.None);
        
        private static readonly ConfigurationProperty nuernbergerBiPROWorkaroundProperty = new ConfigurationProperty(NuernbergerBiPROWorkaroundKey, typeof(bool?), (object)null, ConfigurationPropertyOptions.None);

        static BiProBehavior() {
            BiProBehavior.properties.Add(useTokenProperty);
            BiProBehavior.properties.Add(tokenPathProperty);
            BiProBehavior.properties.Add(tokenTemplatepathProperty);
            BiProBehavior.properties.Add(namespacesProperty);
            BiProBehavior.properties.Add(repairAttributePrefixesProperty);
            BiProBehavior.properties.Add(tokenURLProperty);
            BiProBehavior.properties.Add(tokenUsernameProperty);
            BiProBehavior.properties.Add(tokenPasswordProperty);

            BiProBehavior.properties.Add(nuernbergerBiPROWorkaroundProperty);
        }

        [ConfigurationProperty(UseTokenKey, DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
        public bool? UseToken
        {
            get
            {
                return (bool?)this[UseTokenKey];
            }
            set
            {
                this[UseTokenKey] = (object)value;
            }
        }
        [ConfigurationProperty(TokenPathKey, DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
        public string TokenPath
        {
            get
            {
                return (string)this[TokenPathKey];
            }
            set
            {
                this[TokenPathKey] = (object)value;
            }
        }
        [ConfigurationProperty(TokenTemplatepathKey, DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
        public string TokenTemplatepath
        {
            get
            {
                return (string)this[TokenTemplatepathKey];
            }
            set
            {
                this[TokenTemplatepathKey] = (object)value;
            }
        }
        [ConfigurationProperty(NamespacesKey, DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
        public string Namespaces
        {
            get
            {
                return (string)this[NamespacesKey];
            }
            set
            {
                this[NamespacesKey] = (object)value;
            }
        }
        [ConfigurationProperty(repairAttributePrefixesKey, DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
        public bool? RepairAttributePrefixes
        {
            get
            {
                return (bool?)this[repairAttributePrefixesKey];
            }
            set
            {
                this[repairAttributePrefixesKey] = (object)value;
            }
        }
        [ConfigurationProperty(TokenURLKey, DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
        public string TokenURL
        {
            get
            {
                return (string)this[TokenURLKey];
            }
            set
            {
                this[TokenURLKey] = (object)value;
            }
        }
        [ConfigurationProperty(TokenUsernameKey, DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
        public string TokenUsername
        {
            get
            {
                return (string)this[TokenUsernameKey];
            }
            set
            {
                this[TokenUsernameKey] = (object)value;
            }
        }
        [ConfigurationProperty(TokenPasswordKey, DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
        public string TokenPassword
        {
            get
            {
                return (string)this[TokenPasswordKey];
            }
            set
            {
                this[TokenPasswordKey] = (object)value;
            }
        }

        [ConfigurationProperty(NuernbergerBiPROWorkaroundKey, DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
        public bool? NuernbergerBiPROWorkaround
        {
            get
            {
                return (bool?)this[NuernbergerBiPROWorkaroundKey];
            }
            set
            {
                this[NuernbergerBiPROWorkaroundKey] = (object)value;
            }
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(new BiProMessageInspector{
                UseToken = this.UseToken,
                TokenURL = this.TokenURL,
                TokenUsername = this.TokenUsername,
                TokenPassword = this.TokenPassword,
                Tokenpath = this.TokenPath,
                Namespaces = this.Namespaces,
                RepairAttributePrefixes = this.RepairAttributePrefixes,
                TokenTemplatepath = this.TokenTemplatepath,

                NuernbergerBiPROWorkaround = this.NuernbergerBiPROWorkaround
            });
        }
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }
        public void Validate(ServiceEndpoint endpoint) { }
        public override Type BehaviorType
        {
            get { return typeof(BiProBehavior); }
        }
        protected override object CreateBehavior()
        {
            var response = new BiProBehavior {
                UseToken = this.UseToken,
                TokenPath = this.TokenPath,
                Namespaces = this.Namespaces,
                RepairAttributePrefixes = this.RepairAttributePrefixes,
                TokenURL = this.TokenURL,
                TokenUsername = this.TokenUsername,
                TokenPassword = this.TokenPassword,
                TokenTemplatepath = this.TokenTemplatepath,

                NuernbergerBiPROWorkaround = this.NuernbergerBiPROWorkaround
            };

            return response;
        }
        protected override ConfigurationPropertyCollection Properties {
            get
            {
                return BiProBehavior.properties;
            }
        }
    }
}
