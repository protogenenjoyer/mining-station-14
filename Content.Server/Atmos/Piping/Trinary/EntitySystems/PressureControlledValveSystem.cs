using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Atmos.Piping.Trinary.Components;
using Content.Server.NodeContainer;
using Content.Server.NodeContainer.Nodes;
using Content.Shared.Atmos.Piping;
using Content.Shared.Audio;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Piping.Trinary.EntitySystems
{
    [UsedImplicitly]
    public sealed class PressureControlledValveSystem : EntitySystem
    {
        [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
        [Dependency] private readonly SharedAmbientSoundSystem _ambientSoundSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PressureControlledValveComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<PressureControlledValveComponent, AtmosDeviceUpdateEvent>(OnUpdate);
            SubscribeLocalEvent<PressureControlledValveComponent, AtmosDeviceDisabledEvent>(OnFilterLeaveAtmosphere);
        }

        private void OnInit(EntityUid uid, PressureControlledValveComponent comp, ComponentInit args)
        {
            UpdateAppearance(uid, comp);
        }

        private void OnUpdate(EntityUid uid, PressureControlledValveComponent comp, AtmosDeviceUpdateEvent args)
        {
            if (!EntityManager.TryGetComponent(uid, out NodeContainerComponent? nodeContainer)
                || !EntityManager.TryGetComponent(uid, out AtmosDeviceComponent? device)
                || !nodeContainer.TryGetNode(comp.InletName, out PipeNode? inletNode)
                || !nodeContainer.TryGetNode(comp.ControlName, out PipeNode? controlNode)
                || !nodeContainer.TryGetNode(comp.OutletName, out PipeNode? outletNode))
            {
                _ambientSoundSystem.SetAmbience(comp.Owner, false);
                comp.Enabled = false;
                return;
            }

            // If output is higher than input, flip input/output to enable bidirectional flow.
            if (outletNode.Air.Pressure > inletNode.Air.Pressure)
            {
                PipeNode temp = outletNode;
                outletNode = inletNode;
                inletNode = temp;
            }

            float control = (controlNode.Air.Pressure - outletNode.Air.Pressure) - comp.Threshold;
            float transferRate;
            if (control < 0)
            {
                comp.Enabled = false;
                transferRate = 0;
            }
            else
            {
                comp.Enabled = true;
                transferRate = Math.Min(control * comp.Gain, comp.MaxTransferRate);
            }
            UpdateAppearance(uid, comp);

            // We multiply the transfer rate in L/s by the seconds passed since the last process to get the liters.
            var transferVolume = (float)(transferRate * args.dt);
            if (transferVolume <= 0)
            {
                _ambientSoundSystem.SetAmbience(comp.Owner, false);
                return;
            }

            _ambientSoundSystem.SetAmbience(comp.Owner, true);
            var removed = inletNode.Air.RemoveVolume(transferVolume);
            _atmosphereSystem.Merge(outletNode.Air, removed);
        }

        private void OnFilterLeaveAtmosphere(EntityUid uid, PressureControlledValveComponent comp, AtmosDeviceDisabledEvent args)
        {
            comp.Enabled = false;
            UpdateAppearance(uid, comp);
            _ambientSoundSystem.SetAmbience(comp.Owner, false);
        }

        private void UpdateAppearance(EntityUid uid, PressureControlledValveComponent? comp = null, AppearanceComponent? appearance = null)
        {
            if (!Resolve(uid, ref comp, ref appearance, false))
                return;

            appearance.SetData(FilterVisuals.Enabled, comp.Enabled);
        }
    }
}
