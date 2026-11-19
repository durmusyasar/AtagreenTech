using AtaGreenTech.UserService.Application.Features.System.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.System.Queries.GetSystemMenuById;

public sealed record GetSystemMenuByIdQuery(Guid Id) : IQuery<SystemMenuDetailDto>;
