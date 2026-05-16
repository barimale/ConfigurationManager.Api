using Microsoft.Extensions.Configuration;
using System;

namespace ConfigurationManager.Api
{
    public static class ConsulConfigurationBuilderExtensions
    {
        /// <summary>
        /// Adds Consul configuration provider to the configuration builder.
        /// </summary>
        /// <param name="builder">The configuration builder.</param>
        /// <param name="hostName">The Consul server hostname.</param>
        /// <param name="port">The Consul server port.</param>
        /// <param name="serviceHostName">The service host name (optional).</param>
        /// <param name="mainFolder">The main folder path in Consul (optional).</param>
        /// <returns>The configuration builder for chaining.</returns>
        public static IConfigurationBuilder AddConsul(
            this IConfigurationBuilder builder,
            string hostName,
            int port,
            string serviceHostName = "",
            string mainFolder = "")
        {
            return AddConsul(builder, hostName, port, serviceHostName, mainFolder, optional: false);
        }

        /// <summary>
        /// Adds Consul configuration provider to the configuration builder with optional flag.
        /// </summary>
        /// <param name="builder">The configuration builder.</param>
        /// <param name="hostName">The Consul server hostname.</param>
        /// <param name="port">The Consul server port.</param>
        /// <param name="serviceHostName">The service host name (optional).</param>
        /// <param name="mainFolder">The main folder path in Consul (optional).</param>
        /// <param name="optional">Whether the Consul configuration source is optional.</param>
        /// <returns>The configuration builder for chaining.</returns>
        public static IConfigurationBuilder AddConsul(
            this IConfigurationBuilder builder,
            string hostName,
            int port,
            string serviceHostName = "",
            string mainFolder = "",
            bool optional = false)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (string.IsNullOrWhiteSpace(hostName))
                throw new ArgumentNullException(nameof(hostName));
            if (port <= 0)
                throw new ArgumentException("Port must be greater than 0.", nameof(port));

            var source = new ConsulConfigurationSource(hostName, port, serviceHostName, mainFolder)
            {
                Optional = optional
            };

            return builder.Add(source);
        }

        /// <summary>
        /// Adds Consul configuration provider to the configuration builder with custom action.
        /// </summary>
        /// <param name="builder">The configuration builder.</param>
        /// <param name="configureAction">Action to configure the Consul source.</param>
        /// <returns>The configuration builder for chaining.</returns>
        public static IConfigurationBuilder AddConsul(
            this IConfigurationBuilder builder,
            Action<ConsulConfigurationSource> configureAction)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (configureAction == null)
                throw new ArgumentNullException(nameof(configureAction));

            var source = new ConsulConfigurationSource("localhost", 8500);
            configureAction(source);

            return builder.Add(source);
        }
    }
}
