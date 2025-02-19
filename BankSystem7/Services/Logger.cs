﻿using BankSystem7.Models;
using BankSystem7.Services.Interfaces;
using BankSystem7.Services.Repositories;

namespace BankSystem7.Services;

public sealed class Logger<TUser, TCard, TBankAccount, TBank, TCredit> : ILogger
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    private readonly LoggerRepository _loggerRepository;
    private readonly ILogger _logger;

    public Logger()
    {
        LoggerOptions = new LoggerOptions
        {
            IsEnabled = true,
            OperationServiceOptions = new OperationServiceOptions
            {
                CollectionName = "Reports",
            },
        };
    }

    public Logger(ILogger logger)
    {
        _logger = logger;
        IsReused = true;
        _loggerRepository = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ServiceConfiguration?.LoggerRepository ?? new LoggerRepository(logger.LoggerOptions);
    }

    public Logger(LoggerOptions options)
    {
        LoggerOptions = options;
        _loggerRepository = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ServiceConfiguration?.LoggerRepository ?? new LoggerRepository(options);
    }

    public Logger(LoggerRepository loggerRepository, LoggerOptions options)
    {
        LoggerOptions = options;
        _loggerRepository = loggerRepository;
    }

    public bool IsReused { get; set; }
    public LoggerOptions? LoggerOptions { get; set; }

    public ExceptionModel Log(Report report)
    {
        if (LoggerOptions?.IsEnabled == false && _logger?.LoggerOptions?.IsEnabled == false)
            return ExceptionModel.OperationRestricted;

        if (IsReused)
            return _logger.Log(report);

        if (_loggerRepository.Exist(x => x.Id == report.Id))
            _loggerRepository.Update(report);

        _loggerRepository.Create(report);

        return ExceptionModel.Ok;
    }

    public ExceptionModel Log(IEnumerable<Report?>? reports)
    {
        if (reports is null || reports.Any(x => x is null))
            return ExceptionModel.EntityIsNull;

        foreach (var report in reports)
        {
            var log = Log(report);
            if (log != ExceptionModel.Ok)
                return ExceptionModel.OperationFailed;
        }
        return ExceptionModel.Ok;
    }
}

public sealed class LoggerOptions
{
    public OperationServiceOptions? OperationServiceOptions { get; set; }
    public bool IsEnabled { get; set; }
}