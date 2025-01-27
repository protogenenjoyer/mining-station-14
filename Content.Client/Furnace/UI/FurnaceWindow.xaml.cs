using Content.Shared.Mining.Components;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;
using System.Text;
using Content.Shared.Temperature;
using static Content.Shared.Mining.Components.SharedFurnaceComponent;
using FancyWindow = Content.Client.UserInterface.Controls.FancyWindow;

namespace Content.Client.Furnace.UI
{
    [GenerateTypedNameReferences]
    public sealed partial class FurnaceWindow : DefaultWindow
    {

        public FurnaceWindow()
        {
            RobustXamlLoader.Load(this);
        }

        private FurnaceBoundUserInterfaceState? _lastState;

        public void UpdateState(BoundUserInterfaceState state)
        {
            var castState = (FurnaceBoundUserInterfaceState) state;

            DoorButton.Text = "Open";
            if (castState.Opened)
                DoorButton.Text = "Close";

            var tempText = new StringBuilder();

            tempText.Append($"{(int)TemperatureHelpers.KelvinToCelsius(castState.Temperature)}°C");

            Temp.Text = tempText.ToString();

            if (_lastState?.Power != castState.Power)
                TargetPower.SetValueWithoutEvent(castState.Power);

            _lastState = castState;
        }
    }
}
