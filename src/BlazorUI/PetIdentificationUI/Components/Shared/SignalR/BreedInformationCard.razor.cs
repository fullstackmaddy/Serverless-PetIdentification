using Microsoft.AspNetCore.Components;
using PetIdentificationUI.Models;
using System.Threading.Tasks;

namespace PetIdentificationUI.Components.Shared.SignalR
{
    public partial class BreedInformationCard : ComponentBase
    {
        [Parameter] public BreedInfo BreedInformation { get; set; }
        private string imageData;

        protected override void OnParametersSet()
        {
            imageData = string.Format("{0},{1}", "data:image/jpeg;base64", BreedInformation.StockImage);
        }

    }


}
