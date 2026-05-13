using MediatR;
using Users.Core.Domain.Entities;
using Users.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;


namespace Users.Core.Application.UseCases.Users.GetGameUser
{
    public class GetGameUserUseCase
    {
        private readonly IGetGameUserRepository _GetGameUserRepository;
        private readonly ILogger<GetGameUserUseCase> _logger;

        public GetGameUserUseCase(
            IGetGameUserRepository GetGameUserRepository,
            ILogger<GetGameUserUseCase> logger
        )
        {
            _GetGameUserRepository = GetGameUserRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<User>> ExecuteAsync(GetGameUserInGet inGet)
        {
            return await _GetGameUserRepository.GetGameUserAsync(inGet.Login, inGet.Password, inGet.Email);
        }
    }
}
