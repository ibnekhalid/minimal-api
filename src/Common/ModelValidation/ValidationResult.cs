using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.MinimalValidator;

public class ValidationResult
{
    public bool IsValid { get; set; }
    public Dictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
    public void ThorwInvalidArgumentsException()
    {
        if (!IsValid)
            throw new ArgumentException(string.Join("\n", Errors.Select(x => $"{x.Key}: {string.Join("\n", x.Value)}")));
    }
}

