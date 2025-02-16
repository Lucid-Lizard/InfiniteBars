using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace ProceduralOres.Core.Loaders.UILoading;

/// <summary>
///     Automatically loads SmartUIStates ala IoC.
/// </summary>
internal class UILoader : ModSystem
{
	/// <summary>
	///     The collection of automatically craetaed UserInterfaces for SmartUIStates.
	/// </summary>
	private static List<UserInterface> _userInterfaces = new();

	/// <summary>
	///     The collection of all automatically loaded SmartUIStates.
	/// </summary>
	private static List<SmartUIState> _uiStates = new();

	/// <summary>
	///     Uses reflection to scan through and find all types extending SmartUIState that arent abstract, and loads an
	///     instance of them.
	/// </summary>
	public override void Load()
    {
        if (Main.dedServ)
            return;

        _userInterfaces = [];
        _uiStates = [];

        foreach (var t in Mod.Code.GetTypes())
            if (!t.IsAbstract && t.IsSubclassOf(typeof(SmartUIState)))
            {
                var state = (SmartUIState)Activator.CreateInstance(t, null);
                var userInterface = new UserInterface();
                userInterface.SetState(state);
                if (state != null)
                {
                    state.UserInterface = userInterface;

                    _uiStates?.Add(state);
                }
                
                _userInterfaces?.Add(userInterface);
            }
    }

    public override void Unload()
    {
        _uiStates.ForEach(n => n.Unload());
        _userInterfaces = null;
        _uiStates = null;
    }

    /// <summary>
    ///     Helper method for creating and inserting a LegacyGameInterfaceLayer automatically
    /// </summary>
    /// <param name="layers">The vanilla layers</param>
    /// <param name="state">the UIState to bind to the layer</param>
    /// <param name="index">Where this layer should be inserted</param>
    /// <param name="visible">The logic dictating the visibility of this layer</param>
    /// <param name="scale">The scale settings this layer should scale with</param>
    private static void AddLayer(List<GameInterfaceLayer> layers, SmartUIState state, int index, bool visible,
        InterfaceScaleType scale)
    {
        layers.Insert(index, new LegacyGameInterfaceLayer(GetLayerName(state),
            delegate
            {
                if (visible)
                    state?.Draw(Main.spriteBatch);

                return true;
            }, scale));
    }

    /// <summary>
    ///     Gets the interface layer name for the provided SmartUIState instance.
    ///     If the state is null, a default value "Unknown" is returned.
    /// </summary>
    /// <param name="state">The SmartUIState instance to get the interface layer name of</param>
    private static string GetLayerName(SmartUIState state)
    {
        var name = state == null ? "Unknown" : state.ToString();
        return $"{nameof(ProceduralOres)}: {name}";
    }

    /// <summary>
    ///     Gets the autoloaded SmartUIState instance for a given SmartUIState subclass
    /// </summary>
    /// <typeparam name="T">The SmartUIState subclass to get the instance of</typeparam>
    /// <returns>The autoloaded instance of the desired SmartUIState</returns>
    public static T GetUIState<T>() where T : SmartUIState
    {
        return _uiStates.FirstOrDefault(n => n is T) as T;
    }

    /// <summary>
    ///     Forcibly reloads a SmartUIState and it's associated UserInterface
    /// </summary>
    /// <typeparam name="T">The SmartUIState subclass to reload</typeparam>
    public static void ReloadState<T>() where T : SmartUIState
    {
        var index = _uiStates.IndexOf(GetUIState<T>());
        _uiStates[index] = (T)Activator.CreateInstance(typeof(T), null);
        _userInterfaces[index] = new UserInterface();
        _userInterfaces[index].SetState(_uiStates[index]);
    }

    /// <summary>
    ///     Handles the insertion of the automatically generated UIs
    /// </summary>
    /// <param name="layers"></param>
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        foreach (var state in _uiStates)
            AddLayer(layers, state, state.InsertionIndex(layers), state.Visible, state.Scale);
    }

    public override void UpdateUI(GameTime gameTime)
    {
        foreach (var eachState in _userInterfaces)
            if (eachState?.CurrentState != null && ((SmartUIState)eachState.CurrentState).Visible)
                eachState.Update(gameTime);
    }
}