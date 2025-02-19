﻿using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Configuration;
using BankSystem7.Services.Interfaces.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BankSystem7.Services.Repositories;

public sealed class UserRepository<TUser, TCard, TBankAccount, TBank, TCredit> : IUserRepository<TUser>
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    private readonly IBankAccountRepository<TUser, TBankAccount> _bankAccountRepository;
    private readonly ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit> _applicationContext;
    private readonly IBankRepository<TUser, TBank> _bankRepository;
    private readonly ICardRepository<TCard> _cardRepository;
    private bool _disposed;

    public UserRepository()
    {
        _bankAccountRepository = new BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit>(ServicesSettings.Connection);
        _bankRepository = new BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>(ServicesSettings.Connection);
        _cardRepository = new CardRepository<TUser, TCard, TBankAccount, TBank, TCredit>(_bankAccountRepository);
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>
                              (ServicesSettings.Connection);
    }

    public UserRepository(string connection)
    {
        _bankAccountRepository = new BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit>(connection);
        _bankRepository = new BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>(connection);
        _cardRepository = new CardRepository<TUser, TCard, TBankAccount, TBank, TCredit>(_bankAccountRepository);
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>
                              (ServicesSettings.Connection);
    }

    public UserRepository(IBankAccountRepository<TUser, TBankAccount> repository)
    {
        _bankAccountRepository = repository;

        _bankRepository = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ServiceConfiguration?.BankRepository
            ?? new BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>(ServicesSettings.Connection);

        _cardRepository = (BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ServiceConfiguration?.CardRepository
            ?? new CardRepository<TUser, TCard, TBankAccount, TBank, TCredit>(_bankAccountRepository));

        _applicationContext = new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>
            (ServicesSettings.Connection);
    }

    public IQueryable<TUser> All =>
        _applicationContext.Users
            .Include(x => x.Card.BankAccount.Bank)
            .AsNoTracking();

    public ExceptionModel Create(TUser item)
    {
        if (item?.Card?.BankAccount?.Bank is null || item.Equals(User.Default))
            return ExceptionModel.OperationFailed;

        if (Exist(x => x.Id.Equals(item.Id) || x.Name.Equals(item.Name) && x.Email.Equals(item.Email)))
            return ExceptionModel.OperationRestricted;

        _applicationContext.UpdateTracker(item.Card.BankAccount.Bank,  EntityState.Modified, delegate
        {
            item.Card.BankAccount.Bank.AccountAmount += _bankRepository.CalculateBankAccountAmount(item.Card.Amount);
        }, _applicationContext);

        _applicationContext.Users.Add(item);
        
        _applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> CreateAsync(TUser item)
    {
        if (item?.Card?.BankAccount?.Bank is null || item.Equals(User.Default))
            return ExceptionModel.OperationFailed;

        if (Exist(x => x.Id.Equals(item.Id) || x.Name.Equals(item.Name) && x.Email.Equals(item.Email)))
            return ExceptionModel.OperationRestricted;

        _applicationContext.UpdateTracker(item.Card.BankAccount.Bank, EntityState.Modified, delegate
        {
            item.Card.BankAccount.Bank.AccountAmount += _bankRepository.CalculateBankAccountAmount(item.Card.Amount);
        }, _applicationContext);

        _applicationContext.Users.Add(item);

        await _applicationContext.SaveChangesAsync();
        return ExceptionModel.Ok;
    }

    public ExceptionModel Update(TUser item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.UpdateRange(item, item.Card.BankAccount);
        _applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> UpdateAsync(TUser item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.UpdateRange(item, item.Card.BankAccount);
        await _applicationContext.SaveChangesAsync();
        return ExceptionModel.Ok;
    }

    public ExceptionModel Delete(TUser item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.EntityNotExist;

        _applicationContext.RemoveRange(item, (TBankAccount)item.Card.BankAccount);
        _applicationContext.UpdateTracker(item.Card.BankAccount.Bank, EntityState.Modified, delegate
        {
            item.Card.BankAccount.Bank.AccountAmount -=
                _bankRepository.CalculateBankAccountAmount(item.Card.Amount);
        }, _applicationContext);

        _applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> DeleteAsync(TUser item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.EntityNotExist;

        _applicationContext.RemoveRange(item, (TBankAccount)item.Card.BankAccount);
        _applicationContext.UpdateTracker(item.Card.BankAccount.Bank, EntityState.Modified, delegate
        {
            item.Card.BankAccount.Bank.AccountAmount -=
                _bankRepository.CalculateBankAccountAmount(item.Card.Amount);
        }, _applicationContext);

        await _applicationContext.SaveChangesAsync();

        return ExceptionModel.Ok;
    }

    public bool Exist(Expression<Func<TUser, bool>> predicate)
    {
        return All.Any(predicate);
    }

    public async Task<bool> ExistAsync(Expression<Func<TUser, bool>> predicate)
    {
        return await All.AnyAsync(predicate);
    }

    public bool FitsConditions(TUser? item)
    {
        return item?.Card?.BankAccount?.Bank is not null && Exist(x => x.Id == item.Id);
    }

    public async Task<bool> FitsConditionsAsync(TUser? item)
    {
        return item?.Card?.BankAccount?.Bank is not null && await ExistAsync(x => x.Id == item.Id);
    }

    public TUser Get(Expression<Func<TUser, bool>> predicate)
    {
        return All.FirstOrDefault(predicate) ?? (TUser)User.Default;
    }

    public async Task<TUser> GetAsync(Expression<Func<TUser, bool>> predicate)
    {
        return await All.FirstOrDefaultAsync(predicate) ?? (TUser)User.Default;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _bankAccountRepository?.Dispose();
        _bankRepository?.Dispose();
        _cardRepository?.Dispose();
        _applicationContext?.Dispose();

        _disposed = true;
    }
}