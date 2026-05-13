using Catalog.Core.Application.UseCases.GameUser.AddGameUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Core.Domain.Interfaces
{
    public interface IAddGameUserUseCase
    {
        public Task<AddGameUserOutput> ExecuteAsync(AddGameUserInput input);
    }
}
