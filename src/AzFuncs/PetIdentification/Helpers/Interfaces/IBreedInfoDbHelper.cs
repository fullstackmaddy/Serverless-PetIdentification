using PetIdentification.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PetIdentification.Interfaces
{
    public interface IBreedInfoDbHelper
    {

        public Task<BreedInfo> GetBreedInformationAsync(string breed);
    }
}
