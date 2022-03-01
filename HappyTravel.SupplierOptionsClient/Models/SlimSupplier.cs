﻿namespace HappyTravel.SupplierOptionsClient.Models;

public record SlimSupplier
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public string ConnectorUrl { get; init; } = string.Empty;
    public string? ConnectorGrpcEndpoint { get; set; }
    public bool IsMultiRoomFlowSupported { get; init; }
    public Dictionary<string, string>? CustomHeaders { get; init; }
}
