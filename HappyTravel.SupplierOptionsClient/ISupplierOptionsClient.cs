using CSharpFunctionalExtensions;
using HappyTravel.SupplierOptionsClient.Models;

namespace HappyTravel.SupplierOptionsClient;

public interface ISupplierOptionsClient
{
    Task<Result<List<SlimSupplier>>> GetAll();
    Task<Result<RichSupplier>> Get(string code);
    Task<Result> Add(RichSupplier supplier);
    Task<Result> Modify(string code, RichSupplier supplier);
    Task<Result> Delete(string code);
    Task<Result> Activate(string code, string reason);
    Task<Result> Deactivate(string code, string reason);
    Task<Result<SupplierPriorityByTypes>> GetPriorities();
    Task<Result> ModifyPriorities(SupplierPriorityByTypes priorities);
    Task<Result> SetEnablementState(string code, EnablementState state, string reason);
}