// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestModels.ConferencePlanner;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using ConferenceDTO = Microsoft.EntityFrameworkCore.TestModels.ConferencePlanner.ConferenceDTO;

namespace Microsoft.EntityFrameworkCore
{
    public abstract partial class ConferencePlannerTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : ConferencePlannerTestBase<TFixture>.ConferencePlannerFixtureBase, new()
    {
        protected ConferencePlannerTestBase(TFixture fixture)
        {
            Fixture = fixture;
            fixture.ListLoggerFactory.Clear();
        }

        [ConditionalFact]
        public virtual async Task AttendeesController_Get()
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var controller = new AttendeesController(context);

                    var attendee = await controller.Get("RainbowDash");

                    Assert.Equal("Rainbow", attendee.FirstName);
                    Assert.Equal("Dash", attendee.LastName);
                    Assert.Equal("RainbowDash", attendee.UserName);
                    Assert.Equal("sonicrainboom@sample.com", attendee.EmailAddress);

                    var sessions = attendee.Sessions;

                    Assert.Equal(21, sessions.Count);
                    Assert.All(sessions, s => Assert.NotEmpty(s.Title));
                });
        }

        [ConditionalFact]
        public virtual async Task AttendeesController_GetSessions()
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var controller = new AttendeesController(context);

                    var sessions = await controller.GetSessions("Princess");

                    Assert.Equal(21, sessions.Count);
                    Assert.All(sessions, s => Assert.NotEmpty(s.Abstract));
                    Assert.All(sessions, s => Assert.NotEmpty(s.Speakers));
                    Assert.All(sessions, s => Assert.NotNull(s.Track));
                });
        }

        [ConditionalFact]
        public virtual async Task AttendeesController_Post_with_new_attendee()
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var controller = new AttendeesController(context);

                    var result = await controller.Post(
                        new ConferenceDTO.Attendee
                        {
                            EmailAddress = "discord@sample.com", FirstName = "", LastName = "Discord", UserName = "Discord!"
                        });

                    Assert.NotEqual(default, result.Id);
                    Assert.Equal("discord@sample.com", result.EmailAddress);
                    Assert.Equal("", result.FirstName);
                    Assert.Equal("Discord", result.LastName);
                    Assert.Equal("Discord!", result.UserName);
                    Assert.Null(result.Sessions);
                });
        }

        [ConditionalFact]
        public virtual async Task AttendeesController_Post_with_existing_attendee()
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var controller = new AttendeesController(context);

                    var result = await controller.Post(
                        new ConferenceDTO.Attendee
                        {
                            EmailAddress = "pinkie@sample.com", FirstName = "Pinkie", LastName = "Pie", UserName = "Pinks"
                        });

                    Assert.Null(result);
                });
        }

        [ConditionalFact]
        public virtual async Task AttendeesController_AddSession()
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var controller = new AttendeesController(context);

                    var pinky = context.Attendees.Single(a => a.UserName == "Pinks");

                    var pinkySessions = context.Sessions
                        .AsNoTracking()
                        .Where(s => s.SessionAttendees.Any(e => e.Attendee.UserName == "Pinks"))
                        .ToList();

                    var session = context.Sessions.AsNoTracking().Single(e => e.Title == "Hidden gems in .NET Core 3");

                    Assert.Equal(21, pinkySessions.Count);

                    var result = (ConferenceDTO.AttendeeResponse)await controller.AddSession("Pinks", session.Id);

                    Assert.Equal(22, result.Sessions.Count);
                    Assert.Contains(session.Id, result.Sessions.Select(s =>s .Id));

                    Assert.Equal(pinky.Id, result.Id);
                    Assert.Equal(pinky.UserName, result.UserName);
                    Assert.Equal(pinky.FirstName, result.FirstName);
                    Assert.Equal(pinky.LastName, result.LastName);
                    Assert.Equal(pinky.EmailAddress, result.EmailAddress);

                    var existingSessionIds = pinkySessions.Select(s => s.Id).ToList();
                    var newSessionIds = result.Sessions.Select(r => r.Id).ToHashSet();
                    Assert.All(existingSessionIds, i => newSessionIds.Contains(i));

                    Assert.Equal(
                        result.Sessions.Select(r => r.Id).OrderBy(i => i).ToList(),
                        context.Sessions
                            .AsNoTracking()
                            .Where(s => s.SessionAttendees.Any(e => e.Attendee.UserName == "Pinks"))
                            .Select(s => s.Id)
                            .OrderBy(i => i)
                            .ToList());
                });
        }

        [ConditionalFact]
        public virtual async Task AttendeesController_AddSession_bad_session()
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var controller = new AttendeesController(context);

                    var result = (string)await controller.AddSession("Pinks", -777);

                    Assert.Equal("No session", result);
                });
        }

        [ConditionalFact]
        public virtual async Task AttendeesController_AddSession_bad_attendee()
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var controller = new AttendeesController(context);

                    var session = context.Sessions.AsNoTracking().Single(e => e.Title == "Hidden gems in .NET Core 3");

                    var result = (string)await controller.AddSession("The Stig", session.Id);

                    Assert.Equal("No attendee", result);
                });
        }

        [ConditionalFact]
        public virtual async Task AttendeesController_RemoveSession()
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var controller = new AttendeesController(context);

                    var beforeRemove = context.Sessions
                        .AsNoTracking()
                        .Where(s => s.SessionAttendees.Any(e => e.Attendee.UserName == "Pinks"))
                        .OrderBy(e => e.Id)
                        .Select(e => e.Id)
                        .ToList();

                    Assert.Equal(21, beforeRemove.Count);

                    var sessionId = beforeRemove.First();
                    var result = await controller.RemoveSession("Pinks", sessionId);

                    Assert.Equal("Success", result);

                    var afterRemove = context.Sessions
                        .AsNoTracking()
                        .Where(s => s.SessionAttendees.Any(e => e.Attendee.UserName == "Pinks"))
                        .OrderBy(e => e.Id)
                        .Select(e => e.Id)
                        .ToList();

                    Assert.Equal(20, afterRemove.Count);
                    Assert.DoesNotContain(sessionId, afterRemove);
                    Assert.All(afterRemove, s => Assert.Contains(s, beforeRemove));
                });
        }

        [ConditionalFact]
        public virtual async Task AttendeesController_RemoveSession_bad_session()
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var controller = new AttendeesController(context);

                    var result = (string)await controller.RemoveSession("Pinks", -777);

                    Assert.Equal("No session", result);
                });
        }

        [ConditionalFact]
        public virtual async Task AttendeesController_RemoveSession_bad_attendee()
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var controller = new AttendeesController(context);

                    var session = context.Sessions.AsNoTracking().Single(e => e.Title == "Hidden gems in .NET Core 3");

                    var result = (string)await controller.RemoveSession("The Stig", session.Id);

                    Assert.Equal("No attendee", result);
                });
        }

        protected class AttendeesController
        {
            private readonly ApplicationDbContext _db;

            public AttendeesController(ApplicationDbContext db)
            {
                _db = db;
            }

            public async Task<ConferenceDTO.AttendeeResponse> Get(string username)
            {
                var attendee = await _db.Attendees
                    .Include(a => a.SessionsAttendees)
                    .ThenInclude(sa => sa.Session)
                    .SingleOrDefaultAsync(a => a.UserName == username);

                return attendee?.MapAttendeeResponse();
            }

            public async Task<List<ConferenceDTO.SessionResponse>> GetSessions(string username)
            {
                var sessions = await _db.Sessions.AsNoTracking()
                    .Include(s => s.Track)
                    .Include(s => s.SessionSpeakers)
                    .ThenInclude(ss => ss.Speaker)
                    .Where(s => s.SessionAttendees.Any(sa => sa.Attendee.UserName == username))
                    //.Select(m => m.MapSessionResponse())
                    .ToListAsync();

                // BUG: Working around EF Core 3.0 issue: https://github.com/aspnet/EntityFrameworkCore/issues/16318
                return sessions.Select(s => s.MapSessionResponse()).ToList();
            }

            public async Task<ConferenceDTO.AttendeeResponse> Post(ConferenceDTO.Attendee input)
            {
                var existingAttendee = await _db.Attendees
                    .Where(a => a.UserName == input.UserName)
                    .FirstOrDefaultAsync();

                if (existingAttendee != null)
                {
                    return null;
                }

                var attendee = new Attendee
                {
                    FirstName = input.FirstName, LastName = input.LastName, UserName = input.UserName, EmailAddress = input.EmailAddress
                };

                _db.Attendees.Add(attendee);
                await _db.SaveChangesAsync();

                return attendee.MapAttendeeResponse();
            }

            public async Task<object> AddSession(string username, int sessionId)
            {
                var attendee = await _db.Attendees
                    .Include(a => a.SessionsAttendees)
                    .ThenInclude(sa => sa.Session)
                    .SingleOrDefaultAsync(a => a.UserName == username);

                if (attendee == null)
                {
                    return "No attendee";
                }

                var session = await _db.Sessions.FindAsync(sessionId);

                if (session == null)
                {
                    return "No session";
                }

                attendee.SessionsAttendees.Add(
                    new SessionAttendee
                    {
                        AttendeeId = attendee.Id, SessionId = sessionId
                    });

                await _db.SaveChangesAsync();

                return attendee.MapAttendeeResponse();
            }

            public async Task<string> RemoveSession(string username, int sessionId)
            {
                var attendee = await _db.Attendees
                    .Include(a => a.SessionsAttendees)
                    .SingleOrDefaultAsync(a => a.UserName == username);

                if (attendee == null)
                {
                    return "No attendee";
                }

                var session = await _db.Sessions.FindAsync(sessionId);

                if (session == null)
                {
                    return "No session";
                }

                var sessionAttendee = attendee.SessionsAttendees.FirstOrDefault(sa => sa.SessionId == sessionId);
                attendee.SessionsAttendees.Remove(sessionAttendee);

                await _db.SaveChangesAsync();

                return "Success";
            }
        }

        [ConditionalTheory]
        [InlineData("ran", 5, 1)]
        [InlineData("Scott", 1, 0)]
        [InlineData("C#", 3, 3)]
        public virtual async Task SearchController_Search(
            string searchTerm, int totalCount, int sessionCount)
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var controller = new SearchController(context);

                    var results = await controller.Search(
                        new ConferenceDTO.SearchTerm
                        {
                            Query = searchTerm
                        });

                    Assert.Equal(totalCount, results.Count);

                    var sessions = results.Where(r => r.Type == ConferenceDTO.SearchResultType.Session).Select(r => r.Session).ToList();
                    var speakers = results.Where(r => r.Type == ConferenceDTO.SearchResultType.Speaker).Select(r => r.Speaker).ToList();

                    Assert.Equal(sessionCount, sessions.Count);
                    Assert.Equal(totalCount - sessionCount, speakers.Count);

                    Assert.All(sessions, s => Assert.NotEqual(default, s.Id));
                    Assert.All(sessions, s => Assert.NotEmpty(s.Speakers));
                    Assert.All(sessions, s => Assert.NotNull(s.Track));

                    Assert.All(speakers, s => Assert.NotEqual(default, s.Id));
                    Assert.All(speakers, s => Assert.NotEmpty(s.Sessions));
                });
        }

        protected class SearchController
        {
            private readonly ApplicationDbContext _db;

            public SearchController(ApplicationDbContext db)
            {
                _db = db;
            }

            public async Task<List<ConferenceDTO.SearchResult>> Search(ConferenceDTO.SearchTerm term)
            {
                var query = term.Query;
                var sessionResults = await _db.Sessions
                    .Include(s => s.Track)
                    .Include(s => s.SessionSpeakers)
                    .ThenInclude(ss => ss.Speaker)
                    .Where(
                        s =>
                            s.Title.Contains(query) ||
                            s.Track.Name.Contains(query)
                    )
                    .ToListAsync();

                var speakerResults = await _db.Speakers
                    .Include(s => s.SessionSpeakers)
                    .ThenInclude(ss => ss.Session)
                    .Where(
                        s =>
                            s.Name.Contains(query) ||
                            s.Bio.Contains(query) ||
                            s.WebSite.Contains(query)
                    )
                    .ToListAsync();

                var results = sessionResults.Select(
                        session => new ConferenceDTO.SearchResult
                        {
                            Type = ConferenceDTO.SearchResultType.Session, Session = session.MapSessionResponse()
                        })
                    .Concat(
                        speakerResults.Select(
                            speaker => new ConferenceDTO.SearchResult
                            {
                                Type = ConferenceDTO.SearchResultType.Speaker, Speaker = speaker.MapSpeakerResponse()
                            }));

                return results.ToList();
            }
        }

        [ConditionalFact]
        public virtual async Task SessionsController_Get()
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var controller = new SessionsController(context);

                    var results = await controller.Get();

                    Assert.Equal(57, results.Count);

                    Assert.All(results, s => Assert.NotEqual(default, s.Id));
                    Assert.All(results, s => Assert.NotEmpty(s.Speakers));
                    Assert.All(results, s => Assert.NotNull(s.Track));
                });
        }

        [ConditionalFact]
        public virtual async Task SessionsController_Get_with_ID()
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var session = context.Sessions.AsNoTracking().Single(e => e.Title.StartsWith("C# and Rust: combining "));

                    var controller = new SessionsController(context);

                    var result = await controller.Get(session.Id);

                    Assert.Equal(session.Id, result.Id);
                    Assert.NotEmpty(result.Speakers);
                    Assert.NotNull(result.Track);
                });
        }

        [ConditionalFact]
        public virtual async Task SessionsController_Get_with_bad_ID()
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var controller = new SessionsController(context);

                    var result = await controller.Get(-777);

                    Assert.Null(result);
                });
        }

        [ConditionalFact]
        public virtual async Task SessionsController_Post()
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var track = context.Tracks.AsNoTracking().First();

                    var controller = new SessionsController(context);

                    var result = await controller.Post(
                        new ConferenceDTO.Session
                        {
                            Abstract = "Pandas eat bamboo all dat.",
                            Title = "Pandas!",
                            StartTime = DateTimeOffset.Now,
                            EndTime = DateTimeOffset.Now.AddHours(1),
                            TrackId = track.Id
                        });

                    var newSession = context.Sessions.AsNoTracking().Single(e => e.Title == "Pandas!");

                    Assert.Equal(newSession.Id, result.Id);
                    Assert.Null(result.Speakers);
                    Assert.NotNull(result.Track);
                    Assert.Equal(track.Id, result.Track.Id);
                });
        }

        [ConditionalFact]
        public virtual async Task SessionsController_Put()
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var session = context.Sessions.AsNoTracking().Single(e => e.Title.StartsWith("C# and Rust: combining "));

                    var controller = new SessionsController(context);

                    var result = await controller.Put(
                        session.Id,
                        new ConferenceDTO.Session
                        {
                            Id = session.Id,
                            Abstract = session.Abstract,
                            Title = session.Title.Replace("C#", "F#"),
                            StartTime = session.StartTime,
                            EndTime = session.EndTime,
                            TrackId = session.TrackId
                        });

                    Assert.Equal("Success", result);

                    var updatedSession = context.Sessions.AsNoTracking().Single(e => e.Id == session.Id);

                    Assert.Equal(session.Id, updatedSession.Id);
                    Assert.StartsWith("F# and Rust: combining ", updatedSession.Title);
                });
        }

        [ConditionalFact]
        public virtual async Task SessionsController_Put_with_bad_ID()
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var controller = new SessionsController(context);

                    var result = await controller.Put(-777, new ConferenceDTO.Session());

                    Assert.Equal("Not found", result);
                });
        }

        [ConditionalFact]
        public virtual async Task SessionsController_Delete()
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var session = context.Sessions.AsNoTracking().Single(e => e.Title.StartsWith("C# and Rust: combining "));

                    var controller = new SessionsController(context);

                    var result = await controller.Delete(session.Id);

                    Assert.Equal(session.Id, result.Id);
                    Assert.Null(result.Speakers);
                    Assert.NotNull(result.Track);

                    Assert.Null(context.Sessions.AsNoTracking().SingleOrDefault(e => e.Id == session.Id));
                });
        }

        [ConditionalFact]
        public virtual async Task SessionsController_Delete_with_bad_ID()
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var controller = new SessionsController(context);

                    var result = await controller.Delete(-777);

                    Assert.Null(result);
                });
        }

        protected class SessionsController
        {
            private readonly ApplicationDbContext _db;

            public SessionsController(ApplicationDbContext db)
            {
                _db = db;
            }

            public async Task<List<ConferenceDTO.SessionResponse>> Get()
            {
                var sessions = await _db.Sessions.AsNoTracking()
                    .Include(s => s.Track)
                    .Include(s => s.SessionSpeakers)
                    .ThenInclude(ss => ss.Speaker)
                    //.Select(m => m.MapSessionResponse())
                    .ToListAsync();

                // BUG: Working around EF Core 3.0 issue: https://github.com/aspnet/EntityFrameworkCore/issues/16318
                return sessions.Select(s => s.MapSessionResponse())
                    .ToList();
            }

            public async Task<ConferenceDTO.SessionResponse> Get(int id)
            {
                var session = await _db.Sessions.AsNoTracking()
                    .Include(s => s.Track)
                    .Include(s => s.SessionSpeakers)
                    .ThenInclude(ss => ss.Speaker)
                    .SingleOrDefaultAsync(s => s.Id == id);

                return session?.MapSessionResponse();
            }

            public async Task<ConferenceDTO.SessionResponse> Post(ConferenceDTO.Session input)
            {
                var session = new Session
                {
                    Title = input.Title,
                    StartTime = input.StartTime,
                    EndTime = input.EndTime,
                    Abstract = input.Abstract,
                    TrackId = input.TrackId
                };

                _db.Sessions.Add(session);
                await _db.SaveChangesAsync();

                return session.MapSessionResponse();
            }

            public async Task<string> Put(int id, ConferenceDTO.Session input)
            {
                var session = await _db.Sessions.FindAsync(id);

                if (session == null)
                {
                    return "Not found";
                }

                session.Id = input.Id;
                session.Title = input.Title;
                session.Abstract = input.Abstract;
                session.StartTime = input.StartTime;
                session.EndTime = input.EndTime;
                session.TrackId = input.TrackId;

                await _db.SaveChangesAsync();

                return "Success";
            }

            public async Task<ConferenceDTO.SessionResponse> Delete(int id)
            {
                var session = await _db.Sessions.FindAsync(id);

                if (session == null)
                {
                    return null;
                }

                _db.Sessions.Remove(session);
                await _db.SaveChangesAsync();

                return session.MapSessionResponse();
            }
        }

        [ConditionalFact]
        public virtual async Task SpeakersController_GetSpeakers()
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var controller = new SpeakersController(context);

                    var results = await controller.GetSpeakers();

                    Assert.Equal(70, results.Count);

                    Assert.All(results, s => Assert.NotEqual(default, s.Id));
                    Assert.All(results, s => Assert.NotEmpty(s.Sessions));
                });
        }

        [ConditionalFact]
        public virtual async Task SpeakersController_GetSpeaker_with_ID()
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var speaker = context.Speakers.AsNoTracking().Single(e => e.Name == "Julie Lerman");

                    var controller = new SpeakersController(context);

                    var result = await controller.GetSpeaker(speaker.Id);

                    Assert.Equal(speaker.Id, result.Id);
                    Assert.NotEmpty(result.Sessions);
                });
        }

        [ConditionalFact]
        public virtual async Task SpeakersController_GetSpeaker_with_bad_ID()
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var controller = new SpeakersController(context);

                    var result = await controller.GetSpeaker(-777);

                    Assert.Null(result);
                });
        }

        protected class SpeakersController
        {
            private readonly ApplicationDbContext _db;

            public SpeakersController(ApplicationDbContext db)
            {
                _db = db;
            }

            public async Task<List<ConferenceDTO.SpeakerResponse>> GetSpeakers()
            {
                var speakers = await _db.Speakers.AsNoTracking()
                    .Include(s => s.SessionSpeakers)
                    .ThenInclude(ss => ss.Session)
                    //.Select(s => s.MapSpeakerResponse())
                    .ToListAsync();

                // BUG: Working around EF Core 3.0 issue: https://github.com/aspnet/EntityFrameworkCore/issues/16318
                return speakers.Select(s => s.MapSpeakerResponse()).ToList();
            }

            public async Task<ConferenceDTO.SpeakerResponse> GetSpeaker(int id)
            {
                var speaker = await _db.Speakers.AsNoTracking()
                    .Include(s => s.SessionSpeakers)
                    .ThenInclude(ss => ss.Session)
                    .SingleOrDefaultAsync(s => s.Id == id);

                return speaker?.MapSpeakerResponse();
            }
        }

        protected TFixture Fixture { get; }

        protected ApplicationDbContext CreateContext() => Fixture.CreateContext();

        protected virtual Task ExecuteWithStrategyInTransactionAsync(
            Func<ApplicationDbContext, Task> testOperation,
            Func<ApplicationDbContext, Task> nestedTestOperation1 = null,
            Func<ApplicationDbContext, Task> nestedTestOperation2 = null,
            Func<ApplicationDbContext, Task> nestedTestOperation3 = null)
            => TestHelpers.ExecuteWithStrategyInTransactionAsync(
                CreateContext,
                UseTransaction,
                testOperation,
                nestedTestOperation1,
                nestedTestOperation2,
                nestedTestOperation3);

        protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        {
        }

        public abstract class ConferencePlannerFixtureBase : SharedStoreFixtureBase<ApplicationDbContext>
        {
            protected override string StoreName { get; } = "ConferencePlanner";

            protected override bool UsePooling => false;

            protected override void Seed(ApplicationDbContext context)
            {
                var attendees1 = new List<Attendee>
                {
                    new Attendee
                    {
                        EmailAddress = "sonicrainboom@sample.com", FirstName = "Rainbow", LastName = "Dash", UserName = "RainbowDash"
                    },
                    new Attendee
                    {
                        EmailAddress = "solovely@sample.com", FirstName = "Flutter", LastName = "Shy", UserName = "Fluttershy"
                    }
                };

                var attendees2 = new List<Attendee>
                {
                    new Attendee
                    {
                        EmailAddress = "applesforever@sample.com", FirstName = "Apple", LastName = "Jack", UserName = "Applejack"
                    },
                    new Attendee
                    {
                        EmailAddress = "precious@sample.com", FirstName = "Rarity", LastName = "", UserName = "Rarity"
                    }
                };

                var attendees3 = new List<Attendee>
                {
                    new Attendee
                    {
                        EmailAddress = "princess@sample.com", FirstName = "Twilight", LastName = "Sparkle", UserName = "Princess"
                    },
                    new Attendee
                    {
                        EmailAddress = "pinkie@sample.com", FirstName = "Pinkie", LastName = "Pie", UserName = "Pinks"
                    }
                };

                using var document = JsonDocument.Parse(ConferenceData);

                var tracks = new Dictionary<int, Track>();
                var speakers = new Dictionary<Guid, Speaker>();

                var root = document.RootElement;
                foreach (var dayJson in root.EnumerateArray())
                {
                    foreach (var roomJson in dayJson.GetProperty("rooms").EnumerateArray())
                    {
                        var roomId = roomJson.GetProperty("id").GetInt32();
                        if (!tracks.TryGetValue(roomId, out var track))
                        {
                            track = new Track
                            {
                                Name = roomJson.GetProperty("name").GetString(), Sessions = new List<Session>()
                            };

                            tracks[roomId] = track;
                        }

                        foreach (var sessionJson in roomJson.GetProperty("sessions").EnumerateArray())
                        {
                            var sessionSpeakers = new List<Speaker>();
                            foreach (var speakerJson in sessionJson.GetProperty("speakers").EnumerateArray())
                            {
                                var speakerId = speakerJson.GetProperty("id").GetGuid();
                                if (!speakers.TryGetValue(speakerId, out var speaker))
                                {
                                    speaker = new Speaker
                                    {
                                        Name = speakerJson.GetProperty("name").GetString()
                                    };

                                    speakers[speakerId] = speaker;
                                }

                                sessionSpeakers.Add(speaker);
                            }

                            var session = new Session
                            {
                                Title = sessionJson.GetProperty("title").GetString(),
                                Abstract = sessionJson.GetProperty("description").GetString(),
                                StartTime = sessionJson.GetProperty("startsAt").GetDateTime(),
                                EndTime = sessionJson.GetProperty("endsAt").GetDateTime(),
                            };

                            session.SessionSpeakers = sessionSpeakers.Select(
                                s => new SessionSpeaker
                                {
                                    Session = session, Speaker = s
                                }).ToList();

                            var trackName = track.Name;
                            var attendees = trackName.Contains("1") ? attendees1
                                : trackName.Contains("2") ? attendees2
                                : trackName.Contains("3") ? attendees3
                                : attendees1.Concat(attendees2).Concat(attendees3).ToList();

                            session.SessionAttendees = attendees.Select(
                                a => new SessionAttendee
                                {
                                    Session = session, Attendee = a
                                }).ToList();

                            track.Sessions.Add(session);
                        }
                    }
                }

                context.AddRange(tracks.Values);
                context.SaveChanges();
            }
        }
    }
}
