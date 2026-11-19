using AtaGreenTech.UserService.Application.Features.System.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.System.Queries.GetSystemLanguages;

public sealed record GetSystemLanguagesQuery(bool ActiveOnly = true)
    : IQuery<IReadOnlyList<SystemLanguageDto>>;
