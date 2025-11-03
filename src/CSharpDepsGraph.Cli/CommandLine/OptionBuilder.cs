using System.CommandLine;
using System.CommandLine.Parsing;

namespace CSharpDepsGraph.Cli.CommandLine;

internal static class OptionBuilder
{
    public static Option<T> CreateOption<T>(
        string name,
        string? alias,
        string description,
        string? argumentHelpName = null,
        Func<SymbolResult, (T value, string? error)>? parser = null
        )
    {
        var aliases = new List<string>() { $"--{name}" };
        if (alias != null)
        {
            aliases.Add($"-{alias}");
        }

        var result = parser == null
            ? new Option<T>(aliases: aliases.ToArray())
            : new Option<T>(
                aliases: aliases.ToArray(),
                parseArgument: ar =>
                {
                    var (value, error) = parser(ar);
                    ar.ErrorMessage = error;
                    return value;
                }
            );

        result.Description = Description(description);
        result.ArgumentHelpName = argumentHelpName;

        return result;
    }

    public static Option<T> CreateOption<T>(
        string name,
        string? alias,
        string argumentHelpName,
        string description,
        T defaultValue,
        T[] values
        )
        where T : struct, Enum
    {
        return CreateOption(
            name,
            alias,
            argumentHelpName,
            description,
            defaultValue,
            values.Select(i => (i, "", "")).ToArray()
        );
    }

    public static Option<T> CreateOption<T>(
        string name,
        string? alias,
        string argumentHelpName,
        string description,
        T defaultValue,
        (T value, string alias, string hint)[] values
        )
        where T : struct, Enum
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("Empty", nameof(values));
        }

        if (!values.GroupBy(i => i.value).All(x => x.Count() == 1))
        {
            throw new ArgumentException("Duplication", nameof(values));
        }

        var validValues = values.Select(i => new EnumMemberMeta<T>(i.value, i.alias, i.hint));
        var validValuesHint = string.Join(", ", validValues.Select(v => v.Hint));
        var descriptionWithHint = description
            + $@" Allowed values are: {validValuesHint}.
            ";

        var result = CreateOption(name, alias, descriptionWithHint, argumentHelpName, Parse);
        result.SetDefaultValue(defaultValue);
        result.AddValidator(vsr =>
        {
            var (value, error) = Parse(vsr);
            vsr.ErrorMessage = error;
        });

        return result;

        (T result, string? error) Parse(SymbolResult symbolResult)
        {
            if (symbolResult.Tokens.Count == 0)
            {
                return (default, null);
            }

            var value = symbolResult.Tokens.First().Value.ToLowerInvariant();

            var byName = validValues.SingleOrDefault(i => i.Name == value)?.Value;
            if (byName != null)
            {
                return (byName.Value, null);
            }

            var byAlias = validValues.SingleOrDefault(i => i.Alias == value)?.Value;
            if (byAlias != null)
            {
                return (byAlias.Value, null);
            }

            var error = Description($@"
                Invalid value '{value}' for option '{name}'.
                Must be one of: {validValuesHint}
            ");

            return (default(T), error);
        }
    }

    public static Option<IEnumerable<T>> CreateListOption<T>(
        string name,
        string alias,
        string description,
        string argumentHelpName,
        ParseArgument<IEnumerable<T>> parser
        )
    {
        var result = new Option<IEnumerable<T>>(
            aliases: [$"--{name}", $"-{alias}"],
            description: Description(description),
            parseArgument: parser
        );

        result.Arity = ArgumentArity.ZeroOrMore;
        result.ArgumentHelpName = argumentHelpName.ToLowerInvariant();
        result.SetDefaultValue(Array.Empty<T>());

        return result;
    }

    public static string Description(string value)
    {
        var paragraphs = value.Split(Environment.NewLine + Environment.NewLine)
            .Select(p =>
            {
                var lines = p.Split(
                    Environment.NewLine,
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                );

                return string.Join(" ", lines);
            });

        return string.Join(Environment.NewLine, paragraphs);
    }

    public static T Create<T>(Func<T> action)
    {
        return action();
    }

    private class EnumMemberMeta<T> where T : struct, Enum
    {
        public T Value { get; }
        public string Name { get; }
        public string? Alias { get; }
        public string Hint { get; }

        public EnumMemberMeta(T value, string? alias, string? hint)
        {
            Value = value;
            Name = GetCliForm(value);
            Alias = alias?.ToLowerInvariant();
            Hint = !string.IsNullOrEmpty(hint)
                ? hint
                : string.IsNullOrEmpty(alias) ? $"{Name}" : $"{Name} or {Alias}";
        }

        private static string GetCliForm(T value)
        {
            var str = value.ToString();
            var parts = str.Select((c, i) =>
            {
                return i == 0
                    ? char.ToLowerInvariant(c).ToString()
                    : char.IsUpper(c) ? $"-{char.ToLowerInvariant(c)}" : c.ToString();
            });

            return string.Concat(parts);
        }
    }
}