using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace VictronDataAdapter
{
    public static class ConfigValidator
    {
        public static IList<string> ValidateConfig<TConfig>(this IServiceProvider provider)
            where TConfig : class, new()
        {
            var options = provider.GetService<IOptions<TConfig>>();

            return ValidateConfig<TConfig>(options.Value);
        }

        private static IList<string> ValidateConfig<TConfig>(TConfig config)
        {
            var context = new ValidationContext(config, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(config, context, results, true);

            var messages = new List<string>();
            foreach (var validationResult in results)
            {
                messages.Add(validationResult.ErrorMessage);
            }

            return messages;
        }
    }
}