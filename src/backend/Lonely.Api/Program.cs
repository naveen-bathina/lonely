using Lonely.Api.Auth;
using Lonely.Api.Discovery;
using Lonely.Api.Meetup;
using Lonely.Api.Messaging;
using Lonely.Api.Moderation;
using Lonely.Api.Profiles;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<IProfileService, ProfileService>();
builder.Services.AddSingleton<IDiscoveryService, DiscoveryService>();
builder.Services.AddSingleton<IMessageService, MessageService>();
builder.Services.AddSingleton<IModerationService, ModerationService>();
builder.Services.AddSingleton<IMeetupService, MeetupService>();

var app = builder.Build();
app.UseHttpsRedirection();

app.MapPost("/api/v1/auth/register", async (RegisterRequest request, IAuthService authService) =>
{
    try
    {
        var result = await authService.Register(request);
        return Results.Created($"/api/v1/users/{result.UserId}", new { userId = result.UserId });
    }
    catch (UnderageException)
    {
        return Results.BadRequest(new { error = "Users must be 18 or older." });
    }
    catch (GdprConsentRequiredException)
    {
        return Results.BadRequest(new { error = "GDPR consent is required." });
    }
});

app.MapPost("/api/v1/auth/confirm-otp", async (ConfirmOtpRequest request, IAuthService authService) =>
{
    try
    {
        await authService.ConfirmOtp(request);
        return Results.Ok(new { message = "Account confirmed." });
    }
    catch (InvalidOtpException)
    {
        return Results.BadRequest(new { error = "Invalid or expired OTP." });
    }
});

app.MapPost("/api/v1/auth/refresh", async (RefreshRequest request, IAuthService authService) =>
{
    try
    {
        var accessToken = await authService.Refresh(request);
        return Results.Ok(new { accessToken });
    }
    catch
    {
        return Results.Unauthorized();
    }
});

app.MapPost("/api/v1/auth/login", async (LoginRequest request, IAuthService authService) =>
{
    try
    {
        var result = await authService.Login(request);
        return Results.Ok(new { accessToken = result.AccessToken, refreshToken = result.RefreshToken });
    }
    catch (InvalidCredentialsException)
    {
        return Results.Unauthorized();
    }
});

app.MapPut("/api/v1/profiles/{userId}", async (string userId, UpsertProfileRequest request, IProfileService profileService) =>
{
    var profile = await profileService.Upsert(userId, request);
    return Results.Ok(new { userId = profile.UserId, isComplete = profile.IsComplete, badges = profile.Badges });
});

app.MapGet("/api/v1/profiles/{userId}", async (string userId, IProfileService profileService) =>
{
    var profile = await profileService.Get(userId);
    return profile is null ? Results.NotFound() : Results.Ok(profile);
});

app.MapPatch("/api/v1/profiles/{userId}/photos/{photoId}/status",
    async (string userId, string photoId, PhotoStatusRequest request, IProfileService profileService) =>
    {
        var photo = await profileService.UpdatePhotoStatus(userId, photoId, request.Status);
        return Results.Ok(new { photo.PhotoId, photo.Status });
    });

app.MapGet("/api/v1/profiles/{userId}/photos", async (string userId, bool? visible, IProfileService profileService) =>
{
    var photos = await profileService.GetPhotos(userId, visible ?? false);
    return Results.Ok(photos.Select(p => new { photoId = p.PhotoId, status = p.Status }));
});

app.MapGet("/api/v1/discover", async (IProfileService profileService) =>
{
    var feed = await profileService.GetDiscoverFeed();
    return Results.Ok(feed);
});

app.MapGet("/api/v1/discover/recommendations", async (string userId, IDiscoveryService discoveryService) =>
{
    var recommendations = await discoveryService.GetRecommendations(userId);
    return Results.Ok(recommendations);
});

app.MapPost("/api/v1/discover/{userId}/preferences",
    async (string userId, SetPreferencesRequest request, IDiscoveryService discoveryService) =>
    {
        await discoveryService.SetPreferences(userId, request);
        return Results.Ok();
    });

app.MapPost("/api/v1/discover/{userId}/like/{targetId}",
    async (string userId, string targetId, IDiscoveryService discoveryService) =>
    {
        var match = await discoveryService.Like(userId, targetId);
        return match is null
            ? Results.NoContent()
            : Results.Ok(new { matchId = match.MatchId, user1Id = match.User1Id, user2Id = match.User2Id });
    });

app.MapPost("/api/v1/discover/{userId}/pass/{targetId}",
    async (string userId, string targetId, IDiscoveryService discoveryService) =>
    {
        await discoveryService.Pass(userId, targetId);
        return Results.NoContent();
    });

app.MapPost("/api/v1/users/{userId}/block/{targetId}",
    async (string userId, string targetId, IDiscoveryService discoveryService) =>
    {
        await discoveryService.Block(userId, targetId);
        return Results.NoContent();
    });

app.MapPost("/api/v1/profiles/{userId}/questionnaire",
    async (string userId, QuestionnaireRequest request, IDiscoveryService discoveryService) =>
    {
        await discoveryService.SaveQuestionnaire(userId, request);
        return Results.Ok();
    });

app.MapPost("/api/v1/profiles/{userId}/photos", async (string userId, PhotoUploadRequest request, IProfileService profileService) =>
{
    var photo = await profileService.UploadPhoto(userId, request);
    return Results.Accepted($"/api/v1/profiles/{userId}/photos/{photo.PhotoId}",
        new { photoId = photo.PhotoId, status = photo.Status });
});

