// using Microsoft.Xna.Framework;
// using Silk.NET.OpenAL.Extensions.Creative;
// using SpringProject.Core.Debugging;

// namespace SpringProject.Core.Components;

// public class LowPassSource : Component
// {
//     uint _filter;
//     string _registeredChannel = "";

//     [Parameter("Channel")]
//     public string Channel { get; set; } = "";

//     [Parameter("Gain", 0f, 1f)]
//     public float Gain { get; set; } = 1f;

//     [Parameter("Gain HF", 0f, 1f)]
//     public float GainHF { get; set; } = 0.5f;

//     public override void Start()
//     {
//         RegisterFilter();
//     }

//     public override void EditorUpdate(GameTime gameTime)
//     {
//         if (_registeredChannel != Channel)
//         {
//             UnregisterFilter();
//             RegisterFilter();
//         }

//         ApplyProperties();
//     }

//     void RegisterFilter()
//     {
//         var efx = AudioManager.Efx;
//         if (efx == null || string.IsNullOrEmpty(Channel)) return;

//         var channel = AudioManager.GetChannel(Channel);
//         if (channel == null)
//         {
//             Debug.Log($"LowPassSource: channel '{Channel}' does not exist.");
//             return;
//         }

//         _filter = efx.GenFilter();
//         ApplyProperties();

//         channel.AddFilter(_filter);
//         _registeredChannel = Channel;
//     }

//     void UnregisterFilter()
//     {
//         if (_filter == 0) return;

//         AudioManager.GetChannel(_registeredChannel)?.RemoveFilter(_filter);
//         AudioManager.Efx?.DeleteFilter(_filter);

//         _filter = 0;
//         _registeredChannel = "";
//     }

//     void ApplyProperties()
//     {
//         var efx = AudioManager.Efx;
//         if (efx == null || _filter == 0) return;

//         efx.SetFilterProperty(_filter, FilterInteger.FilterType, (int)FilterType.Lowpass);
//         efx.SetFilterProperty(_filter, FilterFloat.LowpassGain, Gain);
//         efx.SetFilterProperty(_filter, FilterFloat.LowpassGainHF, GainHF);

//         // notify channel to reconnect sources with updated filter
//         AudioManager.GetChannel(_registeredChannel)?.RefreshSources();
//     }

//     public override void OnDestroy()
//     {
//         UnregisterFilter();
//         base.OnDestroy();
//     }
// }