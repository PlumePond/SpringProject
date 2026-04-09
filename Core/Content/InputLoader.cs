using FontStashSharp;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpringProject.Core.Debugging;
using SpringProject.Core.Editor;
using SpringProject.Core.UserInput;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpringProject.Core.Content;

public static class InputLoader
{
    public static List<InputState> Load(string directory)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var inputStates = new List<InputState>();

        foreach (var jsonFile in Directory.GetFiles(directory, "*.json"))
        {
            var data = JsonSerializer.Deserialize<InputStateJSONData>(File.ReadAllText(jsonFile), options);
            string name = Path.GetFileNameWithoutExtension(jsonFile);

            bool alreadyExists = false;
            foreach (var state in inputStates)
            {
                if (state.Name == name)
                {
                    alreadyExists = true;
                    break;
                }
            }

            if (!alreadyExists)
            {
                var state = new InputState();

                foreach (var bindingData in data.Bindings)
                {
                    var binding = BuildBinding(bindingData);
                    state.AddBinding(binding);
                }

                state.Name = name;
                state.IgnoreLock = data.IgnoreLock;

                inputStates.Add(state);
            }
        }

        Debug.Log($"Input States loaded! ({inputStates.Count}).");

        return inputStates;
    }

    // build binding from key in the json file
    static InputBinding BuildBinding(InputBindingJSONData data) => data.Type switch
    {
        "key" => new KeyBinding(Enum.Parse<Keys>(data.Key, true)),
        "compound_key" => new CompoundKeyBinding(Enum.Parse<Keys>(data.Up, true), Enum.Parse<Keys>(data.Down, true), Enum.Parse<Keys>(data.Left, true), Enum.Parse<Keys>(data.Right, true)),
        "mouse_click" => new MouseClickBinding(Enum.Parse<MouseButton>(data.Button, true)),
        "controller_button" => new ControllerButtonBinding(Enum.Parse<Buttons>(data.Button, true)),
        "mouse_position" => new MousePositionBinding(),
        "scroll" => new MouseScrollBinding(),
        "modifier" => new ModifierBinding(data.Keys.Select(k => Enum.Parse<Keys>(k, true)).ToArray()),
        _ => throw new Exception($"Unknown binding type: {data.Type}")
    };

    class InputBindingJSONData
    {
        public string Type { get; set; }
        public string Key { get; set; }
        public string Button { get; set; }
        public string[] Keys { get; set; }
        
        // compound key binding
        public string Up { get; set; }
        public string Down { get; set; }
        public string Left { get; set; }
        public string Right { get; set; }
    }

    class InputStateJSONData
    {
        [JsonPropertyName("ignoreLock")] public bool IgnoreLock { get; set; } = false;
        public InputBindingJSONData[] Bindings { get; set; }
    }
}