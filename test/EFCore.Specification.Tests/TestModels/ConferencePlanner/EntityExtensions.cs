// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ConferencePlanner.ConferenceDTO;

namespace Microsoft.EntityFrameworkCore.TestModels.ConferencePlanner;

#nullable disable

public static class EntityExtensions
{
    public static SessionResponse MapSessionResponse(this Session session)
        => new()
        {
            Id = session.Id,
            Title = session.Title,
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            Speakers = session.SessionSpeakers?
                .Select(ss => new ConferenceDTO.Speaker { Id = ss.SpeakerId, Name = ss.Speaker.Name })
                .ToList(),
            TrackId = session.TrackId,
            Track = new ConferenceDTO.Track { Id = session?.TrackId ?? 0, Name = session.Track?.Name },
            Abstract = session.Abstract
        };

    public static SpeakerResponse MapSpeakerResponse(this Speaker speaker)
        => new()
        {
            Id = speaker.Id,
            Name = speaker.Name,
            Bio = speaker.Bio,
            WebSite = speaker.WebSite,
            Sessions = speaker.SessionSpeakers?
                .Select(
                    ss =>
                        new ConferenceDTO.Session { Id = ss.SessionId, Title = ss.Session.Title })
                .ToList()
        };

    public static AttendeeResponse MapAttendeeResponse(this Attendee attendee)
        => new()
        {
            Id = attendee.Id,
            FirstName = attendee.FirstName,
            LastName = attendee.LastName,
            UserName = attendee.UserName,
            EmailAddress = attendee.EmailAddress,
            Sessions = attendee.SessionsAttendees?
                .Select(
                    sa =>
                        new ConferenceDTO.Session
                        {
                            Id = sa.SessionId,
                            Title = sa.Session.Title,
                            StartTime = sa.Session.StartTime,
                            EndTime = sa.Session.EndTime
                        })
                .ToList()
        };
}
