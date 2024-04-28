namespace Content.Server.Station.Components;

/// <summary>
/// Stores Station Assignment Information of a Player/Entity
/// </summary>
[RegisterComponent]
public sealed class StationAssignmentComponent : Component
{

    //uid of the station the player is assigned to
    [ViewVariables]
    public EntityUid? AssignedStationUid;

    /// <summary>
    /// Whether or not the station assignment is "locked"
    /// If not locked then reassign the station each time the entity arrives at a new station
    /// Otherwise some other kind of intervention will be required
    /// </summary>
    [ViewVariables]
    public bool StationAssignmentLocked = false;

}

