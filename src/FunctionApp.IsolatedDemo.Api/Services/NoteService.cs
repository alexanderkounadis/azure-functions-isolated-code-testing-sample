﻿using FunctionApp.IsolatedDemo.Api.Dtos;
using FunctionApp.IsolatedDemo.Api.Entities;
using FunctionApp.IsolatedDemo.Api.Interfaces;
using FunctionApp.IsolatedDemo.Api.Options;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FunctionApp.IsolatedDemo.Api.Services
{
    public class NoteService : INoteService
    {
        private readonly INoteRepository _noteRepository;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly INotificationService _notificationService;
        private readonly ILogger<NoteService> _logger;

        public NoteService(
            INoteRepository noteRepository, 
            IDateTimeProvider dateTimeProvider,
            INotificationService notificationService,
            ILogger<NoteService> logger)
        {
            _noteRepository = noteRepository;
            _dateTimeProvider = dateTimeProvider;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<NoteDto> CreateNoteAsync(CreateNoteOptions createNoteOptions, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(createNoteOptions.Title) || string.IsNullOrEmpty(createNoteOptions.Body))
            {
                _logger.LogError("Title or body of note cannot be empty. Returning null by default.");

                return null;
            }

            var newNote = await _noteRepository.CreateAsync(new Note
            {
                Id = Guid.NewGuid().ToString(),
                Title = createNoteOptions.Title,
                Body = createNoteOptions.Body,
                CreatedAt = _dateTimeProvider.UtcNow,
                LastUpdatedOn = _dateTimeProvider.UtcNow
            }, ct);

            await _notificationService.SendNoteCreatedEventAsync(createNoteOptions, ct);

            return new NoteDto
            {
                Id = newNote.Id,
                Title = newNote.Title,
                LastUpdatedOn = newNote.LastUpdatedOn,
                Body = newNote.Body
            };
        }
    }
}
