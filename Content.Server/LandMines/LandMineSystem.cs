// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 KISS <59531932+YuriyKiss@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Yurii Kis <yurii.kis@smartteksas.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Explosion.EntitySystems;
using Content.Shared.Armable;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.LandMines;
using Content.Shared.Popups;
using Content.Shared.StepTrigger.Systems;
using Robust.Shared.Audio.Systems;

namespace Content.Server.LandMines;

public sealed class LandMineSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly TriggerSystem _trigger = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LandMineComponent, StepTriggeredOnEvent>(HandleStepOnTriggered);
        SubscribeLocalEvent<LandMineComponent, StepTriggeredOffEvent>(HandleStepOffTriggered);
        SubscribeLocalEvent<LandMineComponent, StepTriggerAttemptEvent>(HandleStepTriggerAttempt);
    }

    /// <summary>
    /// Warns the player when stepped on.
    /// </summary>
    private void HandleStepOnTriggered(EntityUid uid, LandMineComponent component, ref StepTriggeredOnEvent args)
    {
      if (!string.IsNullOrEmpty(component.TriggerText))
      {
          _popupSystem.PopupCoordinates(
              Loc.GetString(component.TriggerText, ("mine", uid)),
              Transform(uid).Coordinates,
              args.Tripper,
              PopupType.LargeCaution);
      }
      _audioSystem.PlayPvs(component.Sound, uid);
    }

    /// <summary>
    /// Sends a trigger when stepped off.
    /// </summary>
    private void HandleStepOffTriggered(EntityUid uid, LandMineComponent component, ref StepTriggeredOffEvent args)
    {
        _trigger.Trigger(uid, args.Tripper);
    }

    /// <summary>
    /// Presumes that the landmine isn't armable and should be treated as always armed.
    /// If Armable and ItemToggle is present the event will continue only if the mine is activated.
    /// </summary>
    private void HandleStepTriggerAttempt(EntityUid uid, LandMineComponent component, ref StepTriggerAttemptEvent args)
    {
        args.Continue = true;

        if (HasComp<ArmableComponent>(uid) && TryComp<ItemToggleComponent>(uid, out var itemToggle))
            args.Continue = itemToggle.Activated;
    }
}