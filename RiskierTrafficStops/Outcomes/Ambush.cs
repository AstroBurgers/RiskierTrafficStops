using LSPD_First_Response.Mod.API;
using Rage;
using System.Collections.Generic;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops.Outcomes
{
    internal class Ambush
    {
        internal static Ped Suspect;
        internal static Vehicle suspectVehicle;
        internal static RelationshipGroup SuspectRelateGroup = new RelationshipGroup("Suspect");

        internal static void AmbushOutcome(LHandle handle)
        {
            if (!GetSuspectAndVehicle(handle, out Suspect, out suspectVehicle))
            {
                CleanupEvent(Suspect, suspectVehicle);
                return;
            }

            Debug("Adding all suspect in the vehicle to a list");

            List<Ped> PedsInVehicle = GetAllVehicleOccupants(suspectVehicle);
            Debug($"Peds In Vehicle: {PedsInVehicle.Count}");

            Debug("Setting Suspect Relationship Group");
            SuspectRelateGroup.SetRelationshipWith(MainPlayer.RelationshipGroup, Relationship.Hate);
            SuspectRelateGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);

            Debug();
            MainPlayer.RelationshipGroup.SetRelationshipWith(SuspectRelateGroup, Relationship.Hate); //Relationship groups go both ways
            RelationshipGroup.Cop.SetRelationshipWith(SuspectRelateGroup, Relationship.Hate);
        }
    }
}