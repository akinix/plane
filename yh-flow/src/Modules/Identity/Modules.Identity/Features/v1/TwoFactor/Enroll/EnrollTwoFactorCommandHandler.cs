using System.Text.Encodings.Web;
using YH.Framework.Core.Context;
using YH.Framework.Core.Exceptions;
using YH.Modules.Identity.Contracts.DTOs;
using YH.Modules.Identity.Contracts.v1.TwoFactor;
using YH.Modules.Identity.Domain;
using Mediator;
using Microsoft.AspNetCore.Identity;

namespace YH.Modules.Identity.Features.v1.TwoFactor.Enroll;

public sealed class EnrollTwoFactorCommandHandler
    : ICommandHandler<EnrollTwoFactorCommand, TwoFactorEnrollmentResponse>
{
    private const string IssuerName = "FullStackHero";

    private readonly UserManager<FshUser> _userManager;
    private readonly ICurrentUser _currentUser;

    public EnrollTwoFactorCommandHandler(UserManager<FshUser> userManager, ICurrentUser currentUser)
    {
        _userManager = userManager;
        _currentUser = currentUser;
    }

    public async ValueTask<TwoFactorEnrollmentResponse> Handle(
        EnrollTwoFactorCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!_currentUser.IsAuthenticated())
        {
            throw new UnauthorizedException();
        }

        var userId = _currentUser.GetUserId().ToString();
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException($"User {userId} not found.");

        // Always reset so calling enroll twice rotates the secret — prevents stale codes
        // from a prior incomplete enrollment from silently succeeding.
        await _userManager.ResetAuthenticatorKeyAsync(user);
        var sharedKey = await _userManager.GetAuthenticatorKeyAsync(user)
            ?? throw new CustomException("Failed to generate authenticator key.");

        var email = user.Email ?? user.UserName ?? user.Id;
        var authenticatorUri = string.Format(
            System.Globalization.CultureInfo.InvariantCulture,
            "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
            UrlEncoder.Default.Encode(IssuerName),
            UrlEncoder.Default.Encode(email),
            sharedKey);

        return new TwoFactorEnrollmentResponse(sharedKey, authenticatorUri);
    }
}
