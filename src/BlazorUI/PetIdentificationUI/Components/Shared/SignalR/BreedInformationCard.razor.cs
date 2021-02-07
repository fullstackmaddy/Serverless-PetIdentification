using Microsoft.AspNetCore.Components;
using PetIdentificationUI.Models;

namespace PetIdentificationUI.Components.Shared.SignalR
{
    public partial class BreedInformationCard : ComponentBase
    {
        [Parameter] public BreedInfo BreedInformation { get; set; }
        private string GetCardImage()
        {
            return
                string.Format("{0},{1}", "data:image/jpeg;base64", BreedInformation.StockImage);
        }
    }


}
