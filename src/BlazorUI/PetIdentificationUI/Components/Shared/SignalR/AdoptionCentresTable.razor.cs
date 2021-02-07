using Microsoft.AspNetCore.Components;
using PetIdentificationUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetIdentificationUI.Components.Shared.SignalR
{
    public partial class AdoptionCentresTable : ComponentBase
    {
        [Parameter] public List<AdoptionCentre> AdoptionCentres { get; set; }


    }
}
