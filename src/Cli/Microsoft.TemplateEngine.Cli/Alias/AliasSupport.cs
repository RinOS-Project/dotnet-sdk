// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Cli.TabularOutput;

namespace Microsoft.TemplateEngine.Cli.Alias
{
    /// <summary>
    /// Shared validation, persistence and expansion behavior for <c>dotnet new alias</c>.
    /// </summary>
    internal static class AliasSupport
    {
        // Alias names are persisted and later used as the first command token. Keep the
        // accepted grammar deliberately narrower than a template option grammar.
        private static readonly Regex InvalidAliasRegex = new("[^a-z0-9_.]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex ValidFirstTokenRegex = new("^[a-z0-9]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        internal static NewCommandStatus ManipulateAlias(
            AliasRegistry aliasRegistry,
            string? aliasName,
            IReadOnlyList<string> aliasTokens,
            IReadOnlySet<string> reservedAliasNames)
        {
            if (string.IsNullOrWhiteSpace(aliasName))
            {
                Reporter.Error.WriteLine(LocalizableStrings.AliasNotCreatedInvalidInput);
                return NewCommandStatus.InvalidOption;
            }

            if (reservedAliasNames.Contains(aliasName))
            {
                Reporter.Error.WriteLine(string.Format(LocalizableStrings.AliasCannotBeShortName, aliasName));
                return NewCommandStatus.CreateFailed;
            }

            if (InvalidAliasRegex.IsMatch(aliasName))
            {
                Reporter.Error.WriteLine(LocalizableStrings.AliasNameContainsInvalidCharacters);
                return NewCommandStatus.InvalidOption;
            }

            if (aliasTokens.Count > 0 && !ValidFirstTokenRegex.IsMatch(aliasTokens[0]))
            {
                Reporter.Error.WriteLine(LocalizableStrings.AliasValueFirstArgError);
                return NewCommandStatus.InvalidOption;
            }

            AliasManipulationResult result = aliasRegistry.TryCreateOrRemoveAlias(aliasName, aliasTokens);
            switch (result.Status)
            {
                case AliasManipulationStatus.Created:
                    Reporter.Output.WriteLine(string.Format(LocalizableStrings.AliasCreated, result.AliasName, string.Join(" ", result.AliasTokens)));
                    return NewCommandStatus.Success;
                case AliasManipulationStatus.Removed:
                    Reporter.Output.WriteLine(string.Format(LocalizableStrings.AliasRemoved, result.AliasName, string.Join(" ", result.AliasTokens)));
                    return NewCommandStatus.Success;
                case AliasManipulationStatus.Updated:
                    Reporter.Output.WriteLine(string.Format(LocalizableStrings.AliasUpdated, result.AliasName, string.Join(" ", result.AliasTokens)));
                    return NewCommandStatus.Success;
                case AliasManipulationStatus.RemoveNonExistentFailed:
                    Reporter.Error.WriteLine(string.Format(LocalizableStrings.AliasRemoveNonExistentFailed, result.AliasName));
                    return NewCommandStatus.CreateFailed;
                case AliasManipulationStatus.WouldCreateCycle:
                    Reporter.Error.WriteLine(LocalizableStrings.AliasCycleError);
                    return NewCommandStatus.CreateFailed;
                default:
                    Reporter.Error.WriteLine(LocalizableStrings.AliasNotCreatedInvalidInput);
                    return NewCommandStatus.InvalidOption;
            }
        }

        internal static NewCommandStatus DisplayAliasValues(
            IEngineEnvironmentSettings environmentSettings,
            AliasRegistry aliasRegistry,
            string? aliasName,
            string commandName)
        {
            IReadOnlyDictionary<string, IReadOnlyList<string>> aliasesToShow;
            if (!string.IsNullOrWhiteSpace(aliasName))
            {
                if (!aliasRegistry.AllAliases.TryGetValue(aliasName, out IReadOnlyList<string>? aliasValue))
                {
                    Reporter.Error.WriteLine(string.Format(LocalizableStrings.AliasShowErrorUnknownAlias, aliasName, commandName));
                    return NewCommandStatus.InvalidOption;
                }

                aliasesToShow = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    [aliasName] = aliasValue
                };
            }
            else
            {
                aliasesToShow = aliasRegistry.AllAliases;
                Reporter.Output.WriteLine(LocalizableStrings.AliasShowAllAliasesHeader);
            }

            TabularOutput<KeyValuePair<string, IReadOnlyList<string>>> formatter =
                TabularOutput.For(
                    new TabularOutputSettings(environmentSettings.Environment),
                    aliasesToShow.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
                .DefineColumn(pair => pair.Key, LocalizableStrings.AliasName, showAlways: true)
                .DefineColumn(pair => string.Join(" ", pair.Value), LocalizableStrings.AliasValue, showAlways: true);

            Reporter.Output.WriteLine(formatter.Layout());
            return NewCommandStatus.Success;
        }

        /// <summary>
        /// Expands only the command's first token. Alias values are complete token lists,
        /// so the remaining template arguments stay in their original order.
        /// </summary>
        internal static bool TryExpandAliases(
            AliasRegistry aliasRegistry,
            IReadOnlyList<string> inputTokens,
            out IReadOnlyList<string> expandedTokens)
        {
            if (inputTokens.Count == 0)
            {
                expandedTokens = Array.Empty<string>();
                return true;
            }

            if (!aliasRegistry.TryExpandCommandAliases(inputTokens, out expandedTokens!))
            {
                Reporter.Error.WriteLine(LocalizableStrings.AliasExpansionError);
                expandedTokens = Array.Empty<string>();
                return false;
            }

            if (!expandedTokens.SequenceEqual(inputTokens, StringComparer.Ordinal))
            {
                Reporter.Output.WriteLine(string.Format(LocalizableStrings.AliasCommandAfterExpansion, string.Join(" ", expandedTokens)));
            }

            return true;
        }
    }
}
