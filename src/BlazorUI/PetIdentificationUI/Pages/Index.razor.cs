using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace PetIdentificationUI.Pages
{
    public partial class Index: ComponentBase
    {
        [Inject] public IJSRuntime JSRuntime { get; set; }

        private async Task NavigateToKnowMore()
        {
            await JSRuntime.InvokeAsync<object>("open", "https://devstories.konfhub.com/", "_blank");
        }

    }
}
