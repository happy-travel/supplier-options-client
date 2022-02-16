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
}