﻿using System.Reflection;
using CounterStrikeSharp.API.Modules.Utils;

namespace Core.Models;

public class MessageFormatter
{
    public static string FormatColor(string message)
    {
        string result = message;
        foreach (FieldInfo field in typeof(ChatColors).GetFields())
        {
            string pattern = $"{{{field.Name}}}";

            if (result.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                result = result.Replace(pattern, field.GetValue(null)!.ToString(), StringComparison.OrdinalIgnoreCase);
            }
        }

        return result;
    }
}