app.MapPost("/api/v1/messages/{matchId}",
    async (string matchId, SendMessageRequest request, IMessageService messageService) =>
    {
        try
        {
            var msg = await messageService.Send(matchId, request);
            return Results.Created($"/api/v1/messages/{matchId}/{msg.MessageId}",
                new { messageId = msg.MessageId, matchId = msg.MatchId, senderId = msg.SenderId, text = msg.Text, sentAt = msg.SentAt, readAt = msg.ReadAt });
        }
        catch (MatchNotFoundException)
        {
            return Results.NotFound(new { error = "Match not found." });
        }
    });

app.MapGet("/api/v1/messages/{matchId}",
    async (string matchId, string? viewerId, IMessageService messageService) =>
    {
        try
        {
            var messages = await messageService.GetThread(matchId, viewerId);
            return Results.Ok(messages.Select(m => new
            {
                messageId = m.MessageId, senderId = m.SenderId, text = m.Text,
                sentAt = m.SentAt, readAt = m.ReadAt
            }));
        }
        catch (MatchNotFoundException)
        {
            return Results.NotFound(new { error = "Match not found." });
        }
    });

app.MapMethods("/api/v1/messages/{matchId}/settings", new[] { "PATCH" },
    async (string matchId, ReadReceiptsSettingsRequest request, IMessageService messageService) =>
    {
        await messageService.SetReadReceipts(matchId, request);
        return Results.Ok();
    });

app.MapGet("/api/v1/messages/{matchId}/ghosting-status",
    async (string matchId, string userId, int thresholdHours, IMessageService messageService) =>
    {
        try
        {
            var status = await messageService.GetGhostingStatus(matchId, userId, thresholdHours);
            return Results.Ok(new { isIdle = status.IsIdle, idleHours = status.IdleHours });
        }
        catch (MatchNotFoundException)
        {
            return Results.NotFound(new { error = "Match not found." });
        }
    });

app.MapPost("/api/v1/reports",
    async (SubmitReportRequest request, IModerationService moderationService) =>
    {
        var report = await moderationService.SubmitReport(request);
        return Results.Created($"/api/v1/reports/{report.ReportId}",
            new { reportId = report.ReportId, reporterId = report.ReporterId,
                  targetId = report.TargetId, reason = report.Reason, status = report.Status });
    });

app.MapGet("/api/v1/reports",
    async (string userId, IModerationService moderationService) =>
    {
        var history = await moderationService.GetHistory(userId);
        return Results.Ok(history.Select(r => new
        {
            reportId = r.ReportId, reporterId = r.ReporterId,
            targetId = r.TargetId, reason = r.Reason, status = r.Status
        }));
    });

app.MapGet("/api/v1/admin/moderation-queue",
    async (IModerationService moderationService) =>
    {
        var queue = await moderationService.GetQueue();
        return Results.Ok(queue.Select(r => new
        {
            reportId = r.ReportId, reporterId = r.ReporterId,
            targetId = r.TargetId, reason = r.Reason, status = r.Status
        }));
    });

app.MapMethods("/api/v1/admin/moderation-queue/{reportId}/approve", new[] { "PATCH" },
    async (string reportId, IModerationService moderationService) =>
    {
        var report = await moderationService.Resolve(reportId, "approved");
        return Results.Ok(new { reportId = report.ReportId, status = report.Status });
    });

app.MapMethods("/api/v1/admin/moderation-queue/{reportId}/remove", new[] { "PATCH" },
    async (string reportId, IModerationService moderationService) =>
    {
        var report = await moderationService.Resolve(reportId, "removed");
        return Results.Ok(new { reportId = report.ReportId, status = report.Status });
    });

// ── Phase 6: Meetup Proposal ─────────────────────────────────────────────────

app.MapPost("/api/v1/meetups", async (ProposeMeetupRequest request, IMeetupService meetupService) =>
{
    try
    {
        var proposal = await meetupService.Propose(request);
        return Results.Created($"/api/v1/meetups/{proposal.ProposalId}", new
        {
            proposalId = proposal.ProposalId,
            proposerId = proposal.ProposerId,
            matchId = proposal.MatchId,
            state = proposal.State,
            expiresAt = proposal.ExpiresAt
        });
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
});

app.MapGet("/api/v1/meetups/{proposalId}", async (string proposalId, IMeetupService meetupService) =>
{
    try
    {
        var proposal = await meetupService.GetProposal(proposalId);
        return Results.Ok(new
        {
            proposalId = proposal.ProposalId,
            proposerId = proposal.ProposerId,
            matchId = proposal.MatchId,
            state = proposal.State,
            expiresAt = proposal.ExpiresAt
        });
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
});

app.MapMethods("/api/v1/meetups/{proposalId}/accept", new[] { "PATCH" },
    async (string proposalId, IMeetupService meetupService) =>
    {
        try
        {
            var proposal = await meetupService.Accept(proposalId);
            return Results.Ok(new { proposalId = proposal.ProposalId, state = proposal.State });
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound();
        }
    });

app.MapMethods("/api/v1/meetups/{proposalId}/decline", new[] { "PATCH" },
    async (string proposalId, IMeetupService meetupService) =>
    {
        try
        {
            var proposal = await meetupService.Decline(proposalId);
            return Results.Ok(new { proposalId = proposal.ProposalId, state = proposal.State });
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound();
        }
    });

app.MapMethods("/api/v1/meetups/{proposalId}/expire", new[] { "PATCH" },
    async (string proposalId, IMeetupService meetupService) =>
    {
        try
        {
            var proposal = await meetupService.Expire(proposalId);
            return Results.Ok(new { proposalId = proposal.ProposalId, state = proposal.State });
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound();
        }
    });

app.Run();

public partial class Program { }
