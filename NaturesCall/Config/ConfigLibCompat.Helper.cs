using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using NaturesCall.Util.StatModifier;
using Vintagestory.API.Config;

namespace NaturesCall.Config;

public partial class ConfigLibCompat
{
    private bool OnCheckBox(string id, bool value, string name, bool isDisabled = false)
    {
        var newValue = value && !isDisabled;
        if (isDisabled)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * 0.5f);
        }
        if (ImGui.Checkbox(Lang.Get(settingPrefix + name) + $"##{name}-{id}", ref newValue))
        {
            if (isDisabled)
            {
                newValue = value;
            }
        }
        if (isDisabled)
        {
            ImGui.PopStyleVar();
        }
        return newValue;
    }

    private bool OnCheckBoxWithoutTranslation(string id, bool value, string name)
    {
        var newValue = value;
        ImGui.Checkbox(name + $"##{name}-{id}", ref newValue);
        return newValue;
    }

    private int OnInputInt(string id, int value, string name, int minValue = default)
    {
        var newValue = value;
        ImGui.InputInt(Lang.Get(settingPrefix + name) + $"##{name}-{id}", ref newValue, step: 1, step_fast: 10);
        return newValue < minValue ? minValue : newValue;
    }

    private float OnInputFloat(string id, float value, string name, float minValue = default)
    {
        var newValue = value;
        ImGui.InputFloat(Lang.Get(settingPrefix + name) + $"##{name}-{id}", ref newValue, step: 0.01f, step_fast: 1.0f);
        return newValue < minValue ? minValue : newValue;
    }
    
    private double OnInputDouble(string id, double value, string name, double minValue = default)
    {
        var newValue = value;
        ImGui.InputDouble(Lang.Get(settingPrefix + name) + $"##{name}-{id}", ref newValue, step: 0.01f, step_fast: 1.0f);
        return newValue < minValue ? minValue : newValue;
    }

    private string OnInputText(string id, string value, string name)
    {
        var newValue = value;
        ImGui.InputText(Lang.Get(settingPrefix + name) + $"##{name}-{id}", ref newValue, 64);
        return newValue;
    }
    
    private string OnInputHex(string id, string value, string name)
    {
        var newValue = value;
        ImGui.InputTextWithHint(Lang.Get(settingPrefix + name) + $"##{name}-{id}", textSupportsHex,ref newValue, 64);
        if (string.IsNullOrEmpty(newValue) || !value.StartsWith("#") ||
            (newValue.Length != 7 && newValue.Length != 9)) return value;
        return newValue;
    }
    
    private IEnumerable<string> OnInputTextMultiline(string id, IEnumerable<string> values, string name)
    {
        var newValue = values.Any() ? values.Aggregate((first, second) => $"{first}\n{second}") : "";
        ImGui.InputTextMultiline(Lang.Get(settingPrefix + name) + $"##{name}-{id}", ref newValue, 256, new(0, 0));
        return newValue.Split('\n', StringSplitOptions.RemoveEmptyEntries).AsEnumerable();
    }
    
    private T OnInputEnum<T>(string id, T value, string name) where T : Enum
    {
        var enumNames = Enum.GetNames(typeof(T));
        var index = Array.IndexOf(enumNames, value.ToString());

        if (ImGui.Combo(Lang.Get(settingPrefix + name) + $"##{name}-{id}", ref index, enumNames, enumNames.Length))
        {
            value = (T)Enum.Parse(typeof(T), enumNames[index]);
        }

        return value;
    }
    
    private List<string> OnInputList(string id, List<string> values, string name)
    {
        var newValues = new List<string>(values);
        for (var i = 0; i < newValues.Count; i++)
        {
            var newValue = newValues[i];
            ImGui.InputText($"{name}[{i}]##{name}-{id}-{i}", ref newValue, 64);
            newValues[i] = newValue;
        }

        if (ImGui.Button($"Add##{name}-{id}"))
        {
            newValues.Add("");
        }

        return newValues;
    }
    
    private List<T> OnInputList<T>(string id, List<T> values, string name) where T : struct, Enum
    {
        var newValues = new List<T>(values);
        for (var i = 0; i < newValues.Count; i++)
        {
            var newValue = newValues[i].ToString();
            ImGui.InputText($"{name}[{i}]##{name}-{id}-{i}", ref newValue, 64);
            if (Enum.TryParse(newValue, out T parsedValue))
            {
                newValues[i] = parsedValue;
            }
        }

        if (ImGui.Button($"Add##{name}-{id}"))
        {
            newValues.Add(default);
        }

        return newValues;
    }
    
    private void DictionaryEditor<T>(Dictionary<string, T> dict, T defaultValue = default, string hint = "", string[] possibleValues = null)
    {
        if (ImGui.BeginTable("dict", 2, ImGuiTableFlags.BordersOuter))
        {
            for (var row = 0; row < dict.Count; row++)
            {
                ImGui.TableNextRow();
                var key = dict.Keys.ElementAt(row);
                var prevKey = (string)key.Clone();
                var value = dict.Values.ElementAt(row);
                ImGui.TableNextColumn();
                ImGui.InputTextWithHint($"##text-{row}", hint, ref key, 300);
                if (prevKey != key)
                {
                    dict.Remove(prevKey);
                    dict.TryAdd(key, value);
                    value = dict.Values.ElementAt(row);
                }
                ImGui.TableNextColumn();
                if (typeof(T) == typeof(int))
                {
                    var intValue = Convert.ToInt32(value);
                    ImGui.InputInt($"##int-{row}" + key, ref intValue);
                    value = (T)Convert.ChangeType(intValue, typeof(T));
                }
                else if (typeof(T) == typeof(float))
                {
                    var floatValue = Convert.ToSingle(value);
                    ImGui.InputFloat($"##float-{row}" + key, ref floatValue);
                    value = (T)Convert.ChangeType(floatValue, typeof(T));
                }
                else if (typeof(T) == typeof(bool))
                {
                    var boolValue = Convert.ToBoolean(value);
                    ImGui.Checkbox($"##boolean-{row}" + key, ref boolValue);
                    value = (T)Convert.ChangeType(boolValue, typeof(T));
                }
                else if (typeof(T) == typeof(StatMultiplier))
                {
                    if (value is not StatMultiplier customValue) continue;
                    customValue.Multiplier = OnInputFloat($"##float-{row}" + key, customValue.Multiplier, nameof(StatMultiplier.Multiplier));
                    customValue.Centering = OnInputEnum($"##centering-{row}" + key, customValue.Centering, nameof(StatMultiplier.Centering));
                    customValue.CurveType = OnInputEnum($"##curve-{row}" + key, customValue.CurveType, nameof(StatMultiplier.CurveType));
                    customValue.LowerHalfCurveType = OnInputEnum($"##lowerhalf-{row}" + key, customValue.LowerHalfCurveType, nameof(StatMultiplier.LowerHalfCurveType));
                    customValue.Inverted = OnCheckBoxWithoutTranslation($"##boolean-{row}" + key, customValue.Inverted, nameof(StatMultiplier.Inverted));
                    value = (T)Convert.ChangeType(customValue, typeof(StatMultiplier));
                }
                dict[key] = value;
                if (ImGui.Button($"Remove##row-value-{row}"))
                {
                    dict.Remove(key);
                }
            }
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            if (ImGui.Button("Add"))
            {
                var id = dict.Count;
                var newKey = possibleValues?.FirstOrDefault(x => !dict.ContainsKey(x)) ?? $"row {id}";
                while (dict.ContainsKey(newKey)) newKey = $"row {++id}";
                dict.TryAdd(newKey, defaultValue);
            }
            ImGui.TableNextColumn();
            ImGui.EndTable();
        }
    }
    
    private void DisplayEnumFloatDictionary<T>(Dictionary<T, float> dictionary, string name, string id) where T : Enum
    {
        if (ImGui.CollapsingHeader(Lang.Get(settingPrefix + name) + $"##dictEnumFloat-{id}"))
        {
            ImGui.Indent();
            foreach (var pair in dictionary)
            {
                var key = pair.Key;
                var value = pair.Value;

                ImGui.Text(key.ToString());
                ImGui.SameLine();
                ImGui.InputFloat($"##{key}", ref value);
                
                dictionary[key] = value;
            }
            ImGui.Unindent();
        }
    }
}